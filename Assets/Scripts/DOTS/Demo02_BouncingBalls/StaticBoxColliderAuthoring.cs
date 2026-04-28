using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using PhysicsBoxCollider = Unity.Physics.BoxCollider;
using PhysicsColliderBlob = Unity.Physics.Collider;
using PhysicsMaterial = Unity.Physics.Material;

namespace UnityDotsDemo.Demo02
{
    public sealed class StaticBoxColliderAuthoring : MonoBehaviour
    {
        public Vector3 Size = Vector3.one;
        [Range(0f, 1f)] public float Restitution = 0.8f;
        [Range(0f, 1f)] public float Friction = 0.1f;

        private sealed class Baker : Baker<StaticBoxColliderAuthoring>
        {
            public override void Bake(StaticBoxColliderAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.WorldSpace);
                float3 size = math.max(new float3(0.01f), (float3)authoring.Size);

                BlobAssetReference<PhysicsColliderBlob> collider = PhysicsBoxCollider.Create(
                    new BoxGeometry
                    {
                        Center = float3.zero,
                        Orientation = quaternion.identity,
                        Size = size,
                        BevelRadius = 0f
                    },
                    CollisionFilter.Default,
                    CreateMaterial(authoring.Friction, authoring.Restitution));

                AddBlobAsset(ref collider, out _);
                AddSharedComponent(entity, new PhysicsWorldIndex());
                AddComponent(entity, new PhysicsCollider { Value = collider });
                AddComponent<Simulate>(entity);
            }
        }

        private static PhysicsMaterial CreateMaterial(float friction, float restitution)
        {
            PhysicsMaterial material = PhysicsMaterial.Default;
            material.Friction = math.clamp(friction, 0f, 1f);
            material.Restitution = math.clamp(restitution, 0f, 1f);
            material.FrictionCombinePolicy = PhysicsMaterial.CombinePolicy.Minimum;
            material.RestitutionCombinePolicy = PhysicsMaterial.CombinePolicy.Maximum;
            material.CollisionResponse = CollisionResponsePolicy.Collide;
            return material;
        }
    }
}
