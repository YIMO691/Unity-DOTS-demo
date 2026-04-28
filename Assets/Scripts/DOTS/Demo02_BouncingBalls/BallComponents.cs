using Unity.Entities;
using Unity.Mathematics;

namespace UnityDotsDemo.Demo02
{
    public struct BallTag : IComponentData
    {
    }

    public struct BallSpawnerConfig : IComponentData
    {
        public Entity BallPrefab;
        public int Count;
        public float3 SpawnCenter;
        public float3 SpawnSize;
        public float ResetY;
        public uint RandomSeed;
    }

    public struct ResetHeight : IComponentData
    {
        public float Value;
    }

    public struct SpawnArea : IComponentData
    {
        public float3 Center;
        public float3 Size;
        public uint RandomSeed;
    }
}
