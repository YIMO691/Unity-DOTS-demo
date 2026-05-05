using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UnityDotsDemo.Demo04
{
    public sealed class WaveSpawnerAuthoring : MonoBehaviour
    {
        public GameObject EnemyPrefab;
        public GameObject TowerPrefab;
        public GameObject ProjectilePrefab;
        public List<Transform> Waypoints = new List<Transform>();
        public List<Transform> TowerPositions = new List<Transform>();
        public List<WaveDefinitionAuthoring> Waves = new List<WaveDefinitionAuthoring>();
        [Min(1)] public int EnemiesPerWave = 12;
        [Min(1)] public int MaxWaves = 5;
        [Min(0.05f)] public float SpawnInterval = 0.45f;
        [Min(0f)] public float TimeBetweenWaves = 2f;
        [Min(1f)] public float EnemyHealth = 30f;
        [Min(0.1f)] public float EnemySpeed = 3f;
        [Min(0.1f)] public float TowerRange = 7f;
        [Min(0.1f)] public float TowerFireRate = 1.25f;
        [Min(0.1f)] public float ProjectileSpeed = 12f;
        [Min(0.1f)] public float ProjectileDamage = 10f;
        [Min(0.1f)] public float ProjectileLifetime = 3f;
        [Min(0.05f)] public float ProjectileHitRadius = 0.35f;

        private sealed class WaveSpawnerBaker : Baker<WaveSpawnerAuthoring>
        {
            public override void Bake(WaveSpawnerAuthoring authoring)
            {
                if (authoring.EnemyPrefab == null ||
                    authoring.TowerPrefab == null ||
                    authoring.ProjectilePrefab == null)
                {
                    return;
                }

                Entity spawnerEntity = GetEntity(TransformUsageFlags.None);

                AddComponent(spawnerEntity, new WaveSpawnerConfig
                {
                    EnemyPrefab = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic),
                    TowerPrefab = GetEntity(authoring.TowerPrefab, TransformUsageFlags.Dynamic),
                    ProjectilePrefab = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.Dynamic),
                    EnemiesPerWave = math.max(1, authoring.EnemiesPerWave),
                    MaxWaves = math.max(1, authoring.MaxWaves),
                    SpawnInterval = math.max(0.05f, authoring.SpawnInterval),
                    TimeBetweenWaves = math.max(0f, authoring.TimeBetweenWaves),
                    EnemyHealth = math.max(1f, authoring.EnemyHealth),
                    EnemySpeed = math.max(0.1f, authoring.EnemySpeed),
                    TowerRange = math.max(0.1f, authoring.TowerRange),
                    TowerFireRate = math.max(0.1f, authoring.TowerFireRate),
                    ProjectileSpeed = math.max(0.1f, authoring.ProjectileSpeed),
                    ProjectileDamage = math.max(0.1f, authoring.ProjectileDamage),
                    ProjectileLifetime = math.max(0.1f, authoring.ProjectileLifetime),
                    ProjectileHitRadius = math.max(0.05f, authoring.ProjectileHitRadius)
                });

                AddComponent(spawnerEntity, new WaveSpawnerState());
                DynamicBuffer<WaveDefinition> waveDefinitions = AddBuffer<WaveDefinition>(spawnerEntity);
                if (authoring.Waves != null && authoring.Waves.Count > 0)
                {
                    foreach (WaveDefinitionAuthoring wave in authoring.Waves)
                    {
                        WaveDefinition runtimeWave = wave.ToRuntime(
                            math.max(1f, authoring.EnemyHealth),
                            math.max(0.1f, authoring.EnemySpeed));
                        if (runtimeWave.TotalCount > 0)
                        {
                            waveDefinitions.Add(runtimeWave);
                        }
                    }
                }

                if (waveDefinitions.Length == 0)
                {
                    AddDefaultWaves(
                        waveDefinitions,
                        math.max(1f, authoring.EnemyHealth),
                        math.max(0.1f, authoring.EnemySpeed));
                }

                DynamicBuffer<Waypoint> waypoints = AddBuffer<Waypoint>(spawnerEntity);
                if (authoring.Waypoints != null && authoring.Waypoints.Count > 0)
                {
                    foreach (Transform waypoint in authoring.Waypoints)
                    {
                        if (waypoint != null)
                        {
                            waypoints.Add(new Waypoint { Position = (float3)waypoint.position });
                        }
                    }
                }

                if (waypoints.Length == 0)
                {
                    waypoints.Add(new Waypoint { Position = new float3(-12f, 0f, -5f) });
                    waypoints.Add(new Waypoint { Position = new float3(-4f, 0f, 5f) });
                    waypoints.Add(new Waypoint { Position = new float3(5f, 0f, 5.5f) });
                    waypoints.Add(new Waypoint { Position = new float3(12f, 0f, -3f) });
                }

                DynamicBuffer<TowerSpawnPoint> towerPoints = AddBuffer<TowerSpawnPoint>(spawnerEntity);
                if (authoring.TowerPositions != null && authoring.TowerPositions.Count > 0)
                {
                    foreach (Transform towerPosition in authoring.TowerPositions)
                    {
                        if (towerPosition != null)
                        {
                            towerPoints.Add(new TowerSpawnPoint { Position = (float3)towerPosition.position });
                        }
                    }
                }

                if (towerPoints.Length == 0)
                {
                    towerPoints.Add(new TowerSpawnPoint { Position = new float3(-7f, 0f, 0f) });
                    towerPoints.Add(new TowerSpawnPoint { Position = new float3(5.5f, 0f, -4.5f) });
                    towerPoints.Add(new TowerSpawnPoint { Position = new float3(8f, 0f, 0.5f) });
                }

                AddBuffer<Entity>(spawnerEntity);
            }

            private static void AddDefaultWaves(
                DynamicBuffer<WaveDefinition> waves,
                float baseHealth,
                float baseSpeed)
            {
                waves.Add(CreateWave(5, 0, 0, baseHealth, baseSpeed));
                waves.Add(CreateWave(8, 0, 0, baseHealth * 1.15f, baseSpeed));
                waves.Add(CreateWave(5, 3, 0, baseHealth * 1.25f, baseSpeed));
                waves.Add(CreateWave(10, 5, 0, baseHealth * 1.4f, baseSpeed));
                waves.Add(CreateWave(8, 5, 2, baseHealth * 1.6f, baseSpeed));
            }

            private static WaveDefinition CreateWave(
                int normal,
                int fast,
                int boss,
                float normalHealth,
                float normalSpeed)
            {
                return new WaveDefinition
                {
                    NormalCount = normal,
                    FastCount = fast,
                    BossCount = boss,
                    NormalHealth = normalHealth,
                    NormalSpeed = normalSpeed,
                    FastHealth = normalHealth * 0.7f,
                    FastSpeed = normalSpeed * 1.6f,
                    BossHealth = normalHealth * 4f,
                    BossSpeed = normalSpeed * 0.65f
                };
            }
        }
    }
}
