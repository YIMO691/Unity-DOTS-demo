using DOTSDemo.Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace UnityDotsDemo.Template
{
    [BurstCompile]
    public partial struct TemplateMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (transform, speed, velocity) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveSpeed>, RefRO<Velocity>>()
                         .WithAll<TemplateTag>())
            {
                transform.ValueRW.Position += velocity.ValueRO.Value * speed.ValueRO.Value * deltaTime;
            }
        }
    }
}
