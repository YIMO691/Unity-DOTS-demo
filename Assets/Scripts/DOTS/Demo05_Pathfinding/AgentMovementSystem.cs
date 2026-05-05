using DOTSDemo.Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo05
{
    [BurstCompile]
    public partial struct AgentMovementSystem : ISystem
    {
        private EntityQuery _gridQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _gridQuery = SystemAPI.QueryBuilder()
                .WithAll<FlowFieldGrid, PathTarget>()
                .Build();
            state.RequireForUpdate(_gridQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity gridEntity = _gridQuery.GetSingletonEntity();
            FlowFieldGrid grid = SystemAPI.GetComponent<FlowFieldGrid>(gridEntity);
            BufferLookup<FlowFieldCell> flowFieldLookup = SystemAPI.GetBufferLookup<FlowFieldCell>(true);

            state.Dependency = new AgentMoveJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Grid = grid,
                FlowFieldLookup = flowFieldLookup,
                GridEntity = gridEntity
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(AgentTag))]
        private partial struct AgentMoveJob : IJobEntity
        {
            public float DeltaTime;
            public FlowFieldGrid Grid;
            public BufferLookup<FlowFieldCell> FlowFieldLookup;
            public Entity GridEntity;

            private void Execute(ref LocalTransform transform, in MoveSpeed speed)
            {
                if (!FlowFieldLookup.HasBuffer(GridEntity))
                {
                    return;
                }

                DynamicBuffer<FlowFieldCell> cells = FlowFieldLookup[GridEntity];
                if (cells.Length == 0)
                {
                    return;
                }

                float3 position = transform.Position;
                float cellX = (position.x - Grid.WorldOrigin.x) / Grid.CellSize;
                float cellZ = (position.z - Grid.WorldOrigin.z) / Grid.CellSize;
                int cx = (int)math.floor(cellX);
                int cz = (int)math.floor(cellZ);

                float2 flowDir = float2.zero;
                if (cx >= 0 && cx < Grid.Width && cz >= 0 && cz < Grid.Height)
                {
                    int cellIndex = cz * Grid.Width + cx;
                    flowDir = cells[cellIndex].Direction;
                }

                if (math.lengthsq(flowDir) < 0.0001f)
                {
                    // Agent reached target or is outside grid — drift toward grid center
                    float2 gridCenter = new float2(
                        Grid.WorldOrigin.x + Grid.Width * Grid.CellSize * 0.5f,
                        Grid.WorldOrigin.z + Grid.Height * Grid.CellSize * 0.5f);
                    flowDir = math.normalizesafe(gridCenter - new float2(position.x, position.z));
                }

                float3 newPosition = position;
                newPosition.x += flowDir.x * speed.Value * DeltaTime;
                newPosition.z += flowDir.y * speed.Value * DeltaTime;

                transform.Position = newPosition;
                transform.Rotation = quaternion.LookRotationSafe(
                    new float3(flowDir.x, 0f, flowDir.y), math.up());
            }
        }
    }
}
