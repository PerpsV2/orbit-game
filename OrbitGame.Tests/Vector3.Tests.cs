namespace OrbitGame.Tests;

public class Vector3_Tests
{
    [Fact]
    public void Vector3_DotMethod()
    {
        Vector3 argument1 = new Vector3(0, 1, 2);
        Vector3 argument2 = new Vector3(3, -4, 5);
        
        Assert.Equal(6, Vector3.Dot(argument1, argument2));
    }

    [Fact]
    public void Vector3_CrossMethod()
    {
        Vector3 argument1 = new Vector3(1, 2, 3);
        Vector3 argument2 = new Vector3(3, 4, 5);
        
        Assert.Equal(new Vector3(-2, 4, -2), Vector3.Cross(argument1, argument2));
    }

    [Fact]
    public void Vector3_Magnitude()
    {
        Vector3 argument1 = new Vector3(0, 0, -3);
        Vector3 argument2 = new Vector3(3, 4, 12);
        
        Assert.Equal(3, argument1.Magnitude());
        Assert.Equal(13, argument2.Magnitude());
    }

    [Fact]
    public void Vector3_Normalize()
    {
        Vector3 argument1 = new Vector3(4, 0, 4);
        Vector3 zeroVector = Vector3.Zero;
        
        AssertExtensions.Equal(new Vector3(Math.Cos(Math.PI / 4), 0, Math.Sin(Math.PI / 4)), argument1.Normalize());
        Assert.Throws<ArithmeticException>(() => zeroVector.Normalize());
    }
    
    #region Operators

    [Fact]
    public void Vector3_NegativeOperator()
    {
        Vector3 argument1 = new Vector3(2, -2, 0);
        Assert.Equal(-new Vector3(-2, 2, 0), argument1);
    }

    [Fact]
    public void Vector3_AdditionOperator()
    {
        Vector3 argument1 = new Vector3(1, 1, 1);
        Vector3 argument2 = new Vector3(-2, 2, -1);
        
        Assert.Equal(new Vector3(-1, 3, 0), argument1 + argument2);
    }

    [Fact]
    public void Vector3_SubtractionOperator()
    {
        Vector3 argument1 = new Vector3(1, 1, 1);
        Vector3 argument2 = new Vector3(-2, 2, -1);
        
        Assert.Equal(new Vector3(3, -1, 2), argument1 - argument2);
    }

    [Fact]
    public void Vector3_ScalarMultiplicationOperator()
    {
        Vector3 vector = new Vector3(1, 1, 1);
        ScientificDecimal scalar = -3;
        Assert.Equal(new Vector3(-3, -3, -3), vector * scalar);
    }

    [Fact]
    public void Vector3_ScalarDivisionOperator()
    {
        Vector3 vector = new Vector3(-3, -3, -3);
        ScientificDecimal scalar = 3;
        Assert.Equal(new Vector3(-1, -1, -1), vector / scalar);
    }

    [Fact]
    public void Vector3_DirectionVectorBetweenMethod()
    {
        Vector3 argument1 = new Vector3(0, 0, 1);
        Vector3 argument2 = new Vector3(1, 0, 0);
        
        AssertExtensions.Equal(new Vector3(Math.Cos(Math.PI / 4), 0, -Math.Sin(Math.PI / 4)),
            Vector3.DirectionVectorBetween(argument1, argument2));
    }
    
    #endregion
}