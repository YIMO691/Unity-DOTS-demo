using Unity.Entities;
using Unity.Mathematics;

namespace UnityDotsDemo.Demo04
{
    [UpdateBefore(typeof(EnemySpawnSystem))]
    public partial struct WaveProgressionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaveSpawnerConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (configRef, waveStateRef, spawnerEntity) in
                     SystemAPI.Query<RefRO<WaveSpawnerConfig>, RefRW<WaveSpawnerState>>()
                         .WithEntityAccess())
            {
                WaveSpawnerConfig config = configRef.ValueRO;
                DynamicBuffer<WaveDefinition> waves =
                    SystemAPI.GetBuffer<WaveDefinition>(spawnerEntity);
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
            }
        }
    }
}
