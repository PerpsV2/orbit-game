using System;
using Xunit;

namespace OrbitGame.Tests;

public class Vec3_Tests
{
    private readonly Vec3<SDecimal> _testXVector = new(5, 0, 0);
    private readonly Vec3<SDecimal> _testYVector = new(0, 5, 0);
    private readonly Vec3<SDecimal> _testZVector = new(0, 0, 5);
    private readonly Vec3<SDecimal> _testZeroVector = new(0, 0, 0);
    private readonly SDecimal _testScalar = 5;
    
    [Fact]
    public void Vec3_DotMethod()
    {
        Assert.Equal(0, Vec3<SDecimal>.Dot(_testXVector, _testZVector));
        Assert.Equal(0, Vec3<SDecimal>.Dot(_testXVector, _testYVector));
        Assert.Equal(0, Vec3<SDecimal>.Dot(_testYVector, _testZVector));
        Assert.Equal(25, Vec3<SDecimal>.Dot(_testXVector, _testXVector));
    }

    [Fact]
    public void Vec3_CrossMethod()
    {
        Assert.Equal(new Vec3<SDecimal>(0, 0, 25), Vec3<SDecimal>.Cross(_testXVector, _testYVector));
        Assert.Equal(new Vec3<SDecimal>(0, -25, 0), Vec3<SDecimal>.Cross(_testXVector, _testZVector));
        Assert.Equal(new Vec3<SDecimal>(0, 25, 0), Vec3<SDecimal>.Cross(_testZVector, _testXVector));
    }

    [Fact]
    public void Vec3_MagnitudeMethod()
    {
        Assert.Equal(5, _testXVector.Magnitude());
        Assert.Equal(5, _testYVector.Magnitude());
        Assert.Equal(5, _testZVector.Magnitude());
    }
    
    [Fact]
    public void Vec3_DirectionVectorMethod()
    {
        Assert.Equal(new Vec3<SDecimal>(Math.Cos(Math.PI / 4), 0, -Math.Sin(Math.PI / 4)),
            Vec3<SDecimal>.DirectionVector(_testZVector, _testXVector));
    }

    [Fact]
    public void Vec3_NormalizeMethod()
    {
        Assert.Equal(new Vec3<SDecimal>(1, 0, 0), _testXVector.Normalize());
        Assert.Throws<DivideByZeroException>(() => _testZeroVector.Normalize());
    }

    [Fact]
    public void Vec3_NegativeOperator()
    {
        Assert.Equal(new Vec3<SDecimal>(-5, 0, 0), -_testXVector);
    }

    [Fact]
    public void Vec3_AdditionOperator()
    {
        Assert.Equal(new Vec3<SDecimal>(5, 0, 5), _testXVector + _testZVector);
    }

    [Fact]
    public void Vec3_SubtractionOperator()
    {
        Assert.Equal(new Vec3<SDecimal>(5, 0, -5), _testXVector - _testZVector);
    }

    [Fact]
    public void Vec3_ScalarMultiplicationOperator()
    {
        Assert.Equal(new Vec3<SDecimal>(25, 0, 0), _testXVector * _testScalar);
    }

    [Fact]
    public void Vec3_ScalarDivisionOperator()
    {
        Assert.Equal(new Vec3<SDecimal>(1, 0, 0), _testXVector / _testScalar);
    }
    
    [Fact]
    public void Vec3_EqualsOperator()
    {
        Assert.True(_testXVector == new Vec3<SDecimal>(5, 0, 0));
        Assert.False(_testXVector == _testYVector);
    }
    
    [Fact]
    public void Vec3_UnequalsOperator()
    {
        Assert.False(_testXVector != new Vec3<SDecimal>(5, 0, 0));
        Assert.True(_testXVector != _testYVector);
    }

    [Fact]
    public void Vec3_ToStringMethod()
    {
        Assert.Equal("<5.0000e+0, 0.0000e+0, 0.0000e+0>", _testXVector.ToString());
    }
}