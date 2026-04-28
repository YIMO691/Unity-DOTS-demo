using Unity.Collections;
using Unity.Entities;

namespace UnityDotsDemo.Demo04
{
    [UpdateAfter(typeof(DamageSystem))]
    public partial struct CleanupSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (healthRef, enemyEntity) in
                     SystemAPI.Query<RefRO<Health>>().WithAll<EnemyTag>().WithEntityAccess())
            {
                if (healthRef.ValueRO.Value <= 0f)
                {
                    ecb.DestroyEntity(enemyEntity);
                }
            }

            foreach (var (lifetimeRef, projectileEntity) in
                     SystemAPI.Query<RefRO<Lifetime>>().WithAll<ProjectileTag>().WithEntityAccess())
            {
                if (lifetimeRef.ValueRO.Remaining <= 0f)
                {
                    ecb.DestroyEntity(projectileEntity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
