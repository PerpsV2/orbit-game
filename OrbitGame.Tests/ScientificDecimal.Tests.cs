using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace OrbitGame.Tests;

public class ScientificDecimal_Tests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;
    
    private readonly ScientificDecimal _testPosScientificDecimal = new(4, 0);
    private readonly ScientificDecimal _testNegScientificDecimal = new(-4, 0);

    [Fact]
    public void ScientificDecimal_SqrtMethod()
    {
        Assert.Equal(2, ScientificDecimal.Sqrt(_testPosScientificDecimal));
        Assert.Equal(ScientificDecimal.PosInfinity, ScientificDecimal.Sqrt(ScientificDecimal.PosInfinity));
        Assert.Throws<ArithmeticException>(() => ScientificDecimal.Sqrt(_testNegScientificDecimal));
        Assert.Throws<ArithmeticException>(() => ScientificDecimal.Sqrt(ScientificDecimal.NegInfinity));
    }

    [Fact]
    public void ScientificDecimal_AbsMethod()
    {
        Assert.Equal(4, ScientificDecimal.Abs(_testPosScientificDecimal));
        Assert.Equal(4, ScientificDecimal.Abs(_testNegScientificDecimal));
        Assert.Equal(ScientificDecimal.PosInfinity, ScientificDecimal.Abs(ScientificDecimal.PosInfinity));
        Assert.Equal(ScientificDecimal.PosInfinity, ScientificDecimal.Abs(ScientificDecimal.NegInfinity));
    }

    [Fact]
    public void ScientificDecimal_MinMethod()
    {
        Assert.Equal(-4, ScientificDecimal.Min(_testNegScientificDecimal, _testPosScientificDecimal));
        Assert.Equal(ScientificDecimal.NegInfinity, ScientificDecimal.Min(ScientificDecimal.PosInfinity, ScientificDecimal.NegInfinity));
        Assert.Equal(ScientificDecimal.NegInfinity, ScientificDecimal.Min(ScientificDecimal.NegInfinity, _testNegScientificDecimal));
    }
    
    [Fact]
    public void ScientificDecimal_MaxMethod()
    {
        Assert.Equal(-4, ScientificDecimal.Max(ScientificDecimal.NegInfinity, _testNegScientificDecimal));
        Assert.Equal(4, ScientificDecimal.Max(_testNegScientificDecimal, _testPosScientificDecimal));
        Assert.Equal(ScientificDecimal.PosInfinity, ScientificDecimal.Max(ScientificDecimal.PosInfinity, ScientificDecimal.NegInfinity));
    }

    /*[Fact]
    public void ScientificDecimal_ClampMethod()
    {
        ScientificDecimal argument = new ScientificDecimal(-8, 0);

        Assert.Equal(-4, ScientificDecimal.Clamp(argument, _testNegScientificDecimal, _testPosScientificDecimal));
        Assert.Equal(-4,
            ScientificDecimal.Clamp(ScientificDecimal.NegInfinity, _testNegScientificDecimal, _testPosScientificDecimal));
        Assert.Equal(4,
            ScientificDecimal.Clamp(ScientificDecimal.PosInfinity, _testNegScientificDecimal, _testPosScientificDecimal));
        Assert.Equal(ScientificDecimal.PosInfinity,
            ScientificDecimal.Clamp(ScientificDecimal.PosInfinity, _testNegScientificDecimal, ScientificDecimal.PosInfinity));
        Assert.Throws<ArithmeticException>(() =>
            ScientificDecimal.Clamp(argument, _testPosScientificDecimal, _testNegScientificDecimal));
    }

    [Fact]
    public void ScientificDecimal_RoundMethod()
    {
        ScientificDecimal argument = new(0.00001, 0);
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
        Assert.Equal(0, ScientificDecimal.Floor(new(0.00001, 0)));
        Assert.Equal(-1, new ScientificDecimal(-0.00001, 0).Floor());
        Assert.Equal(10000, new ScientificDecimal(10000.9, 0).Floor());
        Assert.Equal(10001, new ScientificDecimal(10001, 0).Floor());
        Assert.Equal(new ScientificDecimal(100), new ScientificDecimal(100).Floor());
    }*/
    
    #region Operators
    
    [Fact]
    public void ScientificDecimal_NegativeOperator()
    {
        Assert.Equal(-4, -_testPosScientificDecimal);
        Assert.Equal(ScientificDecimal.NegInfinity, -ScientificDecimal.PosInfinity);
    }
    
    [Fact]
    public void ScientificDecimal_AdditionOperator()
    {
        Assert.Equal(0, _testPosScientificDecimal + _testNegScientificDecimal);
        Assert.Equal(ScientificDecimal.PosInfinity, _testPosScientificDecimal + ScientificDecimal.PosInfinity);
        Assert.Equal(ScientificDecimal.PosInfinity, ScientificDecimal.PosInfinity + ScientificDecimal.PosInfinity);
        Assert.Throws<ArithmeticException>(() => ScientificDecimal.PosInfinity + ScientificDecimal.NegInfinity);
    }

    [Fact]
    public void ScientificDecimal_SubtractionOperator()
    {
        Assert.Equal(8, _testPosScientificDecimal - _testNegScientificDecimal);
        Assert.Equal(ScientificDecimal.PosInfinity, _testPosScientificDecimal - ScientificDecimal.NegInfinity);
        Assert.Throws<ArithmeticException>(() => ScientificDecimal.PosInfinity - ScientificDecimal.PosInfinity);
    }

    [Fact]
    public void ScientificDecimal_MultiplicationOperator()
    {
        Assert.Equal(-16, _testPosScientificDecimal * _testNegScientificDecimal);
        Assert.Equal(0, ScientificDecimal.Zero * ScientificDecimal.PosInfinity);
        Assert.Equal(ScientificDecimal.NegInfinity, _testNegScientificDecimal * ScientificDecimal.PosInfinity);
        Assert.Equal(ScientificDecimal.NegInfinity, ScientificDecimal.PosInfinity * ScientificDecimal.NegInfinity);
        Assert.Equal(ScientificDecimal.PosInfinity, ScientificDecimal.NegInfinity * ScientificDecimal.NegInfinity);
    }

    [Fact]
    public void ScientificDecimal_DivisionOperator()
    {
        Assert.Equal(-1, _testPosScientificDecimal / _testNegScientificDecimal);
        Assert.Equal(0, _testPosScientificDecimal / ScientificDecimal.PosInfinity);
        Assert.Equal(ScientificDecimal.NegInfinity, ScientificDecimal.PosInfinity / _testNegScientificDecimal);
        Assert.Equal(ScientificDecimal.NegInfinity, _testNegScientificDecimal / ScientificDecimal.Zero);
        Assert.Throws<ArithmeticException>(() => ScientificDecimal.Zero / ScientificDecimal.Zero);
        Assert.Throws<ArithmeticException>(() => ScientificDecimal.PosInfinity / ScientificDecimal.NegInfinity);
    }

    [Fact]
    public void ScientificDecimal_ModuloOperator()
    {
        Assert.Equal(2, new ScientificDecimal(2, 0) % new ScientificDecimal(4, 0));
        Assert.Equal(0, new ScientificDecimal(4, 0) % new ScientificDecimal(2, 0));
        Assert.Throws<ArithmeticException>(() => ScientificDecimal.PosInfinity % _testPosScientificDecimal);
        Assert.Throws<ArithmeticException>(() => _testPosScientificDecimal % ScientificDecimal.PosInfinity);
        Assert.Throws<ArithmeticException>(() => _testPosScientificDecimal % ScientificDecimal.Zero);
        Assert.Equal(3, new ScientificDecimal(10) % 13);
    }
    
    [Fact]
    public void ScientificDecimal_EqualsOperator()
    {
        Assert.False(_testPosScientificDecimal == _testNegScientificDecimal);
        Assert.False(_testPosScientificDecimal == ScientificDecimal.PosInfinity);
    }

    [Fact]
    public void ScientificDecimal_GreaterThanOperator()
    {
        Assert.True(ScientificDecimal.PosInfinity > ScientificDecimal.NegInfinity);
        Assert.True(_testPosScientificDecimal > _testNegScientificDecimal);
    }

    [Fact]
    public void ScientificDecimal_LessThanOperator()
    {
        Assert.True(_testNegScientificDecimal < _testPosScientificDecimal);
        Assert.False(ScientificDecimal.PosInfinity < _testPosScientificDecimal);
    }
    
    #endregion Operators
    
    #region Casts

    [Fact]
    public void ScientificDecimal_DoubleCast()
    {
        ScientificDecimal argument = new ScientificDecimal(-1.59, 1);
        
        Assert.Equal(-15.9d, (double)argument);
    }
    
    [Fact]
    public void ScientificDecimal_IntCast()
    {
        ScientificDecimal posArgument = new ScientificDecimal(1.59, 1);
        ScientificDecimal negArgument = new ScientificDecimal(-1.59, 1);
        
        Assert.Equal(15, (int)posArgument);
        Assert.Equal(-15, (int)negArgument);
    }
    
    #endregion Casts

    [Fact]
    public void ScientificDecimal_IsZeroMethod()
    {
        Assert.False(ScientificDecimal.IsZero(ScientificDecimal.PosInfinity));
        Assert.True(ScientificDecimal.IsZero(ScientificDecimal.Zero));
    }

    [Fact]
    public void ScientificDecimal_IsFiniteMethod()
    {
        Assert.True(ScientificDecimal.IsFinite(ScientificDecimal.Zero));
        Assert.True(ScientificDecimal.IsFinite(_testPosScientificDecimal));
        Assert.False(ScientificDecimal.IsFinite(ScientificDecimal.PosInfinity));
        Assert.False(ScientificDecimal.IsFinite(ScientificDecimal.NegInfinity));
    }

    [Fact]
    public void ScientificDecimal_IsRealNumberMethod()
    {
        Assert.True(ScientificDecimal.IsRealNumber(ScientificDecimal.Zero));
        Assert.True(ScientificDecimal.IsRealNumber(ScientificDecimal.PosInfinity));
    }
    
    [Fact]
    public void ScientificDecimal_IsImaginaryNumberMethod()
    {
        Assert.False(ScientificDecimal.IsImaginaryNumber(ScientificDecimal.Zero));
        Assert.False(ScientificDecimal.IsImaginaryNumber(ScientificDecimal.PosInfinity));
    }
    
    [Fact]
    public void ScientificDecimal_IsComplexNumberMethod()
    {
        Assert.False(ScientificDecimal.IsComplexNumber(ScientificDecimal.Zero));
        Assert.False(ScientificDecimal.IsComplexNumber(ScientificDecimal.PosInfinity));
    }

    [Fact]
    public void ScientificDecimal_IsIntegerMethod()
    {
        Assert.True(ScientificDecimal.IsInteger(ScientificDecimal.Zero));
        Assert.True(ScientificDecimal.IsInteger(new ScientificDecimal(10)));
        Assert.True(ScientificDecimal.IsInteger(new ScientificDecimal(0.5, 1)));
        Assert.False(ScientificDecimal.IsInteger(new ScientificDecimal(0.5, 0)));
        Assert.False(ScientificDecimal.IsInteger(ScientificDecimal.PosInfinity));
    }
    
    [Fact]
    public void ScientificDecimal_IsEvenIntegerMethod()
    {
        Assert.True(ScientificDecimal.IsEvenInteger(ScientificDecimal.Zero));
        Assert.False(ScientificDecimal.IsEvenInteger(ScientificDecimal.One));
        Assert.False(ScientificDecimal.IsEvenInteger(new ScientificDecimal(0.5, 0)));
        Assert.False(ScientificDecimal.IsEvenInteger(ScientificDecimal.PosInfinity));
    }
    
    [Fact]
    public void ScientificDecimal_IsOddIntegerMethod()
    {
        Assert.False(ScientificDecimal.IsOddInteger(ScientificDecimal.Zero));
        Assert.True(ScientificDecimal.IsOddInteger(ScientificDecimal.One));
        Assert.False(ScientificDecimal.IsOddInteger(new ScientificDecimal(0.5, 0)));
        Assert.False(ScientificDecimal.IsOddInteger(ScientificDecimal.PosInfinity));
    }

    [Fact]
    public void ScientificDecimal_IsNaN()
    {
        Assert.False(ScientificDecimal.IsNaN(ScientificDecimal.Zero));
        Assert.True(ScientificDecimal.IsNaN(ScientificDecimal.PosInfinity));
    }
    
    [Fact]
    public void ScientificDecimal_IsInfinityMethod()
    {
        Assert.False(ScientificDecimal.IsInfinity(ScientificDecimal.Zero));
        Assert.False(ScientificDecimal.IsInfinity(_testPosScientificDecimal));
        Assert.True(ScientificDecimal.IsInfinity(ScientificDecimal.PosInfinity));
        Assert.True(ScientificDecimal.IsInfinity(ScientificDecimal.NegInfinity));
    }

    [Fact]
    public void ScientificDecimal_IsNegativeInfinityMethod()
    {
        Assert.False(ScientificDecimal.IsNegativeInfinity(ScientificDecimal.Zero));
        Assert.False(ScientificDecimal.IsNegativeInfinity(ScientificDecimal.PosInfinity));
        Assert.True(ScientificDecimal.IsNegativeInfinity(ScientificDecimal.NegInfinity));
    }
    
    [Fact]
    public void ScientificDecimal_IsPositiveInfinityMethod()
    {
        Assert.False(ScientificDecimal.IsPositiveInfinity(ScientificDecimal.Zero));
        Assert.True(ScientificDecimal.IsPositiveInfinity(ScientificDecimal.PosInfinity));
        Assert.False(ScientificDecimal.IsPositiveInfinity(ScientificDecimal.NegInfinity));
    }
    
    [Fact]
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
    }
}