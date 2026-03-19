using System;
using Xunit;

namespace OrbitGame.Tests;

public class SD_Vector3_Tests
{
    private readonly DVector3<> _testXVector = new(5, 0, 0);
    private readonly DVector3<> _testYVector = new(0, 5, 0);
    private readonly DVector3<> _testZVector = new(0, 0, 5);
    private readonly DVector3<> _testZeroVector = new(0, 0, 0);
    private readonly PDecimal _testScalar = 5;
    
    [Fact]
    public void Vector3_DotMethod()
    {
        Assert.Equal(0, DVector3<>.Dot(_testXVector, _testZVector));
        Assert.Equal(0, DVector3<>.Dot(_testXVector, _testYVector));
        Assert.Equal(0, DVector3<>.Dot(_testYVector, _testZVector));
        Assert.Equal(25, DVector3<>.Dot(_testXVector, _testXVector));
    }

    [Fact]
    public void Vector3_CrossMethod()
    {
        Assert.Equal(new DVector3<>(0, 0, 25), DVector3<>.Cross(_testXVector, _testYVector));
        Assert.Equal(new DVector3<>(0, -25, 0), DVector3<>.Cross(_testXVector, _testZVector));
        Assert.Equal(new DVector3<>(0, 25, 0), DVector3<>.Cross(_testZVector, _testXVector));
    }

    [Fact]
    public void Vector3_MagnitudeMethod()
    {
        Assert.Equal(5, _testXVector.Magnitude());
        Assert.Equal(5, _testYVector.Magnitude());
        Assert.Equal(5, _testZVector.Magnitude());
    }
    
    [Fact]
    public void Vector3_DirectionVectorMethod()
    {
        Assert.Equal(new DVector3<>(Math.Cos(Math.PI / 4), 0, -Math.Sin(Math.PI / 4)),
            DVector3<>.DirectionVector(_testZVector, _testXVector));
    }

    [Fact]
    public void Vector3_NormalizeMethod()
    {
        Assert.Equal(new DVector3<>(1, 0, 0), _testXVector.Normalize());
        Assert.Throws<ArithmeticException>(() => _testZeroVector.Normalize());
    }

    [Fact]
    public void Vector3_NegativeOperator()
    {
        Assert.Equal(new DVector3<>(-5, 0, 0), -_testXVector);
    }

    [Fact]
    public void Vector3_AdditionOperator()
    {
        Assert.Equal(new DVector3<>(5, 0, 5), _testXVector + _testZVector);
    }

    [Fact]
    public void Vector3_SubtractionOperator()
    {
        Assert.Equal(new DVector3<>(5, 0, -5), _testXVector - _testZVector);
    }

    [Fact]
    public void Vector3_ScalarMultiplicationOperator()
    {
        Assert.Equal(new DVector3<>(25, 0, 0), _testXVector * _testScalar);
    }

    [Fact]
    public void Vector3_ScalarDivisionOperator()
    {
        Assert.Equal(new DVector3<>(1, 0, 0), _testXVector / _testScalar);
    }
    
    [Fact]
    public void Vector3_EqualsOperator()
    {
        Assert.True(_testXVector == new DVector3<>(5, 0, 0));
        Assert.False(_testXVector == _testYVector);
    }
    
    [Fact]
    public void Vector3_UnequalsOperator()
    {
        Assert.False(_testXVector != new DVector3<>(5, 0, 0));
        Assert.True(_testXVector != _testYVector);
    }

    [Fact]
    public void Vector3_ToStringMethod()
    {
        Assert.Equal("<5.0000e+0, 0.0000e+0, 0.0000e+0>", _testXVector.ToString());
    }
}