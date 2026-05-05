using DOTSDemo.Shared;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UnityDotsDemo.Demo05
{
    public sealed class AgentSpawnerAuthoring : MonoBehaviour
    {
        public GameObject AgentPrefab;
        [Min(1)] public int Count = 200;
        [Min(0.1f)] public float MoveSpeed = 3f;
        [Min(1f)] public float SpawnRadius = 18f;
        [Min(1)] public int RandomSeed = 42;

        private sealed class AgentSpawnerBaker : Baker<AgentSpawnerAuthoring>
        {
            public override void Bake(AgentSpawnerAuthoring authoring)
            {
                if (authoring.AgentPrefab == null)
                {
                    return;
                }

                Entity spawnerEntity = GetEntity(TransformUsageFlags.None);
                Entity agentPrefab = GetEntity(authoring.AgentPrefab, TransformUsageFlags.Dynamic);

                AddComponent(spawnerEntity, new AgentSpawnerConfig
                {
                    AgentPrefab = agentPrefab,
                    Count = math.max(1, authoring.Count),
                    SpawnCenter = float3.zero,
                    SpawnHalfExtents = new float2(math.max(1f, authoring.SpawnRadius), math.max(1f, authoring.SpawnRadius)),
                    MoveSpeed = math.max(0.1f, authoring.MoveSpeed),
                    RandomSeed = (uint)math.max(1, authoring.RandomSeed)
                });
            }
        }
    }
}
