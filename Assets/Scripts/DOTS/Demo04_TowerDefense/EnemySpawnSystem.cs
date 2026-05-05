using DOTSDemo.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo04
{
    [UpdateAfter(typeof(WaveProgressionSystem))]
    public partial struct EnemySpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaveSpawnerConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            bool gameOver = false;
            foreach (RefRO<GameState> gameStateRef in SystemAPI.Query<RefRO<GameState>>())
            {
                gameOver = gameStateRef.ValueRO.Phase == GamePhase.Victory ||
                           gameStateRef.ValueRO.Phase == GamePhase.Defeat;
                break;
            }

            if (gameOver)
            {
                return;
            }

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (configRef, waveStateRef, pool, spawnerEntity) in
                     SystemAPI.Query<RefRO<WaveSpawnerConfig>, RefRW<WaveSpawnerState>,
                         DynamicBuffer<SpawnPoolElement>>().WithEntityAccess())
            {
                WaveSpawnerConfig config = configRef.ValueRO;
                DynamicBuffer<Waypoint> waypoints = SystemAPI.GetBuffer<Waypoint>(spawnerEntity);
                DynamicBuffer<WaveDefinition> waves = SystemAPI.GetBuffer<WaveDefinition>(spawnerEntity);

                EnemyPoolUtility.GrowPoolIfNeeded(ecb, spawnerEntity, pool, config);

                if (waypoints.Length == 0 ||
                    config.EnemyPrefab == Entity.Null)
                {
                    continue;
                }

                int maxWaves = waves.Length > 0 ? waves.Length : config.MaxWaves;
                if (waveStateRef.ValueRO.CurrentWave >= maxWaves)
                {
                    continue;
                }

                WaveDefinition wave = EnemyPoolUtility.GetWaveDefinition(
                    config, waves, waveStateRef.ValueRO.CurrentWave);
                int enemiesInWave = math.max(1, wave.TotalCount);

                if (waveStateRef.ValueRO.SpawnedInWave >= enemiesInWave)
                {
                    continue;
                }

                if (waveStateRef.ValueRO.SpawnTimer > 0f)
                {
                    continue;
                }

                if (pool.Length == 0)
                {
                    continue;
                }

                EnemyPoolUtility.GetEnemyStats(
                    wave, waveStateRef.ValueRO.SpawnedInWave,
                    out float enemyHealth, out float enemySpeed);

                int lastIndex = pool.Length - 1;
                Entity enemy = pool[lastIndex].Value;
                pool.RemoveAt(lastIndex);

                LocalTransform enemyTransform = LocalTransform.FromPositionRotationScale(
                    waypoints[0].Position, quaternion.identity, 1f);
                ecb.RemoveComponent<PooledEnemy>(enemy);
                ecb.AddComponent<EnemyTag>(enemy);
                ecb.SetComponent(enemy, enemyTransform);
                ecb.SetComponent(enemy, new Health { Value = enemyHealth });
                ecb.SetComponent(enemy, new EnemyMaxHealth { Value = enemyHealth });
                ecb.SetComponent(enemy, new MoveSpeed { Value = enemySpeed });
                ecb.SetComponent(enemy, new WaypointIndex
                {
                    Value = math.min(1, waypoints.Length - 1)
                });

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
    }
}
