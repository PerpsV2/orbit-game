using System;
using Xunit;

namespace OrbitGame.Tests;

public class SD_Vector2_Tests
{
    private readonly double _testAngle = Math.PI;
    private readonly double _testMagnitude = 5;
    private readonly DVector2<SDecimal> _testHorizontalVector = new(5, 0);
    private readonly DVector2<SDecimal> _testVerticalVector = new(0, 5);
    private readonly DVector2<SDecimal> _testPythagoreanVector = new(3, 4);
    private readonly DVector2<SDecimal> _testZeroVector = new(0, 0);
    private readonly SDecimal _testScalar = 5;
    private readonly double _testReferenceAngle = Math.PI / 2;
    
    [Fact]
    public void Vector2_FromPolarMethod()
    {
        Assert.Equal(new DVector2<SDecimal>(-5, 0), DVector2<SDecimal>.FromPolar(_testAngle, _testMagnitude));
        Assert.Equal(new DVector2<SDecimal>(5, 0), DVector2<SDecimal>.FromPolar(_testAngle, -_testMagnitude));
    }
    
    [Fact]
    public void Vector2_DotMethod()
    {
        Assert.Equal(0, DVector2<SDecimal>.Dot(_testHorizontalVector, _testVerticalVector));
        Assert.Equal(25, DVector2<SDecimal>.Dot(_testHorizontalVector, _testHorizontalVector));
        Assert.Equal(25, DVector2<SDecimal>.Dot(_testVerticalVector, _testVerticalVector));
    }

    [Fact]
    public void Vector2_CrossMethod()
    {
        Assert.Equal(new DVector3<SDecimal>(0, 0, -25), DVector2<SDecimal>.Cross(_testVerticalVector, _testHorizontalVector));
        Assert.Equal(new DVector3<SDecimal>(0, 0, 25), DVector2<SDecimal>.Cross(_testHorizontalVector, _testVerticalVector));
        Assert.Equal(new DVector3<SDecimal>(0, 0, 0), DVector2<SDecimal>.Cross(_testHorizontalVector, _testHorizontalVector));
    }

    [Fact]
    public void Vector2_MagnitudeMethod()
    {
        Assert.Equal(5, _testPythagoreanVector.Magnitude());
        Assert.Equal(5, _testHorizontalVector.Magnitude());
    }
    
    [Fact]
    public void Vector2_MagnitudeSquaredMethod()
    {
        Assert.Equal(25, _testPythagoreanVector.MagnitudeSquared());
        Assert.Equal(25, _testHorizontalVector.MagnitudeSquared());
    }

    [Fact]
    public void Vector2_NormalizeMethod()
    {
        Assert.Equal(new DVector2<SDecimal>(1, 0), _testHorizontalVector.Normalize());
        Assert.Equal(new DVector2<SDecimal>(0, 1), _testVerticalVector.Normalize());
        Assert.Throws<ArithmeticException>(() => _testZeroVector.Normalize());
    }
    
    [Fact]
    public void Vector2_DirectionMethod()
    {
        Assert.Throws<DivideByZeroException>(() => DVector2<SDecimal>.Zero.Direction());
        Assert.Equal(_testReferenceAngle,
            new DVector2<SDecimal>(Math.Cos(_testReferenceAngle), Math.Sin(_testReferenceAngle)).Direction());
        Assert.Equal(Math.PI - _testReferenceAngle, 
            new DVector2<SDecimal>(Math.Cos(-_testReferenceAngle), Math.Sin(_testReferenceAngle)).Direction());
        Assert.Equal(Math.PI + _testReferenceAngle, 
            new DVector2<SDecimal>(Math.Cos(-_testReferenceAngle), Math.Sin(-_testReferenceAngle)).Direction());
        Assert.Equal(Math.Tau - _testReferenceAngle, 
            new DVector2<SDecimal>(Math.Cos(_testReferenceAngle), Math.Sin(-_testReferenceAngle)).Direction());
    }

    [Fact]
    public void Vector2_NegativeOperator()
    {
        Assert.Equal(new DVector2<SDecimal>(-5, 0), -_testHorizontalVector);
        Assert.Equal(new DVector2<SDecimal>(5, 0), - -_testHorizontalVector);
    }

    [Fact]
    public void Vector2_AdditionOperator()
    {
        Assert.Equal(new DVector2<SDecimal>(5, 5), _testHorizontalVector + _testVerticalVector);
    }

    [Fact]
    public void Vector2_SubtractionOperator()
    {
        Assert.Equal(new DVector2<SDecimal>(5, -5), _testHorizontalVector - _testVerticalVector);
    }

    [Fact]
    public void Vector2_ScalarMultiplicationOperator()
    {
        Assert.Equal(new DVector2<SDecimal>(25, 0), _testHorizontalVector * _testScalar);
    }

    [Fact]
    public void Vector2_ScalarDivisionOperator()
    {
        Assert.Equal(new DVector2<SDecimal>(1, 0), _testHorizontalVector / _testScalar);
        Assert.Throws<ArithmeticException>(() => _testHorizontalVector / 0);
    }

    [Fact]
    public void Vector2_ToString()
    {
        Assert.Equal("<5.0000e+0, 0.0000e+0>", _testHorizontalVector.ToString());
    }
}