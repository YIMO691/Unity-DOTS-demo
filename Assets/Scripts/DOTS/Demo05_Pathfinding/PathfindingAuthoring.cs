using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UnityDotsDemo.Demo05
{
    public sealed class PathfindingAuthoring : MonoBehaviour
    {
        [Min(2)] public int GridWidth = 40;
        [Min(2)] public int GridHeight = 40;
        [Min(0.1f)] public float CellSize = 1f;
        public Vector3 WorldOrigin = new Vector3(-20f, 0f, -20f);
        public Vector3 TargetPosition = new Vector3(0f, 0f, 0f);

        private sealed class PathfindingBaker : Baker<PathfindingAuthoring>
        {
            public override void Bake(PathfindingAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new FlowFieldGrid
                {
                    Width = math.max(2, authoring.GridWidth),
                    Height = math.max(2, authoring.GridHeight),
                    CellSize = math.max(0.1f, authoring.CellSize),
                    WorldOrigin = authoring.WorldOrigin
                });

                AddComponent(entity, new PathTarget
                {
                    Position = authoring.TargetPosition
                });

                AddBuffer<FlowFieldCell>(entity);
            }
        }
    }
}
