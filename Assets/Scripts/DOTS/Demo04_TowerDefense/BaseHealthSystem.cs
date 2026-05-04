using Unity.Collections;
using Unity.Entities;

namespace UnityDotsDemo.Demo04
{
    [UpdateAfter(typeof(EnemyMovementSystem))]
    public partial struct BaseHealthSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            Entity baseEntity = Entity.Null;
            BaseHealth baseHealth = default;

            foreach (var (healthRef, entity) in
                     SystemAPI.Query<RefRO<BaseHealth>>().WithEntityAccess())
            {
                baseEntity = entity;
                baseHealth = healthRef.ValueRO;
                break;
            }

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            NativeArray<Entity> reachedEnemies = SystemAPI.QueryBuilder()
                .WithAll<EnemyTag, EnemyReachedBase>()
                .Build()
                .ToEntityArray(Allocator.Temp);

            foreach (Entity enemyEntity in reachedEnemies)
            {
                if (baseEntity != Entity.Null && baseHealth.CurrentHP > 0)
                {
                    baseHealth.CurrentHP--;
                }

                ecb.DestroyEntity(enemyEntity);
            }

            if (baseEntity != Entity.Null)
            {
                ecb.SetComponent(baseEntity, baseHealth);
                if (baseHealth.CurrentHP <= 0)
                {
                    foreach (var (gameStateRef, entity) in
                             SystemAPI.Query<RefRO<GameState>>().WithEntityAccess())
                    {
                        GameState gameState = gameStateRef.ValueRO;
                        gameState.Phase = GamePhase.Defeat;
                        ecb.SetComponent(entity, gameState);
                        break;
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            reachedEnemies.Dispose();
            ecb.Dispose();
        }
    }
}
