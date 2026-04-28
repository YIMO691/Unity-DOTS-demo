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
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (configRef, spawnerEntity) in
                     SystemAPI.Query<RefRO<BoidSpawnerConfig>>().WithEntityAccess())
            {
                BoidSpawnerConfig config = configRef.ValueRO;
                Random random = Random.CreateFromIndex(config.RandomSeed);

                for (int i = 0; i < config.Count; i++)
                {
                    Entity boid = ecb.Instantiate(config.BoidPrefab);
                    float3 position = config.Center + new float3(
                        random.NextFloat(-config.BoundsExtents.x, config.BoundsExtents.x),
                        random.NextFloat(-config.BoundsExtents.y, config.BoundsExtents.y),
                        random.NextFloat(-config.BoundsExtents.z, config.BoundsExtents.z));
                    float3 velocity = random.NextFloat3Direction() *
                                      random.NextFloat(config.Settings.MinSpeed, config.Settings.MaxSpeed);

                    ecb.SetComponent(boid, LocalTransform.FromPositionRotationScale(
                        position,
                        quaternion.LookRotationSafe(velocity, math.up()),
                        1f));
                    ecb.AddComponent<BoidTag>(boid);
                    ecb.AddComponent(boid, new BoidVelocity { Value = velocity });
                    ecb.AddComponent(boid, config.Settings);
                    ecb.AddComponent(boid, new SimulationBounds
                    {
                        Center = config.Center,
                        Extents = config.BoundsExtents
                    });
                }

                ecb.DestroyEntity(spawnerEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
