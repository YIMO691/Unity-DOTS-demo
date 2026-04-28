using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo04
{
    public partial struct WaveSpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaveSpawnerConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (configRef, waveStateRef, spawnerEntity) in
                     SystemAPI.Query<RefRO<WaveSpawnerConfig>, RefRW<WaveSpawnerState>>().WithEntityAccess())
            {
                WaveSpawnerConfig config = configRef.ValueRO;
                DynamicBuffer<Waypoint> waypoints = SystemAPI.GetBuffer<Waypoint>(spawnerEntity);
                DynamicBuffer<TowerSpawnPoint> towerPoints = SystemAPI.GetBuffer<TowerSpawnPoint>(spawnerEntity);

                if (waveStateRef.ValueRO.TowersSpawned == 0)
                {
                    SpawnTowers(ecb, config, towerPoints);
                    waveStateRef.ValueRW.TowersSpawned = 1;
                }

                if (waypoints.Length == 0 ||
                    config.EnemyPrefab == Entity.Null ||
                    waveStateRef.ValueRO.CurrentWave >= config.MaxWaves)
                {
                    continue;
                }

                if (waveStateRef.ValueRO.SpawnedInWave >= config.EnemiesPerWave)
                {
                    waveStateRef.ValueRW.WaveCooldown -= deltaTime;
                    if (waveStateRef.ValueRO.WaveCooldown <= 0f)
                    {
                        waveStateRef.ValueRW.CurrentWave++;
                        waveStateRef.ValueRW.SpawnedInWave = 0;
                        waveStateRef.ValueRW.SpawnTimer = 0f;
                        waveStateRef.ValueRW.WaveCooldown = config.TimeBetweenWaves;
                    }

                    continue;
                }

                waveStateRef.ValueRW.SpawnTimer -= deltaTime;
                if (waveStateRef.ValueRO.SpawnTimer > 0f)
                {
                    continue;
                }

                Entity enemy = ecb.Instantiate(config.EnemyPrefab);
                ecb.SetComponent(enemy, LocalTransform.FromPositionRotationScale(
                    waypoints[0].Position,
                    quaternion.identity,
                    1f));
                ecb.AddComponent<EnemyTag>(enemy);
                ecb.AddComponent(enemy, new Health { Value = config.EnemyHealth });
                ecb.AddComponent(enemy, new MoveSpeed { Value = config.EnemySpeed });
                ecb.AddComponent(enemy, new WaypointIndex { Value = math.min(1, waypoints.Length - 1) });
                ecb.AddBuffer<DamageEvent>(enemy);

                waveStateRef.ValueRW.SpawnedInWave++;
                waveStateRef.ValueRW.SpawnTimer = config.SpawnInterval;

                if (waveStateRef.ValueRO.SpawnedInWave >= config.EnemiesPerWave)
                {
                    waveStateRef.ValueRW.WaveCooldown = config.TimeBetweenWaves;
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static void SpawnTowers(
            EntityCommandBuffer ecb,
            WaveSpawnerConfig config,
            DynamicBuffer<TowerSpawnPoint> towerPoints)
        {
            if (config.TowerPrefab == Entity.Null || config.ProjectilePrefab == Entity.Null)
            {
                return;
            }

            for (int i = 0; i < towerPoints.Length; i++)
            {
                Entity tower = ecb.Instantiate(config.TowerPrefab);
                ecb.SetComponent(tower, LocalTransform.FromPositionRotationScale(
                    towerPoints[i].Position,
                    quaternion.identity,
                    1f));
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
        }
    }
}
