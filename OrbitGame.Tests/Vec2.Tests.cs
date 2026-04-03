using System;
using Xunit;

namespace OrbitGame.Tests;

public class Vec2_Tests
{
    private readonly double _testAngle = Math.PI;
    private readonly double _testMagnitude = 5;
    private readonly Vec2<SDecimal> _testHorizontalVector = new(5, 0);
    private readonly Vec2<SDecimal> _testVerticalVector = new(0, 5);
    private readonly Vec2<SDecimal> _testPythagoreanVector = new(3, 4);
    private readonly Vec2<SDecimal> _testZeroVector = new(0, 0);
    private readonly SDecimal _testScalar = 5;
    private readonly double _testReferenceAngle = Math.PI / 2;
    
    [Fact]
    public void Vec2_FromPolarMethod()
    {
        Assert.Equal(new Vec2<SDecimal>(-5, 0), Vec2<SDecimal>.FromPolar(_testAngle, _testMagnitude));
        Assert.Equal(new Vec2<SDecimal>(5, 0), Vec2<SDecimal>.FromPolar(_testAngle, -_testMagnitude));
    }
    
    [Fact]
    public void Vec2_DotMethod()
    {
        Assert.Equal(0, Vec2<SDecimal>.Dot(_testHorizontalVector, _testVerticalVector));
        Assert.Equal(25, Vec2<SDecimal>.Dot(_testHorizontalVector, _testHorizontalVector));
        Assert.Equal(25, Vec2<SDecimal>.Dot(_testVerticalVector, _testVerticalVector));
    }

    [Fact]
    public void Vec2_CrossMethod()
    {
        Assert.Equal(new Vec3<SDecimal>(0, 0, -25), Vec2<SDecimal>.Cross(_testVerticalVector, _testHorizontalVector));
        Assert.Equal(new Vec3<SDecimal>(0, 0, 25), Vec2<SDecimal>.Cross(_testHorizontalVector, _testVerticalVector));
        Assert.Equal(new Vec3<SDecimal>(0, 0, 0), Vec2<SDecimal>.Cross(_testHorizontalVector, _testHorizontalVector));
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
        Assert.Throws<DivideByZeroException>(() => Vec2<SDecimal>.Zero.Direction());
        Assert.Equal(_testReferenceAngle,
            new Vec2<SDecimal>(Math.Cos(_testReferenceAngle), Math.Sin(_testReferenceAngle)).Direction());
        Assert.Equal(Math.PI - _testReferenceAngle, 
            new Vec2<SDecimal>(Math.Cos(-_testReferenceAngle), Math.Sin(_testReferenceAngle)).Direction());
        Assert.Equal(Math.PI + _testReferenceAngle, 
            new Vec2<SDecimal>(Math.Cos(-_testReferenceAngle), Math.Sin(-_testReferenceAngle)).Direction());
        Assert.Equal(Math.Tau - _testReferenceAngle, 
            new Vec2<SDecimal>(Math.Cos(_testReferenceAngle), Math.Sin(-_testReferenceAngle)).Direction());
        
        Assert.Equal(3 * Math.PI / 4, Vec2<SDecimal>.Direction(_testHorizontalVector, _testVerticalVector));
        Assert.Equal(7 * Math.PI / 4, Vec2<SDecimal>.Direction(_testVerticalVector, _testHorizontalVector));
    }
    
    [Fact]
    public void Vec2_NormalizeMethod()
    {
        Assert.Equal(new Vec2<SDecimal>(1, 0), _testHorizontalVector.Normalize());
        Assert.Equal(new Vec2<SDecimal>(0, 1), _testVerticalVector.Normalize());
        Assert.Throws<DivideByZeroException>(() => _testZeroVector.Normalize());
    }
    
    [Fact]
    public void Vec3_MapMethod()
    {
        Assert.Equal(new Vec2<PDecimal>(5, 0), _testHorizontalVector.Map<PDecimal>());
        Assert.Equal(new Vec2<PDecimal>(0, 5), _testVerticalVector.Map<PDecimal>());
        Assert.Equal(_testHorizontalVector, _testHorizontalVector.Map<SDecimal>());
    }

    [Fact]
    public void Vec2_NegativeOperator()
    {
        Assert.Equal(new Vec2<SDecimal>(-5, 0), -_testHorizontalVector);
        Assert.Equal(new Vec2<SDecimal>(5, 0), - -_testHorizontalVector);
    }

    [Fact]
    public void Vec2_AdditionOperator()
    {
        Assert.Equal(new Vec2<SDecimal>(5, 5), _testHorizontalVector + _testVerticalVector);
    }

    [Fact]
    public void Vec2_SubtractionOperator()
    {
        Assert.Equal(new Vec2<SDecimal>(5, -5), _testHorizontalVector - _testVerticalVector);
    }

    [Fact]
    public void Vec2_ScalarMultiplicationOperator()
    {
        Assert.Equal(new Vec2<SDecimal>(25, 0), _testHorizontalVector * _testScalar);
    }

    [Fact]
    public void Vec2_ScalarDivisionOperator()
    {
        Assert.Equal(new Vec2<SDecimal>(1, 0), _testHorizontalVector / _testScalar);
        Assert.Throws<DivideByZeroException>(() => _testHorizontalVector / 0);
    }
    
    [Fact]
    public void Vec2_EqualityOperator()
    {
        Assert.True(_testHorizontalVector == new Vec2<SDecimal>(5, 0));
        Assert.False(_testHorizontalVector == _testVerticalVector);
    }
    
    [Fact]
    public void Vec2_InequalityOperator()
    {
        Assert.False(_testHorizontalVector != new Vec2<SDecimal>(5, 0));
        Assert.True(_testHorizontalVector != _testVerticalVector);
    }

    [Fact]
    public void Vec2_ToString()
    {
        Assert.Equal("<5.0000e+0, 0.0000e+0>", _testHorizontalVector.ToString());
        Assert.Equal("<5e+0, 0e+0>", _testHorizontalVector.ToString("G1"));
        Assert.Equal("<5, 0>", _testHorizontalVector.ToString("N1"));
    }
}