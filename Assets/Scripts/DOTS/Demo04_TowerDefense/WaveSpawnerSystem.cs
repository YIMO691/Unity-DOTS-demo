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
            bool gameOver = false;
            foreach (RefRO<GameState> gameStateRef in SystemAPI.Query<RefRO<GameState>>())
            {
                gameOver = gameStateRef.ValueRO.Phase == GamePhase.Victory ||
                           gameStateRef.ValueRO.Phase == GamePhase.Defeat;
                break;
            }

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (configRef, waveStateRef, spawnerEntity) in
                     SystemAPI.Query<RefRO<WaveSpawnerConfig>, RefRW<WaveSpawnerState>>().WithEntityAccess())
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
                bool enemyPrefabHasLocalToWorld =
                    state.EntityManager.HasComponent<LocalToWorld>(config.EnemyPrefab);
                Entity enemy = ecb.Instantiate(config.EnemyPrefab);
                LocalTransform enemyTransform = LocalTransform.FromPositionRotationScale(
                    waypoints[0].Position,
                    quaternion.identity,
                    1f);
                ecb.SetComponent(enemy, enemyTransform);
                SetOrAddLocalToWorld(ecb, enemy, enemyPrefabHasLocalToWorld, enemyTransform);
                ecb.AddComponent<EnemyTag>(enemy);
                ecb.AddComponent(enemy, new Health { Value = enemyHealth });
                ecb.AddComponent(enemy, new EnemyMaxHealth { Value = enemyHealth });
                ecb.AddComponent(enemy, new MoveSpeed { Value = enemySpeed });
                ecb.AddComponent(enemy, new WaypointIndex { Value = math.min(1, waypoints.Length - 1) });
                ecb.AddBuffer<DamageEvent>(enemy);

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
                    towerPoints[i].Position,
                    quaternion.identity,
                    1f);
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
