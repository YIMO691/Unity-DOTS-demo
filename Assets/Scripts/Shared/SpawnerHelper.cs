using System;
using Unity.Collections;
using Unity.Entities;

namespace DOTSDemo.Shared
{
    public struct SpawnerHelper : IDisposable
    {
        public readonly EntityCommandBuffer Ecb;
        private readonly EntityManager _entityManager;

        public SpawnerHelper(EntityManager entityManager, Allocator allocator)
        {
            _entityManager = entityManager;
            Ecb = new EntityCommandBuffer(allocator);
        }

        public void DestroySpawner(Entity spawnerEntity)
        {
            Ecb.DestroyEntity(spawnerEntity);
        }

        public void Dispose()
        {
            Ecb.Playback(_entityManager);
            if (Ecb.IsCreated)
            {
                Ecb.Dispose();
            }
        }
    }
}
