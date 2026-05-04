using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo03
{
    [BurstCompile]
    public partial struct BoidMovementSystem : ISystem
    {
        private EntityQuery _boidQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _boidQuery = SystemAPI.QueryBuilder()
                .WithAll<BoidTag, LocalTransform, BoidVelocity, BoidSettings, SimulationBounds>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (RefRO<BoidSimulationModeData> modeRef in SystemAPI.Query<RefRO<BoidSimulationModeData>>())
            {
                if (modeRef.ValueRO.Mode != BoidSimulationMode.Basic)
                {
                    return;
                }

                break;
            }

            int count = _boidQuery.CalculateEntityCount();
            if (count == 0)
            {
                return;
            }

            NativeArray<LocalTransform> positions = _boidQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            NativeArray<BoidVelocity> velocities = _boidQuery.ToComponentDataArray<BoidVelocity>(Allocator.TempJob);

            var job = new BoidMoveJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Positions = positions,
                Velocities = velocities
            };

            var handle = job.ScheduleParallel(_boidQuery, state.Dependency);
            handle = positions.Dispose(handle);
            handle = velocities.Dispose(handle);
            state.Dependency = handle;
        }

        [BurstCompile]
        [WithAll(typeof(BoidTag))]
        private partial struct BoidMoveJob : IJobEntity
        {
            public float DeltaTime;

            [ReadOnly] public NativeArray<LocalTransform> Positions;
            [ReadOnly] public NativeArray<BoidVelocity> Velocities;

            private void Execute(
                [EntityIndexInQuery] int entityIndex,
                ref LocalTransform transform,
                ref BoidVelocity velocity,
                in BoidSettings settings,
                in SimulationBounds bounds)
            {
                float3 position = transform.Position;
                float3 currentVelocity = velocity.Value;
                float3 separation = float3.zero;
                float3 alignment = float3.zero;
                float3 cohesion = float3.zero;
                int neighborCount = 0;
                int sampleCount = math.min(8, math.max(0, Positions.Length - 1));

                for (int i = 1; i <= sampleCount; i++)
                {
                    int sampleIndex = (entityIndex + i * 31) % Positions.Length;
                    float3 otherPosition = Positions[sampleIndex].Position;
                    float3 toOther = otherPosition - position;
                    float distance = math.length(toOther);

                    if (distance <= 0.0001f || distance > settings.NeighborRadius)
                    {
                        continue;
                    }

                    if (distance < settings.SeparationRadius)
                    {
                        separation -= toOther / math.max(distance, 0.0001f);
                    }

                    alignment += Velocities[sampleIndex].Value;
                    cohesion += otherPosition;
                    neighborCount++;
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

                steering += boundsSteer * settings.BoundsWeight;

                currentVelocity += steering * DeltaTime;
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
