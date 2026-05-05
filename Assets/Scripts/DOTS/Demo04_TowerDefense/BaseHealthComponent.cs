using Unity.Entities;

namespace UnityDotsDemo.Demo04
{
    public struct BaseHealth : IComponentData
    {
        public int CurrentHP;
        public int MaxHP;
    }

    public struct EnemyReachedBase : IComponentData
    {
    }
}
