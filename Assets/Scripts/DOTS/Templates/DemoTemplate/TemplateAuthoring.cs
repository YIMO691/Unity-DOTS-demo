using DOTSDemo.Shared;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UnityDotsDemo.Template
{
    public class TemplateAuthoring : MonoBehaviour
    {
        public float moveSpeed = 3f;
        public Vector3 direction = Vector3.forward;

        private class TemplateBaker : Baker<TemplateAuthoring>
        {
            public override void Bake(TemplateAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<TemplateTag>(entity);
                AddComponent(entity, new MoveSpeed { Value = authoring.moveSpeed });
                AddComponent(entity, new Velocity { Value = (float3)authoring.direction });
            }
        }
    }
}
