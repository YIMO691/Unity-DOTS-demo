using Unity.Entities;
using Unity.Mathematics;

namespace UnityDotsDemo.Demo01
{
    public struct CubeSpawnerConfig : IComponentData
    {
        public Entity CubePrefab;
        public int CountX;
        public int CountZ;
        public float Spacing;
        public float MinSpeed;
        public float MaxSpeed;
        public float2 AreaHalfSize;
        public uint RandomSeed;
    }

    public struct MoveSpeed : IComponentData
    {
        public float Value;
    }

    public struct MoveDirection : IComponentData
    {
        public float3 Value;
    }

    public struct WrapArea : IComponentData
    {
        public float2 HalfExtents;
    }
}
