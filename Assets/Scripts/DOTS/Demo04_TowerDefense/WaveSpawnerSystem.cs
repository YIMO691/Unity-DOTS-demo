using DOTSDemo.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo04
{
    public partial struct WaveSpawnerSystem : ISystem
    {
        private const int PoolGrowSize = 20;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaveSpawnerConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            bool gameOver = false;
            foreach (RefRO<GameState> gameStateRef in SystemAPI.Query<RefRO<GameState>>())
            {
                gameOver = gameStateRef.ValueRO.Phase == GamePhase.Victory ||
                           gameStateRef.ValueRO.Phase == GamePhase.Defeat;
                break;
            }

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (configRef, waveStateRef, pool, spawnerEntity) in
                     SystemAPI.Query<RefRO<WaveSpawnerConfig>, RefRW<WaveSpawnerState>, DynamicBuffer<Entity>>()
                         .WithEntityAccess())
            {
                if (gameOver)
                {
                    continue;
                }

                WaveSpawnerConfig config = configRef.ValueRO;
                DynamicBuffer<Waypoint> waypoints = SystemAPI.GetBuffer<Waypoint>(spawnerEntity);
                DynamicBuffer<TowerSpawnPoint> towerPoints = SystemAPI.GetBuffer<TowerSpawnPoint>(spawnerEntity);
                DynamicBuffer<WaveDefinition> waves = SystemAPI.GetBuffer<WaveDefinition>(spawnerEntity);
                int maxWaves = waves.Length > 0 ? waves.Length : config.MaxWaves;

                GrowPoolIfNeeded(ref state, pool, config);

                if (waveStateRef.ValueRO.TowersSpawned == 0)
                {
                    bool towerPrefabHasLocalToWorld =
                        config.TowerPrefab != Entity.Null &&
                        state.EntityManager.HasComponent<LocalToWorld>(config.TowerPrefab);
                    SpawnTowers(ecb, config, towerPoints, towerPrefabHasLocalToWorld);
                    waveStateRef.ValueRW.TowersSpawned = 1;
                }

                if (waypoints.Length == 0 ||
                    config.EnemyPrefab == Entity.Null ||
                    waveStateRef.ValueRO.CurrentWave >= maxWaves)
                {
                    continue;
                }

                WaveDefinition wave = GetWaveDefinition(config, waves, waveStateRef.ValueRO.CurrentWave);
                int enemiesInWave = math.max(1, wave.TotalCount);

                if (waveStateRef.ValueRO.SpawnedInWave >= enemiesInWave)
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

                GetEnemyStats(wave, waveStateRef.ValueRO.SpawnedInWave, out float enemyHealth, out float enemySpeed);

                if (pool.Length == 0)
                {
                    GrowPool(ref state, pool, config, PoolGrowSize);
                    if (pool.Length == 0)
                    {
                        continue;
                    }
                }

                int lastIndex = pool.Length - 1;
                Entity enemy = pool[lastIndex];
                pool.RemoveAt(lastIndex);

                LocalTransform enemyTransform = LocalTransform.FromPositionRotationScale(
                    waypoints[0].Position, quaternion.identity, 1f);
                ecb.RemoveComponent<PooledEnemy>(enemy);
                ecb.AddComponent<EnemyTag>(enemy);
                ecb.SetComponent(enemy, enemyTransform);
                ecb.SetComponent(enemy, new Health { Value = enemyHealth });
                ecb.SetComponent(enemy, new EnemyMaxHealth { Value = enemyHealth });
                ecb.SetComponent(enemy, new MoveSpeed { Value = enemySpeed });
                ecb.SetComponent(enemy, new WaypointIndex { Value = math.min(1, waypoints.Length - 1) });

                if (state.EntityManager.HasBuffer<DamageEvent>(enemy))
                {
                    ecb.SetBuffer<DamageEvent>(enemy);
                }
                else
                {
                    ecb.AddBuffer<DamageEvent>(enemy);
                }

                waveStateRef.ValueRW.SpawnedInWave++;
                waveStateRef.ValueRW.SpawnTimer = config.SpawnInterval;

                if (waveStateRef.ValueRO.SpawnedInWave >= enemiesInWave)
                {
                    waveStateRef.ValueRW.WaveCooldown = config.TimeBetweenWaves;
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static void GrowPoolIfNeeded(
            ref SystemState state,
            DynamicBuffer<Entity> pool,
            WaveSpawnerConfig config)
        {
            if (pool.Length < PoolGrowSize)
            {
                GrowPool(ref state, pool, config, PoolGrowSize);
            }
        }

        private static void GrowPool(
            ref SystemState state,
            DynamicBuffer<Entity> pool,
            WaveSpawnerConfig config,
            int count)
        {
            bool prefabHasLocalToWorld =
                state.EntityManager.HasComponent<LocalToWorld>(config.EnemyPrefab);

            for (int i = 0; i < count; i++)
            {
                Entity enemy = state.EntityManager.Instantiate(config.EnemyPrefab);
                state.EntityManager.AddComponent<PooledEnemy>(enemy);
                state.EntityManager.AddComponent<MoveSpeed>(enemy);
                state.EntityManager.AddComponent<WaypointIndex>(enemy);

                if (!state.EntityManager.HasComponent<Health>(enemy))
                {
                    state.EntityManager.AddComponent<Health>(enemy);
                }

                if (!state.EntityManager.HasComponent<EnemyMaxHealth>(enemy))
                {
                    state.EntityManager.AddComponent<EnemyMaxHealth>(enemy);
                }

                if (!state.EntityManager.HasBuffer<DamageEvent>(enemy))
                {
                    state.EntityManager.AddBuffer<DamageEvent>(enemy);
                }

                state.EntityManager.SetComponentData(enemy,
                    LocalTransform.FromPosition(0f, -100f, 0f));

                if (prefabHasLocalToWorld)
                {
                    state.EntityManager.SetComponentData(enemy, new LocalToWorld
                    {
                        Value = float4x4.TRS(new float3(0f, -100f, 0f), quaternion.identity, new float3(1f))
                    });
                }

                pool.Add(enemy);
            }
        }

        public static void ReturnToPool(EntityCommandBuffer ecb, Entity enemy)
        {
            ecb.RemoveComponent<EnemyTag>(enemy);
            ecb.RemoveComponent<EnemyReachedBase>(enemy);
            ecb.AddComponent<PooledEnemy>(enemy);
            ecb.SetComponent(enemy, LocalTransform.FromPosition(0f, -100f, 0f));
        }

        private static WaveDefinition GetWaveDefinition(
            WaveSpawnerConfig config,
            DynamicBuffer<WaveDefinition> waves,
            int waveIndex)
        {
            if (waves.Length > 0)
            {
                return waves[math.clamp(waveIndex, 0, waves.Length - 1)];
            }

            return new WaveDefinition
            {
                NormalCount = math.max(1, config.EnemiesPerWave),
                FastCount = 0,
                BossCount = 0,
                NormalHealth = config.EnemyHealth,
                NormalSpeed = config.EnemySpeed,
                FastHealth = config.EnemyHealth * 0.7f,
                FastSpeed = config.EnemySpeed * 1.6f,
                BossHealth = config.EnemyHealth * 4f,
                BossSpeed = config.EnemySpeed * 0.65f
            };
        }

        private static void GetEnemyStats(
            WaveDefinition wave,
            int spawnedInWave,
            out float health,
            out float speed)
        {
            if (spawnedInWave < wave.NormalCount)
            {
                health = wave.NormalHealth;
                speed = wave.NormalSpeed;
                return;
            }

            if (spawnedInWave < wave.NormalCount + wave.FastCount)
            {
                health = wave.FastHealth;
                speed = wave.FastSpeed;
                return;
            }

            health = wave.BossHealth;
            speed = wave.BossSpeed;
        }

        private static void SpawnTowers(
            EntityCommandBuffer ecb,
            WaveSpawnerConfig config,
            DynamicBuffer<TowerSpawnPoint> towerPoints,
            bool towerPrefabHasLocalToWorld)
        {
            if (config.TowerPrefab == Entity.Null || config.ProjectilePrefab == Entity.Null)
            {
                return;
            }

            for (int i = 0; i < towerPoints.Length; i++)
            {
                Entity tower = ecb.Instantiate(config.TowerPrefab);
                LocalTransform towerTransform = LocalTransform.FromPositionRotationScale(
                    towerPoints[i].Position, quaternion.identity, 1f);
                ecb.SetComponent(tower, towerTransform);
                SetOrAddLocalToWorld(ecb, tower, towerPrefabHasLocalToWorld, towerTransform);
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

        private static void SetOrAddLocalToWorld(
            EntityCommandBuffer ecb,
            Entity entity,
            bool hasLocalToWorld,
            LocalTransform transform)
        {
            LocalToWorld localToWorld = new LocalToWorld
            {
                Value = float4x4.TRS(
                    transform.Position,
                    transform.Rotation,
                    new float3(transform.Scale))
            };

            if (hasLocalToWorld)
            {
                ecb.SetComponent(entity, localToWorld);
            }
            else
            {
                ecb.AddComponent(entity, localToWorld);
            }
        }
    }
}
