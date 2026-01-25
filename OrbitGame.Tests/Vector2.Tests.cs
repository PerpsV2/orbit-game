namespace OrbitGame.Tests;

public class Vector2_Tests
{
    [Fact]
    public void Vector2_FromPolarMethod()
    {
        double angle = Math.PI;
        ScientificDecimal magnitude = 2;
        
        AssertExtensions.Equal(new Vector2(-2, 0), Vector2.FromPolar(angle, magnitude));
    }
    
    [Fact]
    public void Vector2_DotMethod()
    {
        Vector2 argument1 = new Vector2(0, 1);
        Vector2 argument2 = new Vector2(2, -4);
        
        Assert.Equal(-4, Vector2.Dot(argument1, argument2));
    }

    [Fact]
    public void Vector2_CrossMethod()
    {
        Vector2 argument1 = new Vector2(1, 2);
        Vector2 argument2 = new Vector2(3, 4);
        
        Assert.Equal(new Vector3(0, 0, -2), Vector2.Cross(argument1, argument2));
    }

    [Fact]
    public void Vector2_Magnitude()
    {
        Vector2 argument1 = new Vector2(0, -3);
        Vector2 argument2 = new Vector2(3, 4);
        
        Assert.Equal(3, argument1.Magnitude());
        Assert.Equal(5, argument2.Magnitude());
    }

    [Fact]
    public void Vector2_Normalize()
    {
        Vector2 argument1 = new Vector2(4, 4);
        Vector2 zeroVector = Vector2.Zero;
        
        AssertExtensions.Equal(new Vector2(Math.Cos(Math.PI / 4), Math.Sin(Math.PI / 4)), argument1.Normalize());
        Assert.Throws<ArithmeticException>(() => zeroVector.Normalize());
    }
    
    #region Operators

    [Fact]
    public void Vector2_NegativeOperator()
    {
        Vector2 argument1 = new Vector2(2, -2);
        Assert.Equal(-new Vector2(-2, 2), argument1);
    }

    [Fact]
    public void Vector2_AdditionOperator()
    {
        Vector2 argument1 = new Vector2(1, 1);
        Vector2 argument2 = new Vector2(-2, 2);
        
        Assert.Equal(new Vector2(-1, 3), argument1 + argument2);
    }

    [Fact]
    public void Vector2_SubtractionOperator()
    {
        Vector2 argument1 = new Vector2(1, 1);
        Vector2 argument2 = new Vector2(-2, 2);
        
        Assert.Equal(new Vector2(3, -1), argument1 - argument2);
    }

    [Fact]
    public void Vector2_ScalarMultiplicationOperator()
    {
        Vector2 vector = new Vector2(1, 1);
        ScientificDecimal scalar = -3;
        Assert.Equal(new Vector2(-3, -3), vector * scalar);
    }

    [Fact]
    public void Vector2_ScalarDivisionOperator()
    {
        Vector2 vector = new Vector2(-3, -3);
        ScientificDecimal scalar = 3;
        Assert.Equal(new Vector2(-1, -1), vector / scalar);
    }
    
    #endregion

    [Fact]
    public void Vector2_TriangulateConvexMethod()
    {
        Vector2[] convex = [
            new (2, 2),
            new (2, -2),
            new (-2, -2),
            new (-2, 2)
        ];
        
        Assert.Equal(new[] {
                (new Vector2(2, 2), new Vector2(2, -2), new Vector2(-2, -2)), 
                (new Vector2(2, 2),new Vector2(-2, -2), new Vector2(-2, 2))
            }, Vector2.TriangulateConvex(convex));
    }

    [Fact]
    public void Vector2_CenterOfMassConvexMethod()
    {
        Vector2[] convex = [
            new (4, 4),
            new (4, 0),
            new (0, 0),
            new (0, 4)
        ];
        
        Assert.Equal(new Vector2(2, 2), Vector2.CenterOfMassConvex(convex));
    }

    [Fact]
    public void Vector2_CenterConvexMethod()
    {
        Vector2[] convex =
        [
            new(4, 4),
            new(4, 0),
            new(0, 0),
            new(0, 4)
        ];

        Assert.Equal(new[] {
                new Vector2(2, 2),
                new Vector2(2, -2),
                new Vector2(-2, -2),
                new Vector2(-2, 2)
            }, Vector2.CenterConvex(convex));
    }

    [Fact]
    public void Vector2_TripletRotationDirectionMethod()
    {
        Vector2[] cwConvex = [new(-1, -1), new(0, 1), new(1, 0)];
        Vector2[] ccwConvex = [new(1, 0), new(0, 1), new(-1, -1)];
        Vector2[] noneConvex = [new(-1, -1), new(-1, -1), new(1, 1)];
        Vector2[] errConvex = [];
        
        Assert.Equal(RotationDirection.Clockwise, Vector2.TripletRotationDirection(cwConvex));
        Assert.Equal(RotationDirection.Counterclockwise, Vector2.TripletRotationDirection(ccwConvex));
        Assert.Equal(RotationDirection.None, Vector2.TripletRotationDirection(noneConvex));
        Assert.ThrowsAny<ArgumentException>(() => Vector2.TripletRotationDirection(errConvex));
    }

    [Fact]
    public void Vector2_GetConvexHullIndicesMethod()
    {
        Vector2[] points = [
            new(2,2),
            new(-4, -2),
            new(-3, 0),
            new(-2, 2),
            new (0, 0),
            new (2, -2)
        ];
        
        Assert.Equal(new[] {1, 5, 0, 3, 1}, Vector2.GetConvexHullIndices(points).ToArray());
    }

    [Fact]
    public void Vector2_DirectionVectorBetweenMethod()
    {
        Vector2 argument1 = new Vector2(0, 1);
        Vector2 argument2 = new Vector2(1, 0);
        
        AssertExtensions.Equal(new Vector2(Math.Cos(Math.PI / 4), -Math.Sin(Math.PI / 4)),
            Vector2.DirectionVectorBetween(argument1, argument2));
    }

    [Fact]
    public void Vector2_GetPrincipalAngleMethod()
    {
        Vector2 argument1 = new Vector2(Math.Cos(Math.PI / 6), Math.Sin(Math.PI / 6));
        Vector2 argument2 = new Vector2(-Math.Cos(Math.PI / 6), Math.Sin(Math.PI / 6));
        Vector2 argument3 = new Vector2(-Math.Cos(Math.PI / 6), -Math.Sin(Math.PI / 6));
        Vector2 argument4 = new Vector2(Math.Cos(Math.PI / 6), -Math.Sin(Math.PI / 6));
        
        AssertExtensions.Equal(Math.PI / 6, argument1.GetPrincipalAngle());
        AssertExtensions.Equal(5 * Math.PI / 6, argument2.GetPrincipalAngle());
        AssertExtensions.Equal(7 * Math.PI / 6, argument3.GetPrincipalAngle());
        AssertExtensions.Equal(11 * Math.PI / 6, argument4.GetPrincipalAngle());
    }
}