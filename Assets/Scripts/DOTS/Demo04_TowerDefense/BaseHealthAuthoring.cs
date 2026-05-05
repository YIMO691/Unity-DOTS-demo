using Unity.Entities;
using UnityEngine;

namespace UnityDotsDemo.Demo04
{
    [DisallowMultipleComponent]
    public sealed class BaseHealthAuthoring : MonoBehaviour
    {
        [Min(1)] public int MaxHP = 20;

        private sealed class BaseHealthBaker : Baker<BaseHealthAuthoring>
        {
            public override void Bake(BaseHealthAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                int maxHp = Mathf.Max(1, authoring.MaxHP);
                AddComponent(entity, new BaseHealth
                {
                    CurrentHP = maxHp,
                    MaxHP = maxHp
                });
            }
        }
    }
}
