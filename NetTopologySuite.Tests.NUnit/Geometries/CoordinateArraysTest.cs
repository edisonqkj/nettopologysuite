using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class CoordinateArraysTest
    {
        [Test]
        public void TestPtNotInList1()
        {
            Coordinate list = CoordinateArrays.PointNotInList(
                new[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) },
                new[] { new Coordinate(1, 1), new Coordinate(1, 2), new Coordinate(1, 3) }
                );
            Assert.IsTrue(list.Equals2D(new Coordinate(2, 2)));
        }

        [Test]
        public void TestPtNotInList2()
        {
            Coordinate list = CoordinateArrays.PointNotInList(
                new[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) },
                new[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) }
                );
            Assert.IsTrue(list == null);
        }
    }
}