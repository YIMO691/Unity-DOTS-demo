using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo04
{
    [UpdateAfter(typeof(WaveSpawnerSystem))]
    public partial struct EnemyMovementSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            DynamicBuffer<Waypoint> waypoints = default;
            bool hasPath = false;

            foreach (DynamicBuffer<Waypoint> waypointBuffer in SystemAPI.Query<DynamicBuffer<Waypoint>>())
            {
                waypoints = waypointBuffer;
                hasPath = true;
                break;
            }

            if (!hasPath || waypoints.Length == 0)
            {
                return;
            }

            float deltaTime = SystemAPI.Time.DeltaTime;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (transformRef, waypointIndexRef, speedRef, enemyEntity) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRW<WaypointIndex>, RefRO<MoveSpeed>>()
                         .WithAll<EnemyTag>()
                         .WithEntityAccess())
            {
                int targetIndex = waypointIndexRef.ValueRO.Value;
                if (targetIndex >= waypoints.Length)
                {
                    ecb.DestroyEntity(enemyEntity);
                    continue;
                }

                float3 position = transformRef.ValueRO.Position;
                float3 target = waypoints[targetIndex].Position;
                float3 toTarget = target - position;
                float distance = math.length(toTarget);
                float step = speedRef.ValueRO.Value * deltaTime;

                if (distance <= step || distance <= 0.05f)
                {
                    transformRef.ValueRW.Position = target;
                    waypointIndexRef.ValueRW.Value = targetIndex + 1;

                    if (targetIndex + 1 >= waypoints.Length)
                    {
                        ecb.DestroyEntity(enemyEntity);
                    }

                    continue;
                }

                float3 direction = toTarget / distance;
                transformRef.ValueRW.Position = position + direction * step;
                transformRef.ValueRW.Rotation = quaternion.LookRotationSafe(direction, math.up());
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
