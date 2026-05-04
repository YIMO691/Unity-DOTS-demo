using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Templates.DemoTemplate
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
                AddComponent(entity, new TemplateMoveSpeed { Value = authoring.moveSpeed });
                AddComponent(entity, new TemplateDirection { Value = (float3)authoring.direction });
            }
        }
    }
}
