using NUnit.Framework;
using Unity.Mathematics;
using UnityDotsDemo.Demo03;

namespace UnityDotsDemo.Tests.EditMode
{
    public sealed class SpatialHashTests
    {
        [TestCase(0f, 0f, 4f, 0, 0)]
        [TestCase(-1f, -1f, 4f, -1, -1)]
        [TestCase(100.5f, -50.3f, 5f, 20, -11)]
        public void GetCellReturnsExpectedCoordinates(float x, float z, float cellSize, int expectedX, int expectedZ)
        {
            int2 cell = SpatialHashBoidSystem.GetCell(new float3(x, 0f, z), cellSize);
            Assert.AreEqual(new int2(expectedX, expectedZ), cell);
        }
    }
}
