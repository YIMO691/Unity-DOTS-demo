using Unity.Entities;
using UnityEngine;

namespace UnityDotsDemo.Demo04
{
    [DisallowMultipleComponent]
    public sealed class GameStateAuthoring : MonoBehaviour
    {
        [Min(1)] public int TotalWaves = 5;

        private sealed class GameStateBaker : Baker<GameStateAuthoring>
        {
            public override void Bake(GameStateAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GameState
                {
                    Phase = GamePhase.Preparing,
                    CurrentWave = 0,
                    TotalWaves = Mathf.Max(1, authoring.TotalWaves),
                    KillCount = 0,
                    EnemyAliveCount = 0
                });
            }
        }
    }
}
