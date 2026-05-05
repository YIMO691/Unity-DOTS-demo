using DOTSDemo.Shared;
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
            using var helper = new SpawnerHelper(state.EntityManager, Allocator.Temp);

            foreach (var (configRef, spawnerEntity) in
                     SystemAPI.Query<RefRO<CubeSpawnerConfig>>().WithEntityAccess())
            {
                CubeSpawnerConfig config = configRef.ValueRO;
                Random random = Random.CreateFromIndex(config.RandomSeed);

                int columns = math.max(1, config.CountX);
                int rows = math.max(1, (config.SpawnCount + columns - 1) / columns);
                float startX = -(columns - 1) * config.Spacing * 0.5f;
                float startZ = -(rows - 1) * config.Spacing * 0.5f;

                for (int i = 0; i < config.SpawnCount; i++)
                {
                    int x = i % columns;
                    int z = i / columns;
                    Entity cube = helper.Ecb.Instantiate(config.CubePrefab);
                    float angle = random.NextFloat(0f, math.PI * 2f);
                    float speed = random.NextFloat(config.MinSpeed, config.MaxSpeed);
                    float3 direction = new float3(math.cos(angle), 0f, math.sin(angle));
                    float3 position = new float3(
                        startX + x * config.Spacing,
                        0f,
                        startZ + z * config.Spacing);

                    helper.Ecb.SetComponent(
                        cube,
                        LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
                    helper.Ecb.AddComponent(cube, new MoveSpeed { Value = speed });
                    helper.Ecb.AddComponent(cube, new Velocity { Value = direction });
                    helper.Ecb.AddComponent(cube, new WrapArea { HalfExtents = config.AreaHalfSize });
                }

                helper.DestroySpawner(spawnerEntity);
            }
        }
    }
}
