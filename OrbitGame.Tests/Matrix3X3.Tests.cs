using System;
using Xunit;

namespace OrbitGame.Tests;

public class Matrix3X3_Tests
{
    private readonly Matrix3X3<SDecimal> _testNullMatrix = new([0, 0, 0, 0, 0, 0, 0, 0, 0]);
    private readonly Matrix3X3<SDecimal> _testIdentityMatrix = new([1, 0, 0, 0, 1, 0, 0, 0, 1]);
    private readonly Matrix3X3<SDecimal> _testMatrix = new([1, 2, 3, 4, 5, 6, 7, 8, 9]);
    // Rotation by 90 degrees followed by a translation of (1, 5)
    private readonly Matrix3X3<SDecimal> _testTransformationMatrix = new([0, -1, 1, 1, 0, 5, 0, 0, 1]);
    
    private readonly Vec2<SDecimal> _testSDDVector2 = new(1, -1);
    private readonly Vec3<SDecimal> _testSDDVector3 = new(1, -1, 1);
    private readonly SDecimal _testScalar = 5;
    
    #region Operators
    
    [Fact]
    public void Matrix3X3_NegativeOperator()
    {
        Assert.Equal(new Matrix3X3<SDecimal>([-1, 0, 0, 0, -1, 0, 0, 0, -1]), -_testIdentityMatrix);
    }
    
    [Fact]
    public void Matrix3X3_AdditionOperator()
    {
        Assert.Equal(new Matrix3X3<SDecimal>([2, 2, 3, 4, 6, 6, 7, 8, 10]), _testIdentityMatrix + _testMatrix);
    }

    [Fact]
    public void Matrix3X3_SubtractionOperator()
    {
        Assert.Equal(new Matrix3X3<SDecimal>([0, -2, -3, -4, -4, -6, -7, -8, -8]), _testIdentityMatrix - _testMatrix);
    }
    
    [Fact]
    public void Matrix3X3_MatrixMultiplicationOperator()
    {
        Assert.Equal(_testMatrix, _testIdentityMatrix * _testMatrix);
        Assert.Equal(new Matrix3X3<SDecimal>([30, 36, 42, 66, 81, 96, 102, 126, 150]), _testMatrix * _testMatrix);
    }

    [Fact]
    public void Matrix3X3_Vector2MultiplicationOperator()
    {
        Assert.Equal(_testSDDVector2, _testIdentityMatrix * _testSDDVector2);
        Assert.Equal(new Vec2<SDecimal>(2, 6), _testTransformationMatrix * _testSDDVector2);
        Assert.Throws<ArithmeticException>(() => _testNullMatrix * _testSDDVector2);
    }

    [Fact]
    public void Matrix3X3_Vector3MultiplicationOperator()
    {
        Assert.Equal(_testSDDVector3, _testIdentityMatrix * _testSDDVector3);
        Assert.Equal(new Vec3<SDecimal>(2, 6, 1), _testTransformationMatrix * _testSDDVector3);
    }

    [Fact]
    public void Matrix3X3_ScalarMultiplicationOperator()
    {
        Assert.Equal(new Matrix3X3<SDecimal>([5, 0, 0, 0, 5, 0, 0, 0, 5]), _testIdentityMatrix * _testScalar);
    }
    
    [Fact]
    public void Matrix3X3_ScalarDivisionOperator()
    {
        Assert.Equal(new Matrix3X3<SDecimal>([0.2, 0, 0, 0, 0.2, 0, 0, 0, 0.2]), _testIdentityMatrix / _testScalar);
    }
    
    #endregion
    
    #region Transformations

    [Fact]
    public void Matrix3X3_TranslationMethod()
    {
        Vec2<SDecimal> translation = new Vec2<SDecimal>(2, -2);
        Assert.Equal(new Matrix3X3<SDecimal>([1, 0, 2, 0, 1, -2, 0, 0, 1]), Matrix3X3<SDecimal>.Translation(translation));
    }

    [Fact]
    public void Matrix3X3_RotationMethod()
    {
        double angle = Math.PI / 2;
        Assert.Equal(new Matrix3X3<SDecimal>([0, -1, 0, 1, 0, 0, 0, 0, 1]), Matrix3X3<SDecimal>.Rotation(angle));
    }

    [Fact]
    public void Matrix3X3_ScaleMethod()
    {
        Vec2<SDecimal> scale = new Vec2<SDecimal>(2, -2);
        Assert.Equal(new Matrix3X3<SDecimal>([2, 0, 0, 0, -2, 0, 0, 0, 1]), Matrix3X3<SDecimal>.Scale(scale));
    }
    
    #endregion
}