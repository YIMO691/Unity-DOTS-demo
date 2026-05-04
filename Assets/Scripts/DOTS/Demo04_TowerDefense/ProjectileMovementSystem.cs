using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo04
{
    [UpdateAfter(typeof(TowerTargetingSystem))]
    public partial struct ProjectileMovementSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (transformRef, localToWorldRef, lifetimeRef, projectileDataRef, projectileEntity) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRW<LocalToWorld>, RefRW<Lifetime>, RefRO<ProjectileData>>()
                         .WithAll<ProjectileTag>()
                         .WithEntityAccess())
            {
                lifetimeRef.ValueRW.Remaining -= deltaTime;

                Entity target = projectileDataRef.ValueRO.Target;
                if (!state.EntityManager.Exists(target) ||
                    !state.EntityManager.HasComponent<LocalTransform>(target) ||
                    !state.EntityManager.HasBuffer<DamageEvent>(target))
                {
                    ecb.DestroyEntity(projectileEntity);
                    continue;
                }

                LocalTransform targetTransform = state.EntityManager.GetComponentData<LocalTransform>(target);
                float3 position = transformRef.ValueRO.Position;
                float3 toTarget = targetTransform.Position - position;
                float distance = math.length(toTarget);
                float step = projectileDataRef.ValueRO.Speed * deltaTime;

                if (distance <= projectileDataRef.ValueRO.HitRadius || distance <= step)
                {
                    ecb.AppendToBuffer(target, new DamageEvent { Value = projectileDataRef.ValueRO.Damage });
                    ecb.DestroyEntity(projectileEntity);
                    continue;
                }

                float3 direction = toTarget / math.max(distance, 0.0001f);
                LocalTransform nextTransform = transformRef.ValueRO;
                nextTransform.Position = position + direction * step;
                nextTransform.Rotation = quaternion.LookRotationSafe(direction, math.up());
                transformRef.ValueRW = nextTransform;
                localToWorldRef.ValueRW.Value = float4x4.TRS(
                    nextTransform.Position,
                    nextTransform.Rotation,
                    new float3(nextTransform.Scale));
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
