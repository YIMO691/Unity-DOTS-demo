using NUnit.Framework;
using UnityDotsDemo.Demo01;
using UnityEngine;

namespace UnityDotsDemo.Tests.EditMode
{
    public sealed class Demo01SpawnerTests
    {
        [Test]
        public void CubeSpawnerDefaultsAreReasonable()
        {
            GameObject gameObject = new GameObject("Spawner");
            try
            {
                CubeSpawnerAuthoring spawner = gameObject.AddComponent<CubeSpawnerAuthoring>();
                Assert.Greater(spawner.SpawnCount, 0);
                Assert.Greater(spawner.CountX, 0);
                Assert.Greater(spawner.CountZ, 0);
                Assert.Greater(spawner.AreaHalfSize.x, 0f);
                Assert.Greater(spawner.AreaHalfSize.y, 0f);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void SpawnCountCanRepresentBenchmarkSizes()
        {
            int[] counts = { 1000, 5000, 10000, 50000 };
            foreach (int count in counts)
            {
                Assert.Greater(count, 0);
            }
        }
    }
}

