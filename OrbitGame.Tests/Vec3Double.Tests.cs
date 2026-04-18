using System;
using Xunit;

namespace OrbitGame.Tests;

public class Vec3Double_Tests
{
    private readonly Vec3Double _testXVector = new(5, 0, 0);
    private readonly Vec3Double _testYVector = new(0, 5, 0);
    private readonly Vec3Double _testZVector = new(0, 0, 5);
    private readonly Vec3Double _testZeroVector = new(0, 0, 0);
    private readonly double _testScalar = 5;
    
    [Fact]
    public void Vec3_DotMethod()
    {
        Assert.Equal(0, Vec3Double.Dot(_testXVector, _testZVector));
        Assert.Equal(0, Vec3Double.Dot(_testXVector, _testYVector));
        Assert.Equal(0, Vec3Double.Dot(_testYVector, _testZVector));
        Assert.Equal(25, Vec3Double.Dot(_testXVector, _testXVector));
    }

    [Fact]
    public void Vec3_CrossMethod()
    {
        Assert.Equal(new Vec3Double(0, 0, 25), Vec3Double.Cross(_testXVector, _testYVector));
        Assert.Equal(new Vec3Double(0, -25, 0), Vec3Double.Cross(_testXVector, _testZVector));
        Assert.Equal(new Vec3Double(0, 25, 0), Vec3Double.Cross(_testZVector, _testXVector));
    }

    [Fact]
    public void Vec3_MagnitudeMethod()
    {
        Assert.Equal(5, _testXVector.Magnitude());
        Assert.Equal(5, _testYVector.Magnitude());
        Assert.Equal(5, _testZVector.Magnitude());
    }
    
    [Fact]
    public void Vec3_DirectionMethod()
    {
        Assert.Equal(new Vec3Double(Math.Cos(Math.PI / 4), 0, -Math.Sin(Math.PI / 4)),
            Vec3Double.Direction(_testZVector, _testXVector));
    }

    [Fact]
    public void Vec3_NormalizeMethod()
    {
        Assert.Equal(new Vec3Double(1, 0, 0), _testXVector.Normalize());
        Assert.Equal(new Vec3Double(double.NaN, double.NaN, double.NaN), _testZeroVector.Normalize());
    }

    [Fact]
    public void Vec3_UnaryOperators()
    {
        Assert.Equal(new Vec3Double(5, 0, 0), +_testXVector);
        Assert.Equal(new Vec3Double(-5, 0, 0), -_testXVector);
    }

    [Fact]
    public void Vec3_AdditionOperator()
    {
        Assert.Equal(new Vec3Double(5, 0, 5), _testXVector + _testZVector);
    }

    [Fact]
    public void Vec3_SubtractionOperator()
    {
        Assert.Equal(new Vec3Double(5, 0, -5), _testXVector - _testZVector);
    }

    [Fact]
    public void Vec3_ScalarMultiplicationOperator()
    {
        Assert.Equal(new Vec3Double(25, 0, 0), _testXVector * _testScalar);
    }

    [Fact]
    public void Vec3_ScalarDivisionOperator()
    {
        Assert.Equal(new Vec3Double(1, 0, 0), _testXVector / _testScalar);
    }
    
    [Fact]
    public void Vec3_EqualityOperator()
    {
        Assert.True(_testXVector == new Vec3Double(5, 0, 0));
        Assert.False(_testXVector == _testYVector);
    }
    
    [Fact]
    public void Vec3_InequalityOperator()
    {
        Assert.False(_testXVector != new Vec3Double(5, 0, 0));
        Assert.True(_testXVector != _testYVector);
    }

    [Fact]
    public void Vec3_ToStringMethod()
    {
        Assert.Equal("<5, 0, 0>", _testXVector.ToString());
        Assert.Equal("<5.0000, 0.0000, 0.0000>", _testXVector.ToString("N4"));
    }
}