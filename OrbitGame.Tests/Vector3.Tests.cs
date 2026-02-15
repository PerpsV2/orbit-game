namespace OrbitGame.Tests;

public class Vector3_Tests
{
    [Fact]
    public void Vector3_DotMethod()
    {
        SD_Vector3 argument1 = new SD_Vector3(0, 1, 2);
        SD_Vector3 argument2 = new SD_Vector3(3, -4, 5);
        
        Assert.Equal(6, SD_Vector3.Dot(argument1, argument2));
    }

    [Fact]
    public void Vector3_CrossMethod()
    {
        SD_Vector3 argument1 = new SD_Vector3(1, 2, 3);
        SD_Vector3 argument2 = new SD_Vector3(3, 4, 5);
        
        Assert.Equal(new SD_Vector3(-2, 4, -2), SD_Vector3.Cross(argument1, argument2));
    }

    [Fact]
    public void Vector3_MagnitudeMethod()
    {
        SD_Vector3 argument1 = new SD_Vector3(0, 0, -3);
        SD_Vector3 argument2 = new SD_Vector3(3, 4, 12);
        
        Assert.Equal(3, argument1.Magnitude());
        Assert.Equal(13, argument2.Magnitude());
    }

    [Fact]
    public void Vector3_NormalizeMethod()
    {
        SD_Vector3 argument1 = new SD_Vector3(4, 0, 4);
        SD_Vector3 zeroVector = SD_Vector3.Zero;
        
        Assert.Equal(new SD_Vector3(Math.Cos(Math.PI / 4), 0, Math.Sin(Math.PI / 4)), argument1.Normalize());
        Assert.Throws<ArithmeticException>(() => zeroVector.Normalize());
    }
    
    #region Operators

    [Fact]
    public void Vector3_NegativeOperator()
    {
        SD_Vector3 argument1 = new SD_Vector3(2, -2, 0);
        Assert.Equal(-new SD_Vector3(-2, 2, 0), argument1);
    }

    [Fact]
    public void Vector3_AdditionOperator()
    {
        SD_Vector3 argument1 = new SD_Vector3(1, 1, 1);
        SD_Vector3 argument2 = new SD_Vector3(-2, 2, -1);
        
        Assert.Equal(new SD_Vector3(-1, 3, 0), argument1 + argument2);
    }

    [Fact]
    public void Vector3_SubtractionOperator()
    {
        SD_Vector3 argument1 = new SD_Vector3(1, 1, 1);
        SD_Vector3 argument2 = new SD_Vector3(-2, 2, -1);
        
        Assert.Equal(new SD_Vector3(3, -1, 2), argument1 - argument2);
    }

    [Fact]
    public void Vector3_ScalarMultiplicationOperator()
    {
        SD_Vector3 vector = new SD_Vector3(1, 1, 1);
        ScientificDecimal scalar = -3;
        Assert.Equal(new SD_Vector3(-3, -3, -3), vector * scalar);
    }

    [Fact]
    public void Vector3_ScalarDivisionOperator()
    {
        SD_Vector3 vector = new SD_Vector3(-3, -3, -3);
        ScientificDecimal scalar = 3;
        Assert.Equal(new SD_Vector3(-1, -1, -1), vector / scalar);
    }

    [Fact]
    public void Vector3_DirectionVectorBetweenMethod()
    {
        SD_Vector3 argument1 = new SD_Vector3(0, 0, 1);
        SD_Vector3 argument2 = new SD_Vector3(1, 0, 0);
        
        Assert.Equal(new SD_Vector3(Math.Cos(Math.PI / 4), 0, -Math.Sin(Math.PI / 4)),
            SD_Vector3.DirectionVectorBetween(argument1, argument2));
    }
    
    #endregion
}