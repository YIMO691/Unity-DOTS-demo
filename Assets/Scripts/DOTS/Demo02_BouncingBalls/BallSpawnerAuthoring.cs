using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UnityDotsDemo.Demo02
{
    public sealed class BallSpawnerAuthoring : MonoBehaviour
    {
        public GameObject BallPrefab;
        [Min(1)] public int Count = 200;
        public Vector3 SpawnCenter = new Vector3(0f, 12f, 0f);
        public Vector3 SpawnSize = new Vector3(18f, 8f, 18f);
        public float ResetY = -6f;
        [Min(1)] public int RandomSeed = 777;

        private sealed class BallSpawnerBaker : Baker<BallSpawnerAuthoring>
        {
            public override void Bake(BallSpawnerAuthoring authoring)
            {
                if (authoring.BallPrefab == null)
                {
                    return;
                }

                Entity spawnerEntity = GetEntity(TransformUsageFlags.None);
                Entity ballPrefab = GetEntity(authoring.BallPrefab, TransformUsageFlags.Dynamic);

                AddComponent(spawnerEntity, new BallSpawnerConfig
                {
                    BallPrefab = ballPrefab,
                    Count = math.max(1, authoring.Count),
                    SpawnCenter = authoring.SpawnCenter,
                    SpawnSize = math.max(new float3(1f), (float3)authoring.SpawnSize),
                    ResetY = authoring.ResetY,
                    RandomSeed = (uint)math.max(1, authoring.RandomSeed)
                });
            }
        }
    }
}
