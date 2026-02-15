namespace OrbitGame.Tests;

public class Vector2_Tests
{
    [Fact]
    public void Vector2_FromPolarMethod()
    {
        double angle = Math.PI;
        ScientificDecimal magnitude = 2;
        
        Assert.Equal(new SD_Vector2(-2, 0), SD_Vector2.FromPolar(angle, magnitude));
    }
    
    [Fact]
    public void Vector2_DotMethod()
    {
        SD_Vector2 argument1 = new SD_Vector2(0, 1);
        SD_Vector2 argument2 = new SD_Vector2(2, -4);
        
        Assert.Equal(-4, SD_Vector2.Dot(argument1, argument2));
    }

    [Fact]
    public void Vector2_CrossMethod()
    {
        SD_Vector2 argument1 = new SD_Vector2(1, 2);
        SD_Vector2 argument2 = new SD_Vector2(3, 4);
        
        Assert.Equal(new SD_Vector3(0, 0, -2), SD_Vector2.Cross(argument1, argument2));
    }

    [Fact]
    public void Vector2_Magnitude()
    {
        SD_Vector2 argument1 = new SD_Vector2(0, -3);
        SD_Vector2 argument2 = new SD_Vector2(3, 4);
        
        Assert.Equal(3, argument1.Magnitude());
        Assert.Equal(5, argument2.Magnitude());
    }

    [Fact]
    public void Vector2_Normalize()
    {
        SD_Vector2 argument1 = new SD_Vector2(4, 4);
        SD_Vector2 zeroVector = SD_Vector2.Zero;
        
        Assert.Equal(new SD_Vector2(Math.Cos(Math.PI / 4), Math.Sin(Math.PI / 4)), argument1.Normalize());
        Assert.Throws<ArithmeticException>(() => zeroVector.Normalize());
    }
    
    #region Operators

    [Fact]
    public void Vector2_NegativeOperator()
    {
        SD_Vector2 argument1 = new SD_Vector2(2, -2);
        Assert.Equal(-new SD_Vector2(-2, 2), argument1);
    }

    [Fact]
    public void Vector2_AdditionOperator()
    {
        SD_Vector2 argument1 = new SD_Vector2(1, 1);
        SD_Vector2 argument2 = new SD_Vector2(-2, 2);
        
        Assert.Equal(new SD_Vector2(-1, 3), argument1 + argument2);
    }

    [Fact]
    public void Vector2_SubtractionOperator()
    {
        SD_Vector2 argument1 = new SD_Vector2(1, 1);
        SD_Vector2 argument2 = new SD_Vector2(-2, 2);
        
        Assert.Equal(new SD_Vector2(3, -1), argument1 - argument2);
    }

    [Fact]
    public void Vector2_ScalarMultiplicationOperator()
    {
        SD_Vector2 vector = new SD_Vector2(1, 1);
        ScientificDecimal scalar = -3;
        Assert.Equal(new SD_Vector2(-3, -3), vector * scalar);
    }

    [Fact]
    public void Vector2_ScalarDivisionOperator()
    {
        SD_Vector2 vector = new SD_Vector2(-3, -3);
        ScientificDecimal scalar = 3;
        Assert.Equal(new SD_Vector2(-1, -1), vector / scalar);
    }
    
    #endregion

    [Fact]
    public void Vector2_TriangulateConvexMethod()
    {
        SD_Vector2[] convex = [
            new (2, 2),
            new (2, -2),
            new (-2, -2),
            new (-2, 2)
        ];
        
        Assert.Equal(new[] {
                (new SD_Vector2(2, 2), new SD_Vector2(2, -2), new SD_Vector2(-2, -2)), 
                (new SD_Vector2(2, 2),new SD_Vector2(-2, -2), new SD_Vector2(-2, 2))
            }, SD_Vector2.TriangulateConvex(convex));
    }

    [Fact]
    public void Vector2_CenterOfMassConvexMethod()
    {
        SD_Vector2[] convex = [
            new (4, 4),
            new (4, 0),
            new (0, 0),
            new (0, 4)
        ];
        
        Assert.Equal(new SD_Vector2(2, 2), SD_Vector2.CenterOfMassConvex(convex));
    }

    [Fact]
    public void Vector2_CenterConvexMethod()
    {
        SD_Vector2[] convex =
        [
            new(4, 4),
            new(4, 0),
            new(0, 0),
            new(0, 4)
        ];

        Assert.Equal(new[] {
                new SD_Vector2(2, 2),
                new SD_Vector2(2, -2),
                new SD_Vector2(-2, -2),
                new SD_Vector2(-2, 2)
            }, SD_Vector2.CenterConvex(convex));
    }

    [Fact]
    public void Vector2_TripletRotationDirectionMethod()
    {
        SD_Vector2[] cwConvex = [new(-1, -1), new(0, 1), new(1, 0)];
        SD_Vector2[] ccwConvex = [new(1, 0), new(0, 1), new(-1, -1)];
        SD_Vector2[] noneConvex = [new(-1, -1), new(-1, -1), new(1, 1)];
        SD_Vector2[] errConvex = [];
        
        Assert.Equal(RotationDirection.Clockwise, SD_Vector2.TripletRotationDirection(cwConvex));
        Assert.Equal(RotationDirection.Counterclockwise, SD_Vector2.TripletRotationDirection(ccwConvex));
        Assert.Equal(RotationDirection.None, SD_Vector2.TripletRotationDirection(noneConvex));
        Assert.ThrowsAny<ArgumentException>(() => SD_Vector2.TripletRotationDirection(errConvex));
    }

    [Fact]
    public void Vector2_GetConvexHullIndicesMethod()
    {
        SD_Vector2[] points = [
            new(2,2),
            new(-4, -2),
            new(-3, 0),
            new(-2, 2),
            new (0, 0),
            new (2, -2)
        ];
        
        Assert.Equal(new[] {1, 5, 0, 3, 1}, SD_Vector2.GetConvexHullIndices(points).ToArray());
    }

    [Fact]
    public void Vector2_DirectionVectorBetweenMethod()
    {
        SD_Vector2 argument1 = new SD_Vector2(0, 1);
        SD_Vector2 argument2 = new SD_Vector2(1, 0);
        
        Assert.Equal(new SD_Vector2(Math.Cos(Math.PI / 4), -Math.Sin(Math.PI / 4)),
            SD_Vector2.DirectionVectorBetween(argument1, argument2));
    }

    [Fact]
    public void Vector2_GetPrincipalAngleMethod()
    {
        SD_Vector2 argument1 = new SD_Vector2(Math.Cos(Math.PI / 6), Math.Sin(Math.PI / 6));
        SD_Vector2 argument2 = new SD_Vector2(-Math.Cos(Math.PI / 6), Math.Sin(Math.PI / 6));
        SD_Vector2 argument3 = new SD_Vector2(-Math.Cos(Math.PI / 6), -Math.Sin(Math.PI / 6));
        SD_Vector2 argument4 = new SD_Vector2(Math.Cos(Math.PI / 6), -Math.Sin(Math.PI / 6));
        
        Assert.Equal(Math.PI / 6, argument1.GetPrincipalAngle());
        Assert.Equal(5 * Math.PI / 6, argument2.GetPrincipalAngle());
        Assert.Equal(7 * Math.PI / 6, argument3.GetPrincipalAngle());
        Assert.Equal(11 * Math.PI / 6, argument4.GetPrincipalAngle());
    }
}