using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo02
{
    [BurstCompile]
    public partial struct BallResetSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ResetJob
            {
                Tick = (uint)math.max(1, (int)math.floor(SystemAPI.Time.ElapsedTime * 60.0))
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(BallTag))]
        private partial struct ResetJob : IJobEntity
        {
            public uint Tick;

            private void Execute(
                [EntityIndexInQuery] int entityIndex,
                ref LocalTransform transform,
                ref PhysicsVelocity velocity,
                in ResetHeight resetHeight,
                in SpawnArea spawnArea)
            {
                if (transform.Position.y >= resetHeight.Value)
                {
                    return;
                }

                uint seed = math.hash(new uint3(spawnArea.RandomSeed + 1u, (uint)entityIndex + 1u, Tick + 1u));
                seed = seed == 0u ? 1u : seed;
                Random random = new Random(seed);
                float3 halfSize = spawnArea.Size * 0.5f;

                transform.Position = spawnArea.Center + new float3(
                    random.NextFloat(-halfSize.x, halfSize.x),
                    halfSize.y,
                    random.NextFloat(-halfSize.z, halfSize.z));
                velocity.Linear = new float3(
                    random.NextFloat(-1.5f, 1.5f),
                    0f,
                    random.NextFloat(-1.5f, 1.5f));
                velocity.Angular = random.NextFloat3Direction() * random.NextFloat(0.5f, 3f);
            }
        }
    }
}
