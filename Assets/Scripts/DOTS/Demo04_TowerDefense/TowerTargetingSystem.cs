using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityDotsDemo.Demo04
{
    [UpdateAfter(typeof(EnemyMovementSystem))]
    public partial struct TowerTargetingSystem : ISystem
    {
        private EntityQuery _enemyQuery;
        private EntityQuery _projectileQuery;

        public void OnCreate(ref SystemState state)
        {
            _enemyQuery = SystemAPI.QueryBuilder()
                .WithAll<EnemyTag, LocalTransform, Health>()
                .Build();
            _projectileQuery = SystemAPI.QueryBuilder()
                .WithAll<ProjectileTag, ProjectileData>()
                .Build();
        }

        public void OnUpdate(ref SystemState state)
        {
            int enemyCount = _enemyQuery.CalculateEntityCount();
            if (enemyCount == 0)
            {
                return;
            }

            NativeArray<Entity> enemyEntities = _enemyQuery.ToEntityArray(Allocator.Temp);
            NativeArray<LocalTransform> enemyTransforms =
                _enemyQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            NativeArray<Health> enemyHealth = _enemyQuery.ToComponentDataArray<Health>(Allocator.Temp);
            NativeArray<ProjectileData> activeProjectiles =
                _projectileQuery.ToComponentDataArray<ProjectileData>(Allocator.Temp);
            float deltaTime = SystemAPI.Time.DeltaTime;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            NativeParallelHashMap<Entity, float> pendingDamageByTarget =
                new NativeParallelHashMap<Entity, float>(math.max(1, activeProjectiles.Length * 2), Allocator.Temp);

            for (int i = 0; i < activeProjectiles.Length; i++)
            {
                Entity target = activeProjectiles[i].Target;
                if (target == Entity.Null)
                {
                    continue;
                }

                AddPendingDamage(pendingDamageByTarget, target, activeProjectiles[i].Damage);
            }

            foreach (var (towerTransformRef, attackRef) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRW<TowerAttack>>().WithAll<TowerTag>())
            {
                attackRef.ValueRW.Cooldown = math.max(0f, attackRef.ValueRO.Cooldown - deltaTime);
                if (attackRef.ValueRO.Cooldown > 0f ||
                    attackRef.ValueRO.ProjectilePrefab == Entity.Null)
                {
                    continue;
                }

                Entity target = Entity.Null;
                float3 towerPosition = towerTransformRef.ValueRO.Position;
                float bestDistanceSq = attackRef.ValueRO.Range * attackRef.ValueRO.Range;
                float3 targetPosition = towerPosition;

                for (int i = 0; i < enemyCount; i++)
                {
                    if (enemyHealth[i].Value <= 0f)
                    {
                        continue;
                    }

                    if (pendingDamageByTarget.TryGetValue(enemyEntities[i], out float enemyPendingDamage) &&
                        enemyHealth[i].Value <= enemyPendingDamage)
                    {
                        continue;
                    }

                    float distanceSq = math.distancesq(towerPosition, enemyTransforms[i].Position);
                    if (distanceSq <= bestDistanceSq)
                    {
                        bestDistanceSq = distanceSq;
                        target = enemyEntities[i];
                        targetPosition = enemyTransforms[i].Position;
                    }
                }

                if (target == Entity.Null)
                {
                    continue;
                }

                float3 aimDirection = math.normalizesafe(
                    targetPosition - towerPosition,
                    new float3(0f, 0f, 1f));
                float3 spawnPosition = towerPosition
                    + new float3(0f, 0.95f, 0f)
                    + aimDirection * 0.85f;
                float projectileStep = attackRef.ValueRO.ProjectileSpeed * deltaTime;
                float distanceToTarget = math.distance(spawnPosition, targetPosition);

                if (distanceToTarget <= attackRef.ValueRO.ProjectileHitRadius ||
                    distanceToTarget <= projectileStep)
                {
                    ecb.AppendToBuffer(target, new DamageEvent
                    {
                        Value = attackRef.ValueRO.ProjectileDamage
                    });
                    AddPendingDamage(
                        pendingDamageByTarget,
                        target,
                        attackRef.ValueRO.ProjectileDamage);
                    attackRef.ValueRW.Cooldown = 1f / math.max(0.01f, attackRef.ValueRO.FireRate);
                    continue;
                }

                Entity projectile = ecb.Instantiate(attackRef.ValueRO.ProjectilePrefab);
                LocalTransform projectileTransform = LocalTransform.FromPositionRotationScale(
                    spawnPosition,
                    quaternion.identity,
                    0.35f);

                ecb.SetComponent(projectile, projectileTransform);
                ecb.SetComponent(projectile, new LocalToWorld
                {
                    Value = float4x4.TRS(
                        projectileTransform.Position,
                        projectileTransform.Rotation,
                        new float3(projectileTransform.Scale))
                });
                ecb.AddComponent<ProjectileTag>(projectile);
                ecb.AddComponent(projectile, new ProjectileData
                {
                    Target = target,
                    Speed = attackRef.ValueRO.ProjectileSpeed,
                    Damage = attackRef.ValueRO.ProjectileDamage,
                    HitRadius = attackRef.ValueRO.ProjectileHitRadius
                });
                ecb.AddComponent(projectile, new Lifetime
                {
                    Remaining = attackRef.ValueRO.ProjectileLifetime
                });

                AddPendingDamage(
                    pendingDamageByTarget,
                    target,
                    attackRef.ValueRO.ProjectileDamage);

                attackRef.ValueRW.Cooldown = 1f / math.max(0.01f, attackRef.ValueRO.FireRate);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            enemyEntities.Dispose();
            enemyTransforms.Dispose();
            enemyHealth.Dispose();
            activeProjectiles.Dispose();
            pendingDamageByTarget.Dispose();
        }

        private static void AddPendingDamage(
            NativeParallelHashMap<Entity, float> pendingDamageByTarget,
            Entity target,
            float damage)
        {
            if (pendingDamageByTarget.TryGetValue(target, out float existingDamage))
            {
                pendingDamageByTarget[target] = existingDamage + damage;
            }
            else
            {
                pendingDamageByTarget.Add(target, damage);
            }
        }
    }
}
