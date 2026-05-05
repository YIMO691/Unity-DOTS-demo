using DOTSDemo.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo03
{
    public partial struct BoidSpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoidSpawnerConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            using var helper = new SpawnerHelper(state.EntityManager, Allocator.Temp);

            foreach (var (configRef, spawnerEntity) in
                     SystemAPI.Query<RefRO<BoidSpawnerConfig>>().WithEntityAccess())
            {
                BoidSpawnerConfig config = configRef.ValueRO;
                Random random = Random.CreateFromIndex(config.RandomSeed);

                for (int i = 0; i < config.Count; i++)
                {
                    Entity boid = helper.Ecb.Instantiate(config.BoidPrefab);
                    float3 position = config.Center + new float3(
                        random.NextFloat(-config.BoundsExtents.x, config.BoundsExtents.x),
                        random.NextFloat(-config.BoundsExtents.y, config.BoundsExtents.y),
                        random.NextFloat(-config.BoundsExtents.z, config.BoundsExtents.z));
                    float3 velocity = random.NextFloat3Direction() *
                                      random.NextFloat(config.Settings.MinSpeed, config.Settings.MaxSpeed);

                    helper.Ecb.SetComponent(boid, LocalTransform.FromPositionRotationScale(
                        position,
                        quaternion.LookRotationSafe(velocity, math.up()),
                        1f));
                    helper.Ecb.AddComponent<BoidTag>(boid);
                    helper.Ecb.AddComponent(boid, new Velocity { Value = velocity });
                    helper.Ecb.AddComponent(boid, config.Settings);
                    helper.Ecb.AddComponent(boid, new SimulationBounds
                    {
                        Center = config.Center,
                        Extents = config.BoundsExtents
                    });
                }

                helper.DestroySpawner(spawnerEntity);
            }
        }
    }
}
