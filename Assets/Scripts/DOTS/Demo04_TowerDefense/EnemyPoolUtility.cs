using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo04
{
    public static class EnemyPoolUtility
    {
        public const int PoolGrowSize = 20;

        public static void ReturnToPool(EntityCommandBuffer ecb, Entity enemy)
        {
            ecb.RemoveComponent<EnemyTag>(enemy);
            ecb.RemoveComponent<EnemyReachedBase>(enemy);
            ecb.AddComponent<PooledEnemy>(enemy);
            ecb.SetComponent(enemy, LocalTransform.FromPosition(0f, -100f, 0f));
        }

        public static void GrowPoolIfNeeded(
            EntityCommandBuffer ecb,
            Entity spawnerEntity,
            DynamicBuffer<SpawnPoolElement> pool,
            WaveSpawnerConfig config)
        {
            if (pool.Length < PoolGrowSize)
            {
                GrowPool(ecb, spawnerEntity, pool, config, PoolGrowSize);
            }
        }

        public static void GrowPool(
            EntityCommandBuffer ecb,
            Entity spawnerEntity,
            DynamicBuffer<SpawnPoolElement> pool,
            WaveSpawnerConfig config,
            int count)
        {
            float3 offscreen = new float3(0f, -100f, 0f);

            for (int i = 0; i < count; i++)
            {
                Entity enemy = ecb.Instantiate(config.EnemyPrefab);
                ecb.AddComponent<PooledEnemy>(enemy);
                ecb.AddComponent<MoveSpeed>(enemy);
                ecb.AddComponent<WaypointIndex>(enemy);
                ecb.AddComponent<Health>(enemy);
                ecb.AddComponent<EnemyMaxHealth>(enemy);
                ecb.AddBuffer<DamageEvent>(enemy);
                ecb.SetComponent(enemy, LocalTransform.FromPosition(offscreen));

                if (config.EnemyPrefab != Entity.Null)
                {
                    ecb.SetComponent(enemy, new LocalToWorld
                    {
                        Value = float4x4.TRS(offscreen, quaternion.identity, new float3(1f))
                    });
                }

                ecb.AppendToBuffer(spawnerEntity, new SpawnPoolElement { Value = enemy });
            }
        }

        public static void SetOrAddLocalToWorld(
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

        public static WaveDefinition GetWaveDefinition(
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

        public static void GetEnemyStats(
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
    }
}
