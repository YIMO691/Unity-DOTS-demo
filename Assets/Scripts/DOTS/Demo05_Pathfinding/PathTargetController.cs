using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UnityDotsDemo.Demo05
{
    public sealed class PathTargetController : MonoBehaviour
    {
        private EntityQuery _gridQuery;

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane ground = new Plane(Vector3.up, Vector3.zero);
                if (ground.Raycast(ray, out float distance))
                {
                    Vector3 hitPoint = ray.GetPoint(distance);
                    SetTarget(hitPoint);
                }
            }
        }

        private void SetTarget(Vector3 worldPosition)
        {
            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                return;
            }

            EntityManager em = world.EntityManager;
            if (_gridQuery == default)
            {
                _gridQuery = em.CreateEntityQuery(
                    ComponentType.ReadWrite<PathTarget>(),
                    ComponentType.ReadOnly<FlowFieldGrid>());
            }

            if (!_gridQuery.IsEmpty)
            {
                Entity gridEntity = _gridQuery.GetSingletonEntity();
                em.SetComponentData(gridEntity, new PathTarget
                {
                    Position = worldPosition
                });
            }
        }
    }
}
