using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace UnityDotsDemo.Demo05
{
    public partial struct FlowFieldSystem : ISystem
    {
        private EntityQuery _gridQuery;

        public void OnCreate(ref SystemState state)
        {
            _gridQuery = SystemAPI.QueryBuilder()
                .WithAll<FlowFieldGrid, PathTarget>()
                .Build();
            state.RequireForUpdate(_gridQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            Entity gridEntity = _gridQuery.GetSingletonEntity();
            FlowFieldGrid grid = SystemAPI.GetComponent<FlowFieldGrid>(gridEntity);
            PathTarget target = SystemAPI.GetComponent<PathTarget>(gridEntity);
            DynamicBuffer<FlowFieldCell> cells = SystemAPI.GetBuffer<FlowFieldCell>(gridEntity);

            int totalCells = grid.Width * grid.Height;
            if (cells.Length != totalCells)
            {
                cells.Resize(totalCells, NativeArrayOptions.ClearMemory);
            }

            BuildFlowField(ref state, grid, target, cells);
        }

        private static void BuildFlowField(
            ref SystemState state,
            FlowFieldGrid grid,
            PathTarget target,
            DynamicBuffer<FlowFieldCell> cells)
        {
            int width = grid.Width;
            int height = grid.Height;
            int totalCells = width * height;

            // Initialize all cells
            for (int i = 0; i < totalCells; i++)
            {
                cells[i] = new FlowFieldCell { Cost = byte.MaxValue, Direction = float2.zero };
            }

            // Convert target position to grid cell
            int2 targetCell = WorldToCell(grid, target.Position);
            if (targetCell.x < 0 || targetCell.x >= width ||
                targetCell.y < 0 || targetCell.y >= height)
            {
                return;
            }

            // BFS from target outward
            using NativeList<int> queue = new NativeList<int>(totalCells, Allocator.Temp);
            int targetIndex = CellToIndex(targetCell, width);
            queue.Add(targetIndex);
            cells[targetIndex] = new FlowFieldCell { Cost = 0, Direction = float2.zero };

            int queueHead = 0;
            while (queueHead < queue.Length)
            {
                int currentIndex = queue[queueHead++];
                int2 currentCell = IndexToCell(currentIndex, width);

                foreach (int2 neighborOffset in NeighborOffsets)
                {
                    int2 neighborCell = currentCell + neighborOffset;
                    if (neighborCell.x < 0 || neighborCell.x >= width ||
                        neighborCell.y < 0 || neighborCell.y >= height)
                    {
                        continue;
                    }

                    int neighborIndex = CellToIndex(neighborCell, width);
                    if (cells[neighborIndex].Cost != byte.MaxValue)
                    {
                        continue;
                    }

                    float2 currentWorldPos = CellCenterToWorld(grid, currentCell);
                    float2 neighborWorldPos = CellCenterToWorld(grid, neighborCell);
                    float2 direction = math.normalizesafe(currentWorldPos - neighborWorldPos);

                    cells[neighborIndex] = new FlowFieldCell
                    {
                        Cost = (byte)math.min(cells[currentIndex].Cost + 1, byte.MaxValue),
                        Direction = direction
                    };
                    queue.Add(neighborIndex);
                }
            }
        }

        public static int2 WorldToCell(FlowFieldGrid grid, float3 worldPos)
        {
            return new int2(
                (int)math.floor((worldPos.x - grid.WorldOrigin.x) / grid.CellSize),
                (int)math.floor((worldPos.z - grid.WorldOrigin.z) / grid.CellSize));
        }

        public static int CellToIndex(int2 cell, int width) => cell.y * width + cell.x;

        public static int2 IndexToCell(int index, int width) => new int2(index % width, index / width);

        public static float2 CellCenterToWorld(FlowFieldGrid grid, int2 cell)
        {
            return new float2(
                grid.WorldOrigin.x + (cell.x + 0.5f) * grid.CellSize,
                grid.WorldOrigin.z + (cell.y + 0.5f) * grid.CellSize);
        }

        private static readonly int2[] NeighborOffsets =
        {
            new int2(0, 1),
            new int2(0, -1),
            new int2(1, 0),
            new int2(-1, 0)
        };
    }
}
