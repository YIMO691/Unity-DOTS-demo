using Unity.Collections;
using Unity.Entities;

namespace UnityDotsDemo.Demo04
{
    [UpdateAfter(typeof(DamageSystem))]
    public partial struct CleanupSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            Entity gameStateEntity = Entity.Null;
            GameState gameState = default;
            bool hasGameState = false;
            Entity spawnerEntity = Entity.Null;
            bool hasSpawner = false;

            foreach (var (gameStateRef, entity) in
                     SystemAPI.Query<RefRO<GameState>>().WithEntityAccess())
            {
                gameStateEntity = entity;
                gameState = gameStateRef.ValueRO;
                hasGameState = true;
                break;
            }

            foreach (var (configRef, entity) in
                     SystemAPI.Query<RefRO<WaveSpawnerConfig>>().WithEntityAccess())
            {
                spawnerEntity = entity;
                hasSpawner = true;
                break;
            }

            foreach (var (healthRef, enemyEntity) in
                     SystemAPI.Query<RefRO<Health>>().WithAll<EnemyTag>().WithNone<PooledEnemy>().WithEntityAccess())
            {
                if (healthRef.ValueRO.Value <= 0f)
                {
                    if (hasGameState)
                    {
                        gameState.KillCount++;
                    }

                    EnemyPoolUtility.ReturnToPool(ecb, enemyEntity);
                    if (hasSpawner)
                    {
                        ecb.AppendToBuffer(spawnerEntity, new SpawnPoolElement { Value = enemyEntity });
                    }
                }
            }

            if (hasGameState)
            {
                ecb.SetComponent(gameStateEntity, gameState);
            }

            foreach (var (lifetimeRef, projectileEntity) in
                     SystemAPI.Query<RefRO<Lifetime>>().WithAll<ProjectileTag>().WithEntityAccess())
            {
                if (lifetimeRef.ValueRO.Remaining <= 0f)
                {
                    ecb.DestroyEntity(projectileEntity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
