using DOTSDemo.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo03
{
    [BurstCompile]
    public partial struct SpatialHashBoidSystem : ISystem
    {
        private EntityQuery _boidQuery;
        private NativeParallelMultiHashMap<int2, int> _hashMap;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _boidQuery = SystemAPI.QueryBuilder()
                .WithAll<BoidTag, LocalTransform, Velocity, BoidSettings, SimulationBounds>()
                .Build();
            _hashMap = new NativeParallelMultiHashMap<int2, int>(1024, Allocator.Persistent);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (_hashMap.IsCreated)
            {
                _hashMap.Dispose();
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            bool spatialHashMode = false;
            float requestedCellSize = 0f;
            foreach (RefRO<BoidSimulationModeData> modeRef in SystemAPI.Query<RefRO<BoidSimulationModeData>>())
            {
                requestedCellSize = modeRef.ValueRO.CellSize;
                spatialHashMode = modeRef.ValueRO.Mode == BoidSimulationMode.SpatialHash;
                break;
            }

            if (!spatialHashMode)
            {
                return;
            }

            int count = _boidQuery.CalculateEntityCount();
            if (count == 0)
            {
                return;
            }

            state.Dependency.Complete();
            int requiredCapacity = math.max(1024, count * 2);
            if (!_hashMap.IsCreated || _hashMap.Capacity < requiredCapacity)
            {
                if (_hashMap.IsCreated)
                {
                    _hashMap.Dispose();
                }

                _hashMap = new NativeParallelMultiHashMap<int2, int>(requiredCapacity, Allocator.Persistent);
            }

            _hashMap.Clear();

            NativeArray<LocalTransform> positions = _boidQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            NativeArray<Velocity> velocities = _boidQuery.ToComponentDataArray<Velocity>(Allocator.TempJob);
            float fallbackCellSize = 4f;
            foreach (RefRO<BoidSettings> settingsRef in SystemAPI.Query<RefRO<BoidSettings>>().WithAll<BoidTag>())
            {
                fallbackCellSize = math.max(0.1f, settingsRef.ValueRO.NeighborRadius);
                break;
            }

            float cellSize = requestedCellSize > 0f ? requestedCellSize : fallbackCellSize;

            SpatialHashInsertJob insertJob = new SpatialHashInsertJob
            {
                HashMap = _hashMap.AsParallelWriter(),
                CellSize = cellSize
            };

            SpatialHashBoidJob boidJob = new SpatialHashBoidJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                HashMap = _hashMap,
                Positions = positions,
                Velocities = velocities,
                CellSize = cellSize
            };

            var insertHandle = insertJob.ScheduleParallel(_boidQuery, state.Dependency);
            var boidHandle = boidJob.ScheduleParallel(_boidQuery, insertHandle);
            boidHandle = positions.Dispose(boidHandle);
            boidHandle = velocities.Dispose(boidHandle);
            state.Dependency = boidHandle;
        }

        public static int2 GetCell(float3 position, float cellSize)
        {
            float safeCellSize = math.max(0.0001f, cellSize);
            return new int2(
                (int)math.floor(position.x / safeCellSize),
                (int)math.floor(position.z / safeCellSize));
        }

        [BurstCompile]
        public partial struct SpatialHashInsertJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int2, int>.ParallelWriter HashMap;
            public float CellSize;

            private void Execute([EntityIndexInQuery] int entityIndex, in LocalTransform transform, in BoidTag tag)
            {
                HashMap.Add(GetCell(transform.Position, CellSize), entityIndex);
            }
        }

        [BurstCompile]
        [WithAll(typeof(BoidTag))]
        public partial struct SpatialHashBoidJob : IJobEntity
        {
            public float DeltaTime;
            [ReadOnly] public NativeParallelMultiHashMap<int2, int> HashMap;
            [ReadOnly] public NativeArray<LocalTransform> Positions;
            [ReadOnly] public NativeArray<Velocity> Velocities;
            public float CellSize;

            private void Execute(
                [EntityIndexInQuery] int entityIndex,
                ref LocalTransform transform,
                ref Velocity velocity,
                in BoidSettings settings,
                in SimulationBounds bounds)
            {
                float3 position = transform.Position;
                float3 currentVelocity = velocity.Value;
                float3 separation = float3.zero;
                float3 alignment = float3.zero;
                float3 cohesion = float3.zero;
                int neighborCount = 0;
                int2 centerCell = GetCell(position, CellSize);
                float neighborRadiusSq = settings.NeighborRadius * settings.NeighborRadius;
                float separationRadiusSq = settings.SeparationRadius * settings.SeparationRadius;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        int2 cell = centerCell + new int2(dx, dz);
                        if (!HashMap.TryGetFirstValue(cell, out int neighborIndex, out NativeParallelMultiHashMapIterator<int2> iterator))
                        {
                            continue;
                        }

                        do
                        {
                            if (neighborIndex == entityIndex ||
                                neighborIndex < 0 ||
                                neighborIndex >= Positions.Length)
                            {
                                continue;
                            }

                            float3 otherPosition = Positions[neighborIndex].Position;
                            float3 toOther = otherPosition - position;
                            float distanceSq = math.lengthsq(toOther);
                            if (distanceSq <= 0.0001f || distanceSq > neighborRadiusSq)
                            {
                                continue;
                            }

                            if (distanceSq < separationRadiusSq)
                            {
                                separation -= toOther / math.max(math.sqrt(distanceSq), 0.0001f);
                            }

                            alignment += Velocities[neighborIndex].Value;
                            cohesion += otherPosition;
                            neighborCount++;
                        }
                        while (HashMap.TryGetNextValue(out neighborIndex, ref iterator));
                    }
                }

                float3 steering = float3.zero;
                if (neighborCount > 0)
                {
                    float invCount = 1f / neighborCount;
                    alignment = math.normalizesafe(alignment * invCount) * settings.MaxSpeed - currentVelocity;
                    cohesion = math.normalizesafe((cohesion * invCount) - position) * settings.MaxSpeed - currentVelocity;
                    separation = math.normalizesafe(separation) * settings.MaxSpeed - currentVelocity;

                    steering += separation * settings.SeparationWeight;
                    steering += alignment * settings.AlignmentWeight;
                    steering += cohesion * settings.CohesionWeight;
                }

                float3 min = bounds.Center - bounds.Extents;
                float3 max = bounds.Center + bounds.Extents;
                float3 boundsSteer = float3.zero;

                if (position.x < min.x) boundsSteer.x += 1f;
                if (position.x > max.x) boundsSteer.x -= 1f;
                if (position.y < min.y) boundsSteer.y += 1f;
                if (position.y > max.y) boundsSteer.y -= 1f;
                if (position.z < min.z) boundsSteer.z += 1f;
                if (position.z > max.z) boundsSteer.z -= 1f;

                currentVelocity += (steering + boundsSteer * settings.BoundsWeight) * DeltaTime;
                float speed = math.clamp(math.length(currentVelocity), settings.MinSpeed, settings.MaxSpeed);
                currentVelocity = math.normalizesafe(currentVelocity, new float3(0f, 0f, 1f)) * speed;

                position += currentVelocity * DeltaTime;
                transform.Position = position;
                transform.Rotation = quaternion.LookRotationSafe(currentVelocity, math.up());
                velocity.Value = currentVelocity;
            }
        }
    }
}
