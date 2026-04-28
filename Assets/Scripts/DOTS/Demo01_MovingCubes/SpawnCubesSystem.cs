using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo01
{
    [BurstCompile]
    public partial struct SpawnCubesSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CubeSpawnerConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (configRef, spawnerEntity) in
                     SystemAPI.Query<RefRO<CubeSpawnerConfig>>().WithEntityAccess())
            {
                CubeSpawnerConfig config = configRef.ValueRO;
                Random random = Random.CreateFromIndex(config.RandomSeed);

                float startX = -(config.CountX - 1) * config.Spacing * 0.5f;
                float startZ = -(config.CountZ - 1) * config.Spacing * 0.5f;

                for (int z = 0; z < config.CountZ; z++)
                {
                    for (int x = 0; x < config.CountX; x++)
                    {
                        Entity cube = ecb.Instantiate(config.CubePrefab);
                        float angle = random.NextFloat(0f, math.PI * 2f);
                        float speed = random.NextFloat(config.MinSpeed, config.MaxSpeed);
                        float3 direction = new float3(math.cos(angle), 0f, math.sin(angle));
                        float3 position = new float3(
                            startX + x * config.Spacing,
                            0f,
                            startZ + z * config.Spacing);

                        ecb.SetComponent(
                            cube,
                            LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
                        ecb.AddComponent(cube, new MoveSpeed { Value = speed });
                        ecb.AddComponent(cube, new MoveDirection { Value = direction });
                        ecb.AddComponent(cube, new WrapArea { HalfExtents = config.AreaHalfSize });
                    }
                }

                ecb.DestroyEntity(spawnerEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
