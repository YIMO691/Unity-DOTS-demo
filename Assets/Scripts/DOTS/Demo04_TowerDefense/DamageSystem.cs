using Unity.Entities;

namespace UnityDotsDemo.Demo04
{
    [UpdateAfter(typeof(ProjectileMovementSystem))]
    public partial struct DamageSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (healthRef, damageBuffer) in
                     SystemAPI.Query<RefRW<Health>, DynamicBuffer<DamageEvent>>().WithAll<EnemyTag>())
            {
                float totalDamage = 0f;
                for (int i = 0; i < damageBuffer.Length; i++)
                {
                    totalDamage += damageBuffer[i].Value;
                }

                if (totalDamage > 0f)
                {
                    healthRef.ValueRW.Value -= totalDamage;
                    damageBuffer.Clear();
                }
            }
        }
    }
}
