using Xunit;

namespace OrbitGame.Tests;

public class BoundingBox_Tests
{
    private readonly BoundingBox _testPointBoundingBox = new(SD_Vector2.Zero, 0, 0);
    private readonly BoundingBox _testLineBoundingBox = new(SD_Vector2.Zero, 0, 10);
    private readonly BoundingBox _testBoundingBox = new(SD_Vector2.Zero, 10, 10);

    [Fact]
    public void BoundingBox_IsEmptyMethod()
    {
        Assert.True(_testPointBoundingBox.IsEmpty());
        Assert.True(_testLineBoundingBox.IsEmpty());
        Assert.False(_testBoundingBox.IsEmpty());
    }

    [Fact]
    public void BoundingBox_IntersectsWithMethod()
    {
        SpatialInfo originSpatialInfo = new(SD_Vector2.Zero, 0);
        SpatialInfo overlappingSpatialInfo = new(new(1, 0), 0);
        SpatialInfo touchingSpatialInfo = new(new(5, 0), 0);
        SpatialInfo notTouchingSpatialInfo = new(new(10, 0), 0);
        
        Assert.False(_testBoundingBox.IntersectsWith(_testPointBoundingBox, originSpatialInfo, originSpatialInfo));
        Assert.True(_testBoundingBox.IntersectsWith(_testBoundingBox, originSpatialInfo, overlappingSpatialInfo));
        Assert.True(_testBoundingBox.IntersectsWith(_testBoundingBox, originSpatialInfo, touchingSpatialInfo));
        Assert.True(_testBoundingBox.IntersectsWith(_testBoundingBox, originSpatialInfo, notTouchingSpatialInfo));
    }
}