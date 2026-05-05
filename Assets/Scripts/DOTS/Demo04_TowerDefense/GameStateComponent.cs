using System;
using Unity.Entities;
using UnityEngine;

namespace UnityDotsDemo.Demo04
{
    public enum GamePhase : byte
    {
        Preparing,
        Playing,
        Victory,
        Defeat
    }

    public struct GameState : IComponentData
    {
        public GamePhase Phase;
        public int CurrentWave;
        public int TotalWaves;
        public int KillCount;
        public int EnemyAliveCount;
    }

    public struct EnemyMaxHealth : IComponentData
    {
        public float Value;
    }

    public struct WaveDefinition : IBufferElementData
    {
        public int NormalCount;
        public int FastCount;
        public int BossCount;
        public float NormalHealth;
        public float NormalSpeed;
        public float FastHealth;
        public float FastSpeed;
        public float BossHealth;
        public float BossSpeed;

        public int TotalCount => NormalCount + FastCount + BossCount;
    }

    [Serializable]
    public struct WaveDefinitionAuthoring
    {
        [Min(0)] public int NormalCount;
        [Min(0)] public int FastCount;
        [Min(0)] public int BossCount;
        [Min(1f)] public float NormalHealth;
        [Min(0.1f)] public float NormalSpeed;
        [Min(1f)] public float FastHealth;
        [Min(0.1f)] public float FastSpeed;
        [Min(1f)] public float BossHealth;
        [Min(0.1f)] public float BossSpeed;

        public WaveDefinition ToRuntime(float fallbackHealth, float fallbackSpeed)
        {
            float normalHealth = Mathf.Max(1f, NormalHealth <= 0f ? fallbackHealth : NormalHealth);
            float normalSpeed = Mathf.Max(0.1f, NormalSpeed <= 0f ? fallbackSpeed : NormalSpeed);

            return new WaveDefinition
            {
                NormalCount = Mathf.Max(0, NormalCount),
                FastCount = Mathf.Max(0, FastCount),
                BossCount = Mathf.Max(0, BossCount),
                NormalHealth = normalHealth,
                NormalSpeed = normalSpeed,
                FastHealth = Mathf.Max(1f, FastHealth <= 0f ? normalHealth * 0.7f : FastHealth),
                FastSpeed = Mathf.Max(0.1f, FastSpeed <= 0f ? normalSpeed * 1.6f : FastSpeed),
                BossHealth = Mathf.Max(1f, BossHealth <= 0f ? normalHealth * 4f : BossHealth),
                BossSpeed = Mathf.Max(0.1f, BossSpeed <= 0f ? normalSpeed * 0.65f : BossSpeed)
            };
        }
    }

}
