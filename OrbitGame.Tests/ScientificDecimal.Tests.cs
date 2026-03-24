using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace OrbitGame.Tests;

public class ScientificDecimal_Tests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;
    
    private readonly PDecimal _testPosPDecimal = new(4, 0);
    private readonly PDecimal _testNegPDecimal = new(-4, 0);

    [Fact]
    public void ScientificDecimal_SqrtMethod()
    {
        Assert.Equal(2, PDecimal.Sqrt(_testPosPDecimal));
        Assert.Equal(PDecimal.PosInfinity, PDecimal.Sqrt(PDecimal.PosInfinity));
        Assert.Throws<ArithmeticException>(() => PDecimal.Sqrt(_testNegPDecimal));
        Assert.Throws<ArithmeticException>(() => PDecimal.Sqrt(PDecimal.NegInfinity));
    }

    [Fact]
    public void ScientificDecimal_AbsMethod()
    {
        Assert.Equal(4, PDecimal.Abs(_testPosPDecimal));
        Assert.Equal(4, PDecimal.Abs(_testNegPDecimal));
        Assert.Equal(PDecimal.PosInfinity, PDecimal.Abs(PDecimal.PosInfinity));
        Assert.Equal(PDecimal.PosInfinity, PDecimal.Abs(PDecimal.NegInfinity));
    }

    [Fact]
    public void ScientificDecimal_MinMethod()
    {
        Assert.Equal(-4, PDecimal.Min(_testNegPDecimal, _testPosPDecimal));
        Assert.Equal(PDecimal.NegInfinity, PDecimal.Min(PDecimal.PosInfinity, PDecimal.NegInfinity));
        Assert.Equal(PDecimal.NegInfinity, PDecimal.Min(PDecimal.NegInfinity, _testNegPDecimal));
    }
    
    [Fact]
    public void ScientificDecimal_MaxMethod()
    {
        Assert.Equal(-4, PDecimal.Max(PDecimal.NegInfinity, _testNegPDecimal));
        Assert.Equal(4, PDecimal.Max(_testNegPDecimal, _testPosPDecimal));
        Assert.Equal(PDecimal.PosInfinity, PDecimal.Max(PDecimal.PosInfinity, PDecimal.NegInfinity));
    }

    [Fact]
    public void ScientificDecimal_ClampMethod()
    {
        PDecimal argument = new PDecimal(-8, 0);

        Assert.Equal(-4, PDecimal.Clamp(argument, _testNegPDecimal, _testPosPDecimal));
        Assert.Equal(-4,
            PDecimal.Clamp(PDecimal.NegInfinity, _testNegPDecimal, _testPosPDecimal));
        Assert.Equal(4,
            PDecimal.Clamp(PDecimal.PosInfinity, _testNegPDecimal, _testPosPDecimal));
        Assert.Equal(PDecimal.PosInfinity,
            PDecimal.Clamp(PDecimal.PosInfinity, _testNegPDecimal, PDecimal.PosInfinity));
        Assert.Throws<ArithmeticException>(() =>
            PDecimal.Clamp(argument, _testPosPDecimal, _testNegPDecimal));
    }

    [Fact]
    public void ScientificDecimal_RoundMethod()
    {
        SDecimal argument = new(0.00001, 0);
        Assert.Equal(0, argument.Round());
        argument = new(0.9, 0);
        Assert.Equal(1, argument.Round());
        argument = new(1.1, 0);
        Assert.Equal(1, argument.Round());
        argument = new(10000.9, 0);
        Assert.Equal(10001, argument.Round(), Assert.Epsilon);
    }
    
    [Fact]
    public void ScientificDecimal_FloorMethod()
    {
        Assert.Equal(0, new SDecimal(0.00001, 0).Floor());
        Assert.Equal(-1, new SDecimal(-0.00001, 0).Floor());
        Assert.Equal(10000, new SDecimal(10000.9, 0).Floor());
        Assert.Equal(10001, new SDecimal(10001, 0).Floor());
        Assert.Equal(new SDecimal(100), new SDecimal(100).Floor());
    }
    
    #region Operators
    
    [Fact]
    public void ScientificDecimal_NegativeOperator()
    {
        Assert.Equal(-4, -_testPosPDecimal);
        Assert.Equal(PDecimal.NegInfinity, -PDecimal.PosInfinity);
    }
    
    [Fact]
    public void ScientificDecimal_AdditionOperator()
    {
        Assert.Equal(0, _testPosPDecimal + _testNegPDecimal);
        Assert.Equal(PDecimal.PosInfinity, _testPosPDecimal + PDecimal.PosInfinity);
        Assert.Equal(PDecimal.PosInfinity, PDecimal.PosInfinity + PDecimal.PosInfinity);
        Assert.Throws<ArithmeticException>(() => PDecimal.PosInfinity + PDecimal.NegInfinity);
    }

    [Fact]
    public void ScientificDecimal_SubtractionOperator()
    {
        Assert.Equal(8, _testPosPDecimal - _testNegPDecimal);
        Assert.Equal(PDecimal.PosInfinity, _testPosPDecimal - PDecimal.NegInfinity);
        Assert.Throws<ArithmeticException>(() => PDecimal.PosInfinity - PDecimal.PosInfinity);
    }

    [Fact]
    public void ScientificDecimal_MultiplicationOperator()
    {
        Assert.Equal(-16, _testPosPDecimal * _testNegPDecimal);
        Assert.Equal(0, PDecimal.Zero * PDecimal.PosInfinity);
        Assert.Equal(PDecimal.NegInfinity, _testNegPDecimal * PDecimal.PosInfinity);
        Assert.Equal(PDecimal.NegInfinity, PDecimal.PosInfinity * PDecimal.NegInfinity);
        Assert.Equal(PDecimal.PosInfinity, PDecimal.NegInfinity * PDecimal.NegInfinity);
    }

    [Fact]
    public void ScientificDecimal_DivisionOperator()
    {
        Assert.Equal(-1, _testPosPDecimal / _testNegPDecimal);
        Assert.Equal(0, _testPosPDecimal / PDecimal.PosInfinity);
        Assert.Equal(PDecimal.NegInfinity, PDecimal.PosInfinity / _testNegPDecimal);
        Assert.Equal(PDecimal.NegInfinity, _testNegPDecimal / PDecimal.Zero);
        Assert.Throws<ArithmeticException>(() => PDecimal.Zero / PDecimal.Zero);
        Assert.Throws<ArithmeticException>(() => PDecimal.PosInfinity / PDecimal.NegInfinity);
    }

    [Fact]
    public void ScientificDecimal_ModuloOperator()
    {
        Assert.Equal(2, new PDecimal(2, 0) % new PDecimal(4, 0));
        Assert.Equal(0, new PDecimal(4, 0) % new PDecimal(2, 0));
        Assert.Throws<ArithmeticException>(() => PDecimal.PosInfinity % _testPosPDecimal);
        Assert.Equal(4, _testPosPDecimal % PDecimal.PosInfinity);
        Assert.Throws<ArithmeticException>(() => _testPosPDecimal % PDecimal.Zero);
        Assert.Equal(1, new PDecimal(6) % 3, 0.0001);
    }
    
    [Fact]
    public void ScientificDecimal_EqualsOperator()
    {
        Assert.False(_testPosPDecimal == _testNegPDecimal);
        Assert.False(_testPosPDecimal == PDecimal.PosInfinity);
    }

    [Fact]
    public void ScientificDecimal_GreaterThanOperator()
    {
        Assert.True(PDecimal.PosInfinity > PDecimal.NegInfinity);
        Assert.True(_testPosPDecimal > _testNegPDecimal);
    }

    [Fact]
    public void ScientificDecimal_LessThanOperator()
    {
        Assert.True(_testNegPDecimal < _testPosPDecimal);
        Assert.False(PDecimal.PosInfinity < _testPosPDecimal);
    }
    
    #endregion Operators
    
    #region Casts

    [Fact]
    public void ScientificDecimal_DoubleCast()
    {
        PDecimal argument = new PDecimal(-1.59, 1);
        
        Assert.Equal(-15.9d, (double)argument);
    }
    
    [Fact]
    public void ScientificDecimal_IntCast()
    {
        PDecimal posArgument = new PDecimal(1.59, 1);
        PDecimal negArgument = new PDecimal(-1.59, 1);
        
        Assert.Equal(15, (int)posArgument);
        Assert.Equal(-15, (int)negArgument);
    }
    
    #endregion Casts

    [Fact]
    public void ScientificDecimal_IsZeroMethod()
    {
        Assert.False(PDecimal.IsZero(PDecimal.PosInfinity));
        Assert.True(PDecimal.IsZero(PDecimal.Zero));
    }

    [Fact]
    public void ScientificDecimal_IsFiniteMethod()
    {
        Assert.True(PDecimal.IsFinite(PDecimal.Zero));
        Assert.True(PDecimal.IsFinite(_testPosPDecimal));
        Assert.False(PDecimal.IsFinite(PDecimal.PosInfinity));
        Assert.False(PDecimal.IsFinite(PDecimal.NegInfinity));
    }

    [Fact]
    public void ScientificDecimal_IsRealNumberMethod()
    {
        Assert.True(PDecimal.IsRealNumber(PDecimal.Zero));
        Assert.True(PDecimal.IsRealNumber(PDecimal.PosInfinity));
    }
    
    [Fact]
    public void ScientificDecimal_IsImaginaryNumberMethod()
    {
        Assert.False(PDecimal.IsImaginaryNumber(PDecimal.Zero));
        Assert.False(PDecimal.IsImaginaryNumber(PDecimal.PosInfinity));
    }
    
    [Fact]
    public void ScientificDecimal_IsComplexNumberMethod()
    {
        Assert.False(PDecimal.IsComplexNumber(PDecimal.Zero));
        Assert.False(PDecimal.IsComplexNumber(PDecimal.PosInfinity));
    }

    [Fact]
    public void ScientificDecimal_IsIntegerMethod()
    {
        Assert.True(PDecimal.IsInteger(PDecimal.Zero));
        Assert.True(PDecimal.IsInteger(new PDecimal(10)));
        Assert.True(PDecimal.IsInteger(new PDecimal(0.5, 1)));
        Assert.False(PDecimal.IsInteger(new PDecimal(0.5, 0)));
        Assert.False(PDecimal.IsInteger(PDecimal.PosInfinity));
    }
    
    [Fact]
    public void ScientificDecimal_IsEvenIntegerMethod()
    {
        Assert.True(PDecimal.IsEvenInteger(PDecimal.Zero));
        Assert.False(PDecimal.IsEvenInteger(PDecimal.One));
        Assert.False(PDecimal.IsEvenInteger(new PDecimal(0.5, 0)));
        Assert.False(PDecimal.IsEvenInteger(PDecimal.PosInfinity));
    }
    
    [Fact]
    public void ScientificDecimal_IsOddIntegerMethod()
    {
        Assert.False(PDecimal.IsOddInteger(PDecimal.Zero));
        Assert.True(PDecimal.IsOddInteger(PDecimal.One));
        Assert.False(PDecimal.IsOddInteger(new PDecimal(0.5, 0)));
        Assert.False(PDecimal.IsOddInteger(PDecimal.PosInfinity));
    }

    [Fact]
    public void ScientificDecimal_IsNaN()
    {
        Assert.False(PDecimal.IsNaN(PDecimal.Zero));
        Assert.True(PDecimal.IsNaN(PDecimal.PosInfinity));
    }
    
    [Fact]
    public void ScientificDecimal_IsInfinityMethod()
    {
        Assert.False(PDecimal.IsInfinity(PDecimal.Zero));
        Assert.False(PDecimal.IsInfinity(_testPosPDecimal));
        Assert.True(PDecimal.IsInfinity(PDecimal.PosInfinity));
        Assert.True(PDecimal.IsInfinity(PDecimal.NegInfinity));
    }

    [Fact]
    public void ScientificDecimal_IsNegativeInfinityMethod()
    {
        Assert.False(PDecimal.IsNegativeInfinity(PDecimal.Zero));
        Assert.False(PDecimal.IsNegativeInfinity(PDecimal.PosInfinity));
        Assert.True(PDecimal.IsNegativeInfinity(PDecimal.NegInfinity));
    }
    
    [Fact]
    public void ScientificDecimal_IsPositiveInfinityMethod()
    {
        Assert.False(PDecimal.IsPositiveInfinity(PDecimal.Zero));
        Assert.True(PDecimal.IsPositiveInfinity(PDecimal.PosInfinity));
        Assert.False(PDecimal.IsPositiveInfinity(PDecimal.NegInfinity));
    }
    
    /*[Fact]
    public void ScientificDecimal_ToStringMethod()
    {
        ScientificDecimal testArgument1 = -123.456789;
        ScientificDecimal testArgument2 = new(-1.234567, -8);
        ScientificDecimal testArgument3 = new(-1.2345, 10);
        
        Assert.Equal("0.0000e+0", ScientificDecimal.Zero.ToString("G", CultureInfo.InvariantCulture));
        Assert.Equal("0.0000e+0", ScientificDecimal.Zero.ToString("", CultureInfo.InvariantCulture));
        Assert.Equal("-1.2346e+2", testArgument1.ToString("G", CultureInfo.InvariantCulture));
        Assert.Equal("PositiveInfinity", ScientificDecimal.PosInfinity.ToString("G", CultureInfo.InvariantCulture));
        Assert.Equal("NegativeInfinity", ScientificDecimal.NegInfinity.ToString("G", CultureInfo.InvariantCulture));

        Assert.Throws<FormatException>(() => ScientificDecimal.Zero.ToString("P", CultureInfo.InvariantCulture));
        Assert.Equal("0e+0", ScientificDecimal.Zero.ToString("P1", CultureInfo.InvariantCulture));
        Assert.Equal("-1e+2", testArgument1.ToString("P1", CultureInfo.InvariantCulture));
        Assert.Equal("PositiveInfinity", ScientificDecimal.PosInfinity.ToString("P1", CultureInfo.InvariantCulture));
        Assert.Equal("NegativeInfinity", ScientificDecimal.NegInfinity.ToString("P2", CultureInfo.InvariantCulture));
        
        Assert.Equal("-123.46", testArgument1.ToString("S", CultureInfo.InvariantCulture));
        Assert.Equal("-0.00000001234567000", testArgument2.ToString("S10", CultureInfo.InvariantCulture));
        Assert.Equal("-12345000000", testArgument3.ToString("S10", CultureInfo.InvariantCulture));
        Assert.Equal("-12340000000", testArgument3.ToString("S4", CultureInfo.InvariantCulture));
        
        Assert.Throws<FormatException>(() => ScientificDecimal.Zero.ToString("1", CultureInfo.InvariantCulture));
    }*/
}