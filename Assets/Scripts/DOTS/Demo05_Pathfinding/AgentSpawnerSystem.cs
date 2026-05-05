using DOTSDemo.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo05
{
    [BurstCompile]
    public partial struct AgentSpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AgentSpawnerConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var helper = new SpawnerHelper(state.EntityManager, Allocator.Temp);

            foreach (var (configRef, spawnerEntity) in
                     SystemAPI.Query<RefRO<AgentSpawnerConfig>>().WithEntityAccess())
            {
                AgentSpawnerConfig config = configRef.ValueRO;
                Random random = Random.CreateFromIndex(config.RandomSeed);

                for (int i = 0; i < config.Count; i++)
                {
                    float2 circle = random.NextFloat2Direction() *
                                    random.NextFloat(0f, config.SpawnHalfExtents.x);
                    float3 position = config.SpawnCenter + new float3(circle.x, 0f, circle.y);

                    Entity agent = helper.Ecb.Instantiate(config.AgentPrefab);
                    helper.Ecb.SetComponent(agent,
                        LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
                    helper.Ecb.AddComponent<AgentTag>(agent);
                    helper.Ecb.AddComponent(agent, new MoveSpeed { Value = config.MoveSpeed });
                }

                helper.DestroySpawner(spawnerEntity);
            }
        }
    }
}
