namespace Xunit
{
    public abstract partial class Assert
    {
        /// <summary>
        /// Assert equality of two collision manifolds
        /// </summary>
        public static void Equal(HashSet<OrbitGame.SD_Vector2> left, HashSet<OrbitGame.SD_Vector2> right)
        {
            Equal(left.Count, right.Count);
            for (int i = 0; i < left.Count; ++i)
            {
                Equal(left.ElementAt(i), right.ElementAt(i));
            }
        }
        
        public static void Equal(OrbitGame.PhysicsCollision left, OrbitGame.PhysicsCollision right)
        {
            Equal(left.Incident, right.Incident);
            Equal(left.Reference, right.Reference);
            Equal(left.CollisionManifold, right.CollisionManifold);
            Equal(left.PenetrationVector, right.PenetrationVector);
        }
    }
}

namespace OrbitGame.Tests
{
    public class CircularCollider_Tests
    {
        private readonly CircularCollider _testEmptyCollider = new(0);
        private readonly CircularCollider _testUnitCollider = new(1);
        private readonly ConvexCollider _testConvexCollider = new([new(1, 2), new(1, -2), new(-1, -2), new(-1, 2)]);
        private readonly ScientificDecimal _testMass = 10;

        [Fact]
        public void CircularCollider_CalculateInertiaMethod()
        {
            Assert.Equal(5, _testUnitCollider.CalculateInertia(_testMass));
        }

        [Fact]
        public void CircularCollider_IsEmptyMethod()
        {
            Assert.True(_testEmptyCollider.IsEmpty());
            Assert.False(_testUnitCollider.IsEmpty());
        }

        [Fact]
        public void CircularCollider_IntersectsWithPointMethod()
        {
            SpatialInfo originSpatialInfo = new(SD_Vector2.Zero, 0);
            SpatialInfo touchingSpatialInfo = new(new SD_Vector2(1, 0), 0);
            SpatialInfo notTouchingSpatialInfo = new(new SD_Vector2(2, 0), 0);

            Assert.Equal(new PointCollision(false),
                _testEmptyCollider.IntersectsWith(SD_Vector2.Zero, originSpatialInfo));
            Assert.Equal(new PointCollision(true),
                _testUnitCollider.IntersectsWith(SD_Vector2.Zero, originSpatialInfo));
            Assert.Equal(new PointCollision(true),
                _testUnitCollider.IntersectsWith(SD_Vector2.Zero, touchingSpatialInfo));
            Assert.Equal(new PointCollision(false),
                _testUnitCollider.IntersectsWith(SD_Vector2.Zero, notTouchingSpatialInfo));
        }

        [Fact]
        public void CircularCollider_IntersectsWithCircularMethod()
        {
            SpatialInfo originSpatialInfo = new(SD_Vector2.Zero, 0);
            SpatialInfo overlappingSpatialInfo = new(new SD_Vector2(1, 0), 0);
            SpatialInfo touchingSpatialInfo = new(new SD_Vector2(2, 0), 0);
            SpatialInfo notTouchingSpatialInfo = new(new SD_Vector2(3, 0), 0);

            PhysicsCollision? overlappingIntersection = _testUnitCollider
                .IntersectsWith(_testUnitCollider, originSpatialInfo, overlappingSpatialInfo);
            PhysicsCollision? touchingIntersection = _testUnitCollider
                .IntersectsWith(_testUnitCollider, originSpatialInfo, touchingSpatialInfo);
            PhysicsCollision? notTouchingIntersection = _testUnitCollider
                .IntersectsWith(_testUnitCollider, originSpatialInfo, notTouchingSpatialInfo);

            Assert.Null(_testEmptyCollider.IntersectsWith(_testUnitCollider, originSpatialInfo, originSpatialInfo));
            Assert.NotNull(_testUnitCollider.IntersectsWith(_testUnitCollider, originSpatialInfo, originSpatialInfo));

            if (overlappingIntersection.HasValue)
                Assert.Equal(new PhysicsCollision(originSpatialInfo, overlappingSpatialInfo, [new(1, 0)], new(-1, 0)),
                    overlappingIntersection.Value);

            if (touchingIntersection.HasValue)
                Assert.Equal(new PhysicsCollision(originSpatialInfo, touchingSpatialInfo, [new(1, 0)], new(0, 0)),
                    touchingIntersection.Value);

            Assert.Null(notTouchingIntersection);
        }

        [Fact]
        public void CircularCollider_IntersectsWithConvexMethod()
        {
            SpatialInfo originSpatialInfo = new(SD_Vector2.Zero, 0);
            SpatialInfo overlappingSpatialInfo = new(new SD_Vector2(2, 0), Math.PI / 2);
            SpatialInfo touchingSpatialInfo = new(new SD_Vector2(2, 0), 0);
            SpatialInfo notTouchingSpatialInfo = new(new SD_Vector2(3, 0), 0);

            PhysicsCollision? overlappingIntersection = _testUnitCollider
                .IntersectsWith(_testConvexCollider, originSpatialInfo, overlappingSpatialInfo);
            PhysicsCollision? touchingIntersection = _testUnitCollider
                .IntersectsWith(_testConvexCollider, originSpatialInfo, touchingSpatialInfo);
            PhysicsCollision? notTouchingIntersection = _testUnitCollider
                .IntersectsWith(_testConvexCollider, originSpatialInfo, notTouchingSpatialInfo);

            Assert.Null(_testEmptyCollider.IntersectsWith(_testConvexCollider, originSpatialInfo, originSpatialInfo));

            if (!overlappingIntersection.HasValue)
            {
                Assert.Fail("Null collision");
                return;
            }
            Assert.Equal(new PhysicsCollision(originSpatialInfo, overlappingSpatialInfo, [new(1, 0)], new(-1, 0)),
                overlappingIntersection.Value);

            if (!touchingIntersection.HasValue)
            {
                Assert.Fail("Null collision");
                return;
            }
            Assert.Equal(new PhysicsCollision(originSpatialInfo, touchingSpatialInfo, [new(1, 0)], new(0, 0)),
                touchingIntersection.Value);

            Assert.Null(notTouchingIntersection);
        }
    }
}