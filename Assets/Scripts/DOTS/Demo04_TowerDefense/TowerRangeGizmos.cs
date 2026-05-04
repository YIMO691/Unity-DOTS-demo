using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace UnityDotsDemo.Demo04
{
    [AddComponentMenu("DOTS Demo/Demo04 Tower Range Gizmos")]
    public sealed class TowerRangeGizmos : MonoBehaviour
    {
        [SerializeField] private Color color = new Color(0.15f, 0.8f, 1f, 0.9f);
        [SerializeField, Range(12, 96)] private int segments = 48;

        private void Update()
        {
            DrawRanges(useGizmos: false);
        }

        private void OnDrawGizmos()
        {
            DrawRanges(useGizmos: true);
        }

        private void DrawRanges(bool useGizmos)
        {
            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                return;
            }

            EntityManager entityManager = world.EntityManager;
            using EntityQuery query = entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<TowerTag>(),
                ComponentType.ReadOnly<TowerAttack>(),
                ComponentType.ReadOnly<LocalToWorld>());

            NativeArray<TowerAttack> attacks = query.ToComponentDataArray<TowerAttack>(Allocator.Temp);
            NativeArray<LocalToWorld> transforms = query.ToComponentDataArray<LocalToWorld>(Allocator.Temp);

            if (useGizmos)
            {
                Gizmos.color = color;
            }

            int segmentCount = Mathf.Max(12, segments);
            for (int towerIndex = 0; towerIndex < attacks.Length; towerIndex++)
            {
                Vector3 center = transforms[towerIndex].Position;
                float radius = attacks[towerIndex].Range;
                Vector3 previous = center + new Vector3(radius, 0.08f, 0f);

                for (int i = 1; i <= segmentCount; i++)
                {
                    float angle = (Mathf.PI * 2f * i) / segmentCount;
                    Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0.08f, Mathf.Sin(angle) * radius);
                    if (useGizmos)
                    {
                        Gizmos.DrawLine(previous, next);
                    }
                    else
                    {
                        Debug.DrawLine(previous, next, color);
                    }

                    previous = next;
                }
            }

            attacks.Dispose();
            transforms.Dispose();
        }
    }
}
