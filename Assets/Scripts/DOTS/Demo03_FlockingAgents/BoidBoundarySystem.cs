using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo03
{
    [BurstCompile]
    public partial struct BoidBoundarySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new BoundaryJob().ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(BoidTag))]
        private partial struct BoundaryJob : IJobEntity
        {
            private void Execute(
                ref LocalTransform transform,
                ref BoidVelocity velocity,
                in SimulationBounds bounds)
            {
                float3 min = bounds.Center - bounds.Extents;
                float3 max = bounds.Center + bounds.Extents;
                float3 position = transform.Position;
                float3 currentVelocity = velocity.Value;

                if (position.x < min.x || position.x > max.x)
                {
                    position.x = math.clamp(position.x, min.x, max.x);
                    currentVelocity.x *= -1f;
                }

                if (position.y < min.y || position.y > max.y)
                {
                    position.y = math.clamp(position.y, min.y, max.y);
                    currentVelocity.y *= -1f;
                }

                if (position.z < min.z || position.z > max.z)
                {
                    position.z = math.clamp(position.z, min.z, max.z);
                    currentVelocity.z *= -1f;
                }

                transform.Position = position;
                velocity.Value = currentVelocity;
            }
        }
    }
}
