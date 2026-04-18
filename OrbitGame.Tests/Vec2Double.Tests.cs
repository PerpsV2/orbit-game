using System;
using Xunit;

namespace OrbitGame.Tests;

public class Vec2Double_Tests
{
    private readonly double _testAngle = Math.PI;
    private readonly double _testMagnitude = 5;
    private readonly Vec2Double _testHorizontalVector = new(5, 0);
    private readonly Vec2Double _testVerticalVector = new(0, 5);
    private readonly Vec2Double _testPythagoreanVector = new(3, 4);
    private readonly Vec2Double _testZeroVector = new(0, 0);
    private readonly double _testScalar = 5;
    private readonly SDecimal _testSDecimal = 5;
    private readonly double _testReferenceAngle = Math.PI / 2;
    
    [Fact]
    public void Vec2_FromPolarMethod()
    {
        Assert.Equal(new Vec2Double(-1, 0), Vec2Double.FromPolar(_testAngle));
        Assert.Equal(new Vec2Double(-5, 0), Vec2Double.FromPolar(_testAngle, _testMagnitude));
        Assert.Equal(new Vec2Double(5, 0), Vec2Double.FromPolar(_testAngle, -_testMagnitude));
    }
    
    [Fact]
    public void Vec2_DotMethod()
    {
        Assert.Equal(0, Vec2Double.Dot(_testHorizontalVector, _testVerticalVector));
        Assert.Equal(25, Vec2Double.Dot(_testHorizontalVector, _testHorizontalVector));
        Assert.Equal(25, Vec2Double.Dot(_testVerticalVector, _testVerticalVector));
    }

    [Fact]
    public void Vec2_CrossMethod()
    {
        Assert.Equal(new Vec3Double(0, 0, -25), Vec2Double.Cross(_testVerticalVector, _testHorizontalVector));
        Assert.Equal(new Vec3Double(0, 0, 25), Vec2Double.Cross(_testHorizontalVector, _testVerticalVector));
        Assert.Equal(new Vec3Double(0, 0, 0), Vec2Double.Cross(_testHorizontalVector, _testHorizontalVector));
    }
    
    [Fact]
    public void Vec2_MagnitudeSquaredMethod()
    {
        Assert.Equal(25, _testPythagoreanVector.MagnitudeSquared());
        Assert.Equal(25, _testHorizontalVector.MagnitudeSquared());
    }
    
    [Fact]
    public void Vec2_MagnitudeMethod()
    {
        Assert.Equal(5, _testPythagoreanVector.Magnitude());
        Assert.Equal(5, _testHorizontalVector.Magnitude());
    }
    
    [Fact]
    public void Vec2_DirectionMethod()
    {
        Assert.Throws<DivideByZeroException>(() => Vec2Double.Zero.Direction());
        Assert.Equal(_testReferenceAngle,
            new Vec2Double(Math.Cos(_testReferenceAngle), Math.Sin(_testReferenceAngle)).Direction());
        Assert.Equal(Math.PI - _testReferenceAngle, 
            new Vec2Double(Math.Cos(-_testReferenceAngle), Math.Sin(_testReferenceAngle)).Direction());
        Assert.Equal(Math.PI + _testReferenceAngle, 
            new Vec2Double(Math.Cos(-_testReferenceAngle), Math.Sin(-_testReferenceAngle)).Direction());
        Assert.Equal(Math.Tau - _testReferenceAngle, 
            new Vec2Double(Math.Cos(_testReferenceAngle), Math.Sin(-_testReferenceAngle)).Direction());
        
        Assert.Equal(3 * Math.PI / 4, Vec2Double.Direction(_testHorizontalVector, _testVerticalVector));
        Assert.Equal(7 * Math.PI / 4, Vec2Double.Direction(_testVerticalVector, _testHorizontalVector));
    }
    
    [Fact]
    public void Vec2_NormalizeMethod()
    {
        Assert.Equal(new Vec2Double(1, 0), _testHorizontalVector.Normalize());
        Assert.Equal(new Vec2Double(0, 1), _testVerticalVector.Normalize());
        Assert.Throws<DivideByZeroException>(() => _testZeroVector.Normalize());
    }

    [Fact]
    public void Vec2_UnaryOperators()
    {
        Assert.Equal(new Vec2Double(5, 0), +_testHorizontalVector);
        Assert.Equal(new Vec2Double(-5, 0), -_testHorizontalVector);
        Assert.Equal(new Vec2Double(5, 0), - -_testHorizontalVector);
    }

    [Fact]
    public void Vec2_AdditionOperator()
    {
        Assert.Equal(new Vec2Double(5, 5), _testHorizontalVector + _testVerticalVector);
    }

    [Fact]
    public void Vec2_SubtractionOperator()
    {
        Assert.Equal(new Vec2Double(5, -5), _testHorizontalVector - _testVerticalVector);
    }

    [Fact]
    public void Vec2_ScalarMultiplicationOperator()
    {
        Assert.Equal(new Vec2Double(25, 0), _testHorizontalVector * _testSDecimal);
        Assert.Equal(new Vec2Double(25, 0), _testHorizontalVector * _testScalar);
    }

    [Fact]
    public void Vec2_ScalarDivisionOperator()
    {
        Assert.Equal(new Vec2Double(1, 0), _testHorizontalVector / _testScalar);
        Assert.Equal(new Vec2Double(double.PositiveInfinity, double.NaN), _testHorizontalVector / 0);
    }
    
    [Fact]
    public void Vec2_EqualityOperator()
    {
        Assert.True(_testHorizontalVector == new Vec2Double(5, 0));
        Assert.False(_testHorizontalVector == _testVerticalVector);
    }
    
    [Fact]
    public void Vec2_InequalityOperator()
    {
        Assert.False(_testHorizontalVector != new Vec2Double(5, 0));
        Assert.True(_testHorizontalVector != _testVerticalVector);
    }

    [Fact]
    public void Vec2_ToString()
    {
        Assert.Equal("<5, 0>", _testHorizontalVector.ToString());
        Assert.Equal("<5.0000, 0.0000>", _testHorizontalVector.ToString("N4"));
    }
}