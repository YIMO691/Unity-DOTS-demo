using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UnityDotsDemo.Demo03
{
    public sealed class BoidSpawnerAuthoring : MonoBehaviour
    {
        public GameObject BoidPrefab;
        [Min(1)] public int Count = 500;
        public Vector3 Center = Vector3.zero;
        public Vector3 BoundsExtents = new Vector3(24f, 8f, 24f);
        [Min(0.1f)] public float MinSpeed = 3f;
        [Min(0.1f)] public float MaxSpeed = 7f;
        [Min(0.1f)] public float NeighborRadius = 4f;
        [Min(0.1f)] public float SeparationRadius = 1.2f;
        public float SeparationWeight = 1.7f;
        public float AlignmentWeight = 0.7f;
        public float CohesionWeight = 0.6f;
        public float BoundsWeight = 4f;
        [Min(1)] public int RandomSeed = 4242;

        private sealed class Baker : Baker<BoidSpawnerAuthoring>
        {
            public override void Bake(BoidSpawnerAuthoring authoring)
            {
                if (authoring.BoidPrefab == null)
                {
                    return;
                }

                Entity spawnerEntity = GetEntity(TransformUsageFlags.None);
                Entity boidPrefab = GetEntity(authoring.BoidPrefab, TransformUsageFlags.Dynamic);

                AddComponent(spawnerEntity, new BoidSpawnerConfig
                {
                    BoidPrefab = boidPrefab,
                    Count = math.max(1, authoring.Count),
                    Center = authoring.Center,
                    BoundsExtents = math.max(new float3(1f), (float3)authoring.BoundsExtents),
                    Settings = new BoidSettings
                    {
                        MinSpeed = math.max(0.1f, authoring.MinSpeed),
                        MaxSpeed = math.max(authoring.MinSpeed, authoring.MaxSpeed),
                        NeighborRadius = math.max(0.1f, authoring.NeighborRadius),
                        SeparationRadius = math.max(0.1f, authoring.SeparationRadius),
                        SeparationWeight = authoring.SeparationWeight,
                        AlignmentWeight = authoring.AlignmentWeight,
                        CohesionWeight = authoring.CohesionWeight,
                        BoundsWeight = authoring.BoundsWeight
                    },
                    RandomSeed = (uint)math.max(1, authoring.RandomSeed)
                });
            }
        }
    }
}
