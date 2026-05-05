using Unity.Entities;

namespace UnityDotsDemo.Demo04
{
    [UpdateAfter(typeof(CleanupSystem))]
    public partial struct GameStateSystem : ISystem
    {
        private EntityQuery _enemyQuery;

        public void OnCreate(ref SystemState state)
        {
            _enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag>().WithNone<PooledEnemy>().Build();
        }

        public void OnUpdate(ref SystemState state)
        {
            int enemyAliveCount = _enemyQuery.CalculateEntityCount();

            foreach (var gameStateRef in SystemAPI.Query<RefRW<GameState>>())
            {
                GameState gameState = gameStateRef.ValueRO;
                gameState.EnemyAliveCount = enemyAliveCount;

                foreach (var (configRef, waveStateRef, waveBuffer) in
                         SystemAPI.Query<RefRO<WaveSpawnerConfig>, RefRO<WaveSpawnerState>, DynamicBuffer<WaveDefinition>>())
                {
                    int totalWaves = waveBuffer.Length > 0 ? waveBuffer.Length : configRef.ValueRO.MaxWaves;
                    gameState.TotalWaves = totalWaves;
                    gameState.CurrentWave = Unity.Mathematics.math.min(waveStateRef.ValueRO.CurrentWave + 1, totalWaves);

                    if (gameState.Phase != GamePhase.Defeat &&
                        waveStateRef.ValueRO.CurrentWave >= totalWaves &&
                        enemyAliveCount == 0)
                    {
                        gameState.Phase = GamePhase.Victory;
                    }
                    else if (gameState.Phase == GamePhase.Preparing)
                    {
                        gameState.Phase = GamePhase.Playing;
                    }

                    break;
                }

                gameStateRef.ValueRW = gameState;
            }
        }
    }
}
