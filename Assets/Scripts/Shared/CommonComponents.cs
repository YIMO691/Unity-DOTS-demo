using Unity.Entities;
using Unity.Mathematics;

namespace DOTSDemo.Shared
{
    public struct MoveSpeed : IComponentData
    {
        public float Value;
    }

    public struct Velocity : IComponentData
    {
        public float3 Value;
    }
}
