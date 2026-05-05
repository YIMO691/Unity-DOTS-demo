using Unity.Entities;
using Unity.Mathematics;

namespace UnityDotsDemo.Demo04
{
    public struct EnemyTag : IComponentData
    {
    }

    public struct TowerTag : IComponentData
    {
    }

    public struct ProjectileTag : IComponentData
    {
    }

    public struct Health : IComponentData
    {
        public float Value;
    }

    public struct WaypointIndex : IComponentData
    {
        public int Value;
    }

    public struct TowerAttack : IComponentData
    {
        public float Range;
        public float FireRate;
        public float Cooldown;
        public Entity ProjectilePrefab;
        public float ProjectileSpeed;
        public float ProjectileDamage;
        public float ProjectileLifetime;
        public float ProjectileHitRadius;
    }

    public struct ProjectileData : IComponentData
    {
        public Entity Target;
        public float Speed;
        public float Damage;
        public float HitRadius;
    }

    public struct Lifetime : IComponentData
    {
        public float Remaining;
    }

    public struct WaveSpawnerConfig : IComponentData
    {
        public Entity EnemyPrefab;
        public Entity TowerPrefab;
        public Entity ProjectilePrefab;
        public int EnemiesPerWave;
        public int MaxWaves;
        public float SpawnInterval;
        public float TimeBetweenWaves;
        public float EnemyHealth;
        public float EnemySpeed;
        public float TowerRange;
        public float TowerFireRate;
        public float ProjectileSpeed;
        public float ProjectileDamage;
        public float ProjectileLifetime;
        public float ProjectileHitRadius;
    }

    public struct WaveSpawnerState : IComponentData
    {
        public int SpawnedInWave;
        public int CurrentWave;
        public float SpawnTimer;
        public float WaveCooldown;
        public byte TowersSpawned;
    }

    public struct Waypoint : IBufferElementData
    {
        public float3 Position;
    }

    public struct TowerSpawnPoint : IBufferElementData
    {
        public float3 Position;
    }

    public struct DamageEvent : IBufferElementData
    {
        public float Value;
    }

    public struct PooledEnemy : IComponentData
    {
    }
}
