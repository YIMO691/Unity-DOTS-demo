using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.Templates.DemoTemplate
{
    [BurstCompile]
    public partial struct TemplateMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (transform, speed, direction) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<TemplateMoveSpeed>, RefRO<TemplateDirection>>())
            {
                transform.ValueRW.Position += direction.ValueRO.Value * speed.ValueRO.Value * deltaTime;
            }
        }
    }
}
