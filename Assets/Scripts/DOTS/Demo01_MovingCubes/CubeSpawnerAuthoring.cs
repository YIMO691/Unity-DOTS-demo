using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UnityDotsDemo.Demo01
{
    public sealed class CubeSpawnerAuthoring : MonoBehaviour
    {
        public GameObject CubePrefab;
        [Min(1)] public int CountX = 25;
        [Min(1)] public int CountZ = 40;
        [Min(0.1f)] public float Spacing = 1.4f;
        [Min(0f)] public float MinSpeed = 1.5f;
        [Min(0f)] public float MaxSpeed = 4.5f;
        public Vector2 AreaHalfSize = new Vector2(22f, 18f);
        [Min(1)] public int RandomSeed = 12345;

        private sealed class Baker : Baker<CubeSpawnerAuthoring>
        {
            public override void Bake(CubeSpawnerAuthoring authoring)
            {
                if (authoring.CubePrefab == null)
                {
                    return;
                }

                Entity spawnerEntity = GetEntity(TransformUsageFlags.None);
                Entity cubePrefab = GetEntity(authoring.CubePrefab, TransformUsageFlags.Dynamic);

                AddComponent(spawnerEntity, new CubeSpawnerConfig
                {
                    CubePrefab = cubePrefab,
                    CountX = math.max(1, authoring.CountX),
                    CountZ = math.max(1, authoring.CountZ),
                    Spacing = math.max(0.1f, authoring.Spacing),
                    MinSpeed = math.max(0f, authoring.MinSpeed),
                    MaxSpeed = math.max(authoring.MinSpeed, authoring.MaxSpeed),
                    AreaHalfSize = new float2(
                        math.max(1f, authoring.AreaHalfSize.x),
                        math.max(1f, authoring.AreaHalfSize.y)),
                    RandomSeed = (uint)math.max(1, authoring.RandomSeed)
                });
            }
        }
    }
}
