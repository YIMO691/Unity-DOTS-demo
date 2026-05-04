using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo02
{
    public partial struct BallSpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BallSpawnerConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (configRef, spawnerEntity) in
                     SystemAPI.Query<RefRO<BallSpawnerConfig>>().WithEntityAccess())
            {
                BallSpawnerConfig config = configRef.ValueRO;
                Random random = Random.CreateFromIndex(config.RandomSeed);
                float3 halfSize = config.SpawnSize * 0.5f;

                for (int i = 0; i < config.Count; i++)
                {
                    Entity ball = ecb.Instantiate(config.BallPrefab);
                    float3 offset = new float3(
                        random.NextFloat(-halfSize.x, halfSize.x),
                        random.NextFloat(-halfSize.y, halfSize.y),
                        random.NextFloat(-halfSize.z, halfSize.z));
                    float3 position = config.SpawnCenter + offset;

                    ecb.SetComponent(ball, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
                    ecb.SetComponent(ball, new PhysicsVelocity
                    {
                        Linear = new float3(
                            random.NextFloat(-1.5f, 1.5f),
                            random.NextFloat(-0.25f, 0.25f),
                            random.NextFloat(-1.5f, 1.5f)),
                        Angular = random.NextFloat3Direction() * random.NextFloat(0.5f, 3f)
                    });
                    ecb.AddComponent<BallTag>(ball);
                    ecb.AddComponent(ball, new ResetHeight { Value = config.ResetY });
                    ecb.AddComponent(ball, new SpawnArea
                    {
                        Center = config.SpawnCenter,
                        Size = config.SpawnSize,
                        RandomSeed = config.RandomSeed
                    });
                }

                ecb.DestroyEntity(spawnerEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
