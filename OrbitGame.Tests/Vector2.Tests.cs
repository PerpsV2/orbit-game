namespace OrbitGame.Tests;

public class Vector2_Tests
{
    private readonly double _testAngle = Math.PI;
    private readonly double _testMagnitude = 5;
    private readonly SD_Vector2 _testHorizontalVector = new(5, 0);
    private readonly SD_Vector2 _testVerticalVector = new(0, 5);
    private readonly SD_Vector2 _testPythagoreanVector = new(3, 4);
    private readonly SD_Vector2 _testZeroVector = new(0, 0);
    private readonly ScientificDecimal _testScalar = 5;
    private readonly double _testReferenceAngle = Math.PI / 2;
    
    [Fact]
    public void Vector2_FromPolarMethod()
    {
        Assert.Equal(new SD_Vector2(-5, 0), SD_Vector2.FromPolar(_testAngle, _testMagnitude));
        Assert.Equal(new SD_Vector2(5, 0), SD_Vector2.FromPolar(_testAngle, -_testMagnitude));
    }
    
    [Fact]
    public void Vector2_DotMethod()
    {
        Assert.Equal(0, SD_Vector2.Dot(_testHorizontalVector, _testVerticalVector));
        Assert.Equal(25, SD_Vector2.Dot(_testHorizontalVector, _testHorizontalVector));
        Assert.Equal(25, SD_Vector2.Dot(_testVerticalVector, _testVerticalVector));
    }

    [Fact]
    public void Vector2_CrossMethod()
    {
        Assert.Equal(new SD_Vector3(0, 0, -25), SD_Vector2.Cross(_testVerticalVector, _testHorizontalVector));
        Assert.Equal(new SD_Vector3(0, 0, 25), SD_Vector2.Cross(_testHorizontalVector, _testVerticalVector));
        Assert.Equal(new SD_Vector3(0, 0, 0), SD_Vector2.Cross(_testHorizontalVector, _testHorizontalVector));
    }

    [Fact]
    public void Vector2_Magnitude()
    {
        Assert.Equal(5, _testPythagoreanVector.Magnitude());
        Assert.Equal(5, _testHorizontalVector.Magnitude());
    }

    [Fact]
    public void Vector2_Normalize()
    {
        Assert.Equal(new SD_Vector2(1, 0), _testHorizontalVector.Normalize());
        Assert.Equal(new SD_Vector2(0, 1), _testVerticalVector.Normalize());
        Assert.Throws<ArithmeticException>(() => _testZeroVector.Normalize());
    }
    
    #region Operators

    [Fact]
    public void Vector2_NegativeOperator()
    {
        Assert.Equal(new SD_Vector2(-5, 0), -_testHorizontalVector);
        Assert.Equal(new SD_Vector2(5, 0), - -_testHorizontalVector);
    }

    [Fact]
    public void Vector2_AdditionOperator()
    {
        Assert.Equal(new SD_Vector2(5, 5), _testHorizontalVector + _testVerticalVector);
    }

    [Fact]
    public void Vector2_SubtractionOperator()
    {
        Assert.Equal(new SD_Vector2(5, -5), _testHorizontalVector - _testVerticalVector);
    }

    [Fact]
    public void Vector2_ScalarMultiplicationOperator()
    {
        Assert.Equal(new SD_Vector2(25, 0), _testHorizontalVector * _testScalar);
    }

    [Fact]
    public void Vector2_ScalarDivisionOperator()
    {
        Assert.Equal(new SD_Vector2(1, 0), _testHorizontalVector / _testScalar);
        Assert.Throws<ArithmeticException>(() => _testHorizontalVector / 0);
    }
    
    #endregion

    [Fact]
    public void Vector2_GetPrincipalAngleMethod()
    {
        Assert.Equal(_testReferenceAngle, new SD_Vector2(Math.Cos(_testReferenceAngle), Math.Sin(_testReferenceAngle)).Direction());
        Assert.Equal(Math.PI - _testReferenceAngle, new SD_Vector2(Math.Cos(-_testReferenceAngle), Math.Sin(_testReferenceAngle)).Direction());
        Assert.Equal(Math.PI + _testReferenceAngle, new SD_Vector2(Math.Cos(-_testReferenceAngle), Math.Sin(-_testReferenceAngle)).Direction());
        Assert.Equal(Math.Tau - _testReferenceAngle, new SD_Vector2(Math.Cos(_testReferenceAngle), Math.Sin(-_testReferenceAngle)).Direction());
    }
}