namespace OrbitGame.Tests;

public class Utils_Tests
{
    private readonly SD_Vector2[] _testEmptyHull = [];
    private readonly SD_Vector2[] _testConvexHull = [new(4, 4), new(4, 0), new(0, 0), new(0, 4)];
    private readonly SD_Vector2[] _testCWConvexHull = [new(-1, -1), new(0, 1), new(1, 0)];
    private readonly SD_Vector2[] _testCCWConvexHull = [new(-1, -1), new(1, 0), new(0, 1)];
    private readonly SD_Vector2[] _testNoRotationConvexHull = [new(-1, -1), new(-1, -1), new(1, 1)];
    private readonly SD_Vector2[] _testNonConvexHull =
    [
        new(2, 2),
        new(-4, -2),
        new(-3, 0),
        new(-2, 2),
        new(0, 0),
        new(2, -2)
    ];
    
    [Fact]
    public void Utils_ClampMethod()
    {
        int clampedInt = Utils.Clamp(-10, -20, 30);
        double clampedDouble = Utils.Clamp(-30, 20, 30);
        ScientificDecimal clampedScientificDecimal = Utils.Clamp(
            ScientificDecimal.NegInfinity, 
            ScientificDecimal.NegInfinity, 
            30
            );
        
        Assert.Equal(-10, clampedInt);
        Assert.Equal(20, clampedDouble);
        Assert.Equal(ScientificDecimal.NegInfinity, clampedScientificDecimal);
    }

    [Fact]
    public void Utils_UnsignedModMethod()
    {
        double unsignedMod1 = Utils.UnsignedMod(0.5, 1);
        double unsignedMod2 = Utils.UnsignedMod(5 * Math.PI, Math.Tau);
        double unsignedMod3 = Utils.UnsignedMod(-Math.PI / 2, Math.Tau);
        
        Assert.Equal(0.5, unsignedMod1);
        Assert.Equal(Math.PI, unsignedMod2);
        Assert.Equal(3 * Math.PI / 2, unsignedMod3);
    }

    [Fact]
    public void Utils_GetMinAngleRangeMethod()
    {
        double deg45 = 1 * Math.PI / 4;
        double deg90 = Math.PI / 2;
        double deg135 = 3 * Math.PI / 4;
        double deg180 = Math.PI;
        double deg225 = 5 * Math.PI / 4;
        double deg270 = 3 * Math.PI / 2;
        double deg315 = 7 * Math.PI / 4;
        double deg360 = Math.Tau;
       
        Utils.GetMinAngleRange(out double minAngle, out double maxAngle, deg360, deg180);
        Assert.Equal(0, minAngle);
        Assert.Equal(deg180, maxAngle, Assert.Epsilon);
        
        Utils.GetMinAngleRange(out minAngle, out maxAngle, deg180, deg225, deg270);
        Assert.Equal(deg180, minAngle, Assert.Epsilon);
        Assert.Equal(deg270, maxAngle, Assert.Epsilon);
        
        Utils.GetMinAngleRange(out minAngle, out maxAngle, deg225, deg135, deg180, deg45, deg360, deg315);
        Assert.Equal(deg135, minAngle, Assert.Epsilon);
        Assert.Equal(deg45, maxAngle, Assert.Epsilon);
    }
    
    [Fact]
    public void Utils_IterateAngleRangeMethod()
    {
        double deg45 = Math.PI / 4;
        double deg315 = 7 * Math.PI / 4;
        
        List<double> iteratedAngles = new List<double>();
        
        Utils.IterateAngleRange(deg315, deg45, deg45, angle => iteratedAngles.Add(angle));
        Assert.Equal([7*Math.PI/4, 0, Math.PI/4], iteratedAngles);
        
        iteratedAngles.Clear();
        
        Utils.IterateAngleRange(deg45, deg315, deg45, angle => iteratedAngles.Add(angle));
        Assert.Equal([Math.PI/4, Math.PI/2, 3*Math.PI/4, Math.PI, 5*Math.PI/4, 3*Math.PI/2, 7*Math.PI/4], iteratedAngles);
    }

    [Fact]
    public void Utils_AngleInRangeMethod()
    {
        double minAngle = 7 * Math.PI / 4;
        double maxAngle = Math.PI / 4;
        
        Assert.True(Utils.AngleInRange(0, minAngle, maxAngle));
        Assert.False(Utils.AngleInRange(0, maxAngle, minAngle));
    }

    [Fact]
    public void Utils_DecimalSqrtMethod()
    {
        Assert.Equal(4, Utils.DecimalSqrt(16));
        Assert.Equal(Math.Sqrt(2d), Utils.DecimalSqrt(2), Assert.Epsilon);
        Assert.Throws<ArithmeticException>(() => Utils.DecimalSqrt(-1));
    }

    [Fact]
    public void Utils_CalculateTriangleAreaMethod()
    {
        Assert.Equal(3, Utils.CalculateTriangleArea(new(-1, 5), new(2, 5), new(0, 3)));
        Assert.Equal(3, Utils.CalculateTriangleArea(new(-1, 5), new(0, 3), new(2, 5)));
        Assert.Equal(0, Utils.CalculateTriangleArea(new(-1, 5), new(0, 3), new(0, 3)));
    }

    [Fact]
    public void Utils_LogEnumerableMethod()
    {
        Utils.LogEnumerable(_testConvexHull);
        throw new NotImplementedException();
    }
    
    [Fact]
    public void Utils_TriangulateConvexMethod()
    {
        Assert.Throws<ArgumentException>(() => Utils.TriangulateConvex(_testEmptyHull));
        Assert.Equal(new[] {
            (new SD_Vector2(4, 4), new SD_Vector2(4, 0), new SD_Vector2(0, 0)), 
            (new SD_Vector2(4, 4), new SD_Vector2(0, 0), new SD_Vector2(0, 4))
        }, Utils.TriangulateConvex(_testConvexHull));
    }

    [Fact]
    public void Utils_CenterOfMassConvexMethod()
    {
        Assert.Equal(new SD_Vector2(2, 2), Utils.CenterOfMassConvex(_testConvexHull));
    }

    [Fact]
    public void Utils_CenterConvexMethod()
    {
        Assert.Equal([
                new SD_Vector2(2, 2),
                new SD_Vector2(2, -2),
                new SD_Vector2(-2, -2),
                new SD_Vector2(-2, 2)
            ],
            Utils.CenterConvex(_testConvexHull)
        );
    }

    [Fact]
    public void Utils_TripletRotationDirectionMethod()
    {
        Assert.Equal(RotationDirection.Clockwise, Utils.TripletRotationDirection(_testCWConvexHull));
        Assert.Equal(RotationDirection.Counterclockwise, Utils.TripletRotationDirection(_testCCWConvexHull));
        Assert.Equal(RotationDirection.None, Utils.TripletRotationDirection(_testNoRotationConvexHull));
        Assert.ThrowsAny<ArgumentException>(() => Utils.TripletRotationDirection([]));
    }

    [Fact]
    public void Vector2_GetConvexHullIndicesMethod()
    {
        Assert.Equal([1, 5, 0, 3, 1], Utils.GetConvexHullIndices(_testNonConvexHull).ToArray());
    }
}