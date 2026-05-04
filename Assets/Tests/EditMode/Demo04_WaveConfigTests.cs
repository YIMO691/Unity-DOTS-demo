using NUnit.Framework;
using UnityDotsDemo.Demo04;

namespace UnityDotsDemo.Tests.EditMode
{
    public sealed class Demo04WaveConfigTests
    {
        [Test]
        public void RuntimeWaveDefinitionHasEnemies()
        {
            WaveDefinitionAuthoring authoring = new WaveDefinitionAuthoring
            {
                NormalCount = 5,
                FastCount = 3,
                BossCount = 1,
                NormalHealth = 30f,
                NormalSpeed = 3f
            };

            WaveDefinition wave = authoring.ToRuntime(30f, 3f);
            Assert.Greater(wave.TotalCount, 0);
            Assert.Greater(wave.NormalHealth, 0f);
            Assert.Greater(wave.NormalSpeed, 0f);
        }

        [Test]
        public void GameStateTotalWavesCanMatchConfiguredWaves()
        {
            GameState gameState = new GameState
            {
                Phase = GamePhase.Preparing,
                TotalWaves = 5
            };

            Assert.Greater(gameState.TotalWaves, 0);
            Assert.AreEqual(5, gameState.TotalWaves);
        }
    }
}
