using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Templates.DemoTemplate
{
    public struct TemplateTag : IComponentData
    {
    }

    public struct TemplateMoveSpeed : IComponentData
    {
        public float Value;
    }

    public struct TemplateDirection : IComponentData
    {
        public float3 Value;
    }
}
