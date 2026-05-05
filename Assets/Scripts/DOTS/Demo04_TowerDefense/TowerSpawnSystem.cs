using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace UnityDotsDemo.Demo04
{
    public partial struct TowerSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaveSpawnerConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (configRef, waveStateRef, spawnerEntity) in
                     SystemAPI.Query<RefRO<WaveSpawnerConfig>, RefRW<WaveSpawnerState>>()
                         .WithEntityAccess())
            {
                if (waveStateRef.ValueRO.TowersSpawned != 0)
                {
                    continue;
                }

                WaveSpawnerConfig config = configRef.ValueRO;
                if (config.TowerPrefab == Entity.Null || config.ProjectilePrefab == Entity.Null)
                {
                    continue;
                }

                DynamicBuffer<TowerSpawnPoint> towerPoints =
                    SystemAPI.GetBuffer<TowerSpawnPoint>(spawnerEntity);
                bool hasLocalToWorld =
                    state.EntityManager.HasComponent<LocalToWorld>(config.TowerPrefab);

                for (int i = 0; i < towerPoints.Length; i++)
                {
                    Entity tower = ecb.Instantiate(config.TowerPrefab);
                    LocalTransform towerTransform = LocalTransform.FromPositionRotationScale(
                        towerPoints[i].Position, quaternion.identity, 1f);
                    ecb.SetComponent(tower, towerTransform);
                    EnemyPoolUtility.SetOrAddLocalToWorld(ecb, tower, hasLocalToWorld, towerTransform);
                    ecb.AddComponent<TowerTag>(tower);
                    ecb.AddComponent(tower, new TowerAttack
                    {
                        Range = config.TowerRange,
                        FireRate = config.TowerFireRate,
                        Cooldown = 0f,
                        ProjectilePrefab = config.ProjectilePrefab,
                        ProjectileSpeed = config.ProjectileSpeed,
                        ProjectileDamage = config.ProjectileDamage,
                        ProjectileLifetime = config.ProjectileLifetime,
                        ProjectileHitRadius = config.ProjectileHitRadius
                    });
                }

                waveStateRef.ValueRW.TowersSpawned = 1;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
