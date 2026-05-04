using Unity.Entities;

namespace UnityDotsDemo.Demo03
{
    public enum BoidSimulationMode : byte
    {
        Basic,
        SpatialHash
    }

    public struct BoidSimulationModeData : IComponentData
    {
        public BoidSimulationMode Mode;
        public float CellSize;
    }
}
