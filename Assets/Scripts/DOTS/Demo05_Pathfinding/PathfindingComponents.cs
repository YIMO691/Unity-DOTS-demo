using Unity.Entities;
using Unity.Mathematics;

namespace UnityDotsDemo.Demo05
{
    public struct FlowFieldGrid : IComponentData
    {
        public int Width;
        public int Height;
        public float CellSize;
        public float3 WorldOrigin;
    }

    public struct FlowFieldCell : IBufferElementData
    {
        public float2 Direction;
        public byte Cost;
    }

    public struct PathTarget : IComponentData
    {
        public float3 Position;
    }

    public struct AgentTag : IComponentData
    {
    }

    public struct AgentSpawnerConfig : IComponentData
    {
        public Entity AgentPrefab;
        public int Count;
        public float3 SpawnCenter;
        public float2 SpawnHalfExtents;
        public float MoveSpeed;
        public uint RandomSeed;
    }
}
