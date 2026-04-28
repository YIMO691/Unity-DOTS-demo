using Unity.Entities;
using Unity.Mathematics;

namespace UnityDotsDemo.Demo03
{
    public struct BoidTag : IComponentData
    {
    }

    public struct BoidVelocity : IComponentData
    {
        public float3 Value;
    }

    public struct BoidSettings : IComponentData
    {
        public float MinSpeed;
        public float MaxSpeed;
        public float NeighborRadius;
        public float SeparationRadius;
        public float SeparationWeight;
        public float AlignmentWeight;
        public float CohesionWeight;
        public float BoundsWeight;
    }

    public struct BoidSpawnerConfig : IComponentData
    {
        public Entity BoidPrefab;
        public int Count;
        public float3 Center;
        public float3 BoundsExtents;
        public BoidSettings Settings;
        public uint RandomSeed;
    }

    public struct SimulationBounds : IComponentData
    {
        public float3 Center;
        public float3 Extents;
    }
}
