using NUnit.Framework;
using Unity.Mathematics;
using UnityDotsDemo.Demo05;

namespace UnityDotsDemo.Tests.EditMode
{
    public sealed class FlowFieldTests
    {
        private static readonly FlowFieldGrid TestGrid = new FlowFieldGrid
        {
            Width = 10,
            Height = 10,
            CellSize = 1f,
            WorldOrigin = new float3(-5f, 0f, -5f)
        };

        [Test]
        public void WorldToCell_Origin_ReturnsZeroZero()
        {
            int2 cell = FlowFieldSystem.WorldToCell(TestGrid, new float3(-5f, 0f, -5f));
            Assert.AreEqual(new int2(0, 0), cell);
        }

        [Test]
        public void WorldToCell_Center_ReturnsFiveFive()
        {
            int2 cell = FlowFieldSystem.WorldToCell(TestGrid, new float3(0f, 0f, 0f));
            Assert.AreEqual(new int2(5, 5), cell);
        }

        [Test]
        public void WorldToCell_OutsideGrid_ClampsToValidCell()
        {
            int2 cell = FlowFieldSystem.WorldToCell(TestGrid, new float3(-100f, 0f, -100f));
            Assert.AreEqual(new int2(-95, -95), cell);
        }

        [TestCase(0, 0, 10, 0)]
        [TestCase(5, 5, 10, 55)]
        [TestCase(9, 9, 10, 99)]
        [TestCase(0, 5, 10, 50)]
        public void CellToIndex_ReturnsRowMajorIndex(int x, int z, int width, int expected)
        {
            int index = FlowFieldSystem.CellToIndex(new int2(x, z), width);
            Assert.AreEqual(expected, index);
        }

        [TestCase(0, 10, 0, 0)]
        [TestCase(55, 10, 5, 5)]
        [TestCase(99, 10, 9, 9)]
        [TestCase(50, 10, 0, 5)]
        public void IndexToCell_RoundTrips(int index, int width, int expectedX, int expectedZ)
        {
            int2 cell = FlowFieldSystem.IndexToCell(index, width);
            Assert.AreEqual(new int2(expectedX, expectedZ), cell);
            Assert.AreEqual(index, FlowFieldSystem.CellToIndex(cell, width));
        }

        [Test]
        public void CellCenterToWorld_OriginCell_ReturnsCorrectCenter()
        {
            float2 center = FlowFieldSystem.CellCenterToWorld(TestGrid, new int2(0, 0));
            Assert.AreEqual(new float2(-4.5f, -4.5f), center);
        }

        [Test]
        public void CellCenterToWorld_TopRightCell_ReturnsCorrectCenter()
        {
            float2 center = FlowFieldSystem.CellCenterToWorld(TestGrid, new int2(9, 9));
            Assert.AreEqual(new float2(4.5f, 4.5f), center);
        }
    }
}
