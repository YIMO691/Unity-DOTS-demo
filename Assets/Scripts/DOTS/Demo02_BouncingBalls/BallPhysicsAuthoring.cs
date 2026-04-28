using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using PhysicsColliderBlob = Unity.Physics.Collider;
using PhysicsMaterial = Unity.Physics.Material;
using PhysicsSphereCollider = Unity.Physics.SphereCollider;

namespace UnityDotsDemo.Demo02
{
    public sealed class BallPhysicsAuthoring : MonoBehaviour
    {
        [Min(0.05f)] public float Radius = 0.5f;
        [Min(0.01f)] public float Mass = 1f;
        [Range(0f, 1f)] public float Restitution = 0.85f;
        [Range(0f, 1f)] public float Friction = 0.05f;
        [Min(0f)] public float LinearDamping = 0.02f;
        [Min(0f)] public float AngularDamping = 0.05f;

        private sealed class Baker : Baker<BallPhysicsAuthoring>
        {
            public override void Bake(BallPhysicsAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                float radius = math.max(0.05f, authoring.Radius);
                float mass = math.max(0.01f, authoring.Mass);

                BlobAssetReference<PhysicsColliderBlob> collider = PhysicsSphereCollider.Create(
                    new SphereGeometry
                    {
                        Center = float3.zero,
                        Radius = radius
                    },
                    CollisionFilter.Default,
                    CreateMaterial(authoring.Friction, authoring.Restitution));

                AddBlobAsset(ref collider, out _);
                AddSharedComponent(entity, new PhysicsWorldIndex());
                AddComponent(entity, new PhysicsCollider { Value = collider });
                AddComponent(entity, PhysicsMass.CreateDynamic(collider.Value.MassProperties, mass));
                AddComponent(entity, new PhysicsVelocity());
                AddComponent(entity, new PhysicsDamping
                {
                    Linear = math.max(0f, authoring.LinearDamping),
                    Angular = math.max(0f, authoring.AngularDamping)
                });
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
