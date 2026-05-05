using DOTSDemo.Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo01
{
    [BurstCompile]
    public partial struct MoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new MoveJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct MoveJob : IJobEntity
        {
            public float DeltaTime;

            private void Execute(
                ref LocalTransform transform,
                in MoveSpeed speed,
                in Velocity velocity,
                in WrapArea wrapArea)
            {
                float3 position = transform.Position + velocity.Value * speed.Value * DeltaTime;
                float2 halfExtents = wrapArea.HalfExtents;

                if (position.x > halfExtents.x)
                {
                    position.x = -halfExtents.x + (position.x - halfExtents.x);
                }
                else if (position.x < -halfExtents.x)
                {
                    position.x = halfExtents.x - (-halfExtents.x - position.x);
                }

                if (position.z > halfExtents.y)
                {
                    position.z = -halfExtents.y + (position.z - halfExtents.y);
                }
                else if (position.z < -halfExtents.y)
                {
                    position.z = halfExtents.y - (-halfExtents.y - position.z);
                }

                transform.Position = position;
            }
        }
    }
}
