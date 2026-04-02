namespace OrbitGame.Tests;

public class PDecimal_Tests
{
    private readonly PDecimal _highPrecisionPDecimal;
    
    public PDecimal_Tests()
    {
        for (int i = 0; i < 350; ++i)
            _highPrecisionPDecimal += new PDecimal(1, 300 - i);
    }
    
    [Fact]
    public void PDecimal_Constructor()
    {
        PDecimal pDecimal = new PDecimal(1, 5);
        Assert.Equal(1, pDecimal.Mantissa);
        Assert.Equal(5, pDecimal.Exponent);

        pDecimal = new PDecimal(5);
        Assert.Equal(1, pDecimal.Mantissa);
        Assert.Equal(5, pDecimal.Exponent);

        pDecimal = new PDecimal();
        Assert.Equal(0, pDecimal.Mantissa);
        Assert.Equal(0, pDecimal.Exponent);

        pDecimal = new PDecimal(12.345, 100);
        Assert.Equal(12345, pDecimal.Mantissa);
        Assert.Equal(97, pDecimal.Exponent);
    }

    [Fact]
    public void PDecimal_Properties()
    {
        PDecimal positiveInfinitePDecimal = PDecimal.PositiveInfinity;

        Assert.Throws<ArithmeticException>(() => positiveInfinitePDecimal.Mantissa);
        Assert.Throws<ArithmeticException>(() => positiveInfinitePDecimal.Exponent);
        
        Assert.True(positiveInfinitePDecimal.Positive);
        Assert.False(positiveInfinitePDecimal.Negative);

        Assert.Equal(0, PDecimal.Zero);
        Assert.Equal(1, PDecimal.One);
    }

    [Fact]
    public void PDecimal_FromDoubleMethod()
    {
        double posValue = 100000;
        double negValue = -0.00001;
        
        Assert.Equal(new PDecimal(1, 5), PDecimal.FromDouble(posValue));
        Assert.Equal(new PDecimal(-1, 0), PDecimal.FromDouble(negValue, 5));
        
        Assert.Throws<ArgumentException>(() => PDecimal.FromDouble(double.NaN));
        
        Assert.Equal(PDecimal.PositiveInfinity, PDecimal.FromDouble(double.PositiveInfinity));
        Assert.Equal(PDecimal.NegativeInfinity, PDecimal.FromDouble(double.NegativeInfinity));
        Assert.Equal(PDecimal.Zero, PDecimal.FromDouble(double.NegativeZero));
    }

    [Fact]
    public void PDecimal_ToDoubleMethod()
    {
        PDecimal posPDecimal = new PDecimal(1, 5);
        PDecimal negPDecimal = new PDecimal(-1, -5);

        Assert.Equal(100000, PDecimal.ConvertToDouble(posPDecimal));
        Assert.Equal(-0.00001, PDecimal.ConvertToDouble(negPDecimal));
        Assert.Equal(1.111_111_111_111_111*Math.Pow(10, 300), PDecimal.ConvertToDouble(_highPrecisionPDecimal));
        
        Assert.Equal(double.PositiveInfinity, PDecimal.ConvertToDouble(PDecimal.PositiveInfinity));
        Assert.Equal(double.NegativeInfinity, PDecimal.ConvertToDouble(PDecimal.NegativeInfinity));
        
        Assert.Throws<OverflowException>(() => PDecimal.ConvertToDouble(new PDecimal(1000)));
        
        Assert.Equal(0, PDecimal.ConvertToDouble(new PDecimal(-1000)));
    }

    [Fact]
    public void PDecimal_ToDoubleSafeMethod()
    {
        PDecimal posPDecimal = new PDecimal(1, 5);
        PDecimal negPDecimal = new PDecimal(-1, -5);

        Assert.Equal(100000, PDecimal.ConvertToDoubleSaturating(posPDecimal));
        Assert.Equal(-0.00001, PDecimal.ConvertToDoubleSaturating(negPDecimal));
        
        Assert.Equal(double.PositiveInfinity, PDecimal.ConvertToDoubleSaturating(PDecimal.PositiveInfinity));
        Assert.Equal(double.NegativeInfinity, PDecimal.ConvertToDoubleSaturating(PDecimal.NegativeInfinity));
        
        Assert.Equal(double.MaxValue, PDecimal.ConvertToDoubleSaturating(new PDecimal(1000)));
        Assert.Equal(double.MinValue, PDecimal.ConvertToDoubleSaturating(new PDecimal(-1, 1000)));
        
        Assert.Equal(0, PDecimal.ConvertToDoubleSaturating(new PDecimal(-1000)));
    }

    [Fact]
    public void PDecimal_AddOperator()
    {
        Assert.Equal(7, (PDecimal)3 + 4);
        Assert.Equal(1_000_000_000_001L, new PDecimal(12) + 1);
        Assert.NotEqual(new PDecimal(1000), new PDecimal(1000) + 1);
        
        Assert.Equal(PDecimal.PositiveInfinity, 1 + PDecimal.PositiveInfinity);
        Assert.Equal(PDecimal.NegativeInfinity, 1 + PDecimal.NegativeInfinity);
        Assert.Equal(PDecimal.PositiveInfinity, PDecimal.PositiveInfinity + PDecimal.PositiveInfinity);
        Assert.Equal(PDecimal.NegativeInfinity, PDecimal.NegativeInfinity + PDecimal.NegativeInfinity);
        
        Assert.Throws<ArithmeticException>(() => PDecimal.PositiveInfinity + PDecimal.NegativeInfinity);
    }

    [Fact]
    public void PDecimal_SubtractOperator()
    {
        Assert.Equal(-1, (PDecimal)3 - 4);
        Assert.Equal(999_999_999_999L, new PDecimal(12) - 1);
        Assert.NotEqual(new PDecimal(1000), new PDecimal(1000) - new PDecimal(0));
        
        Assert.Equal(PDecimal.NegativeInfinity, 1 - PDecimal.PositiveInfinity);
        Assert.Equal(PDecimal.PositiveInfinity, 1 - PDecimal.NegativeInfinity);
        
        Assert.Throws<ArithmeticException>(() => PDecimal.PositiveInfinity - PDecimal.PositiveInfinity);
    }

    [Fact]
    public void PDecimal_MultiplyOperator()
    {
        Assert.Equal(12, (PDecimal)3 * 4);
        Assert.Equal(new PDecimal(2, 1000), new PDecimal(1000) * 2);
        Assert.Equal(new PDecimal(1, 2000), new PDecimal(1000) * new PDecimal(1000));
        Assert.Equal(new PDecimal(1, 2000) + new PDecimal(2, 1000) + 1, (new PDecimal(1000) + 1) * (new PDecimal(1000) + 1));
        
        Assert.Equal(PDecimal.PositiveInfinity, PDecimal.NegativeInfinity * PDecimal.NegativeInfinity);
        Assert.Equal(0, PDecimal.PositiveInfinity * 0);
    }

    [Fact]
    public void PDecimal_DivideOperator()
    {
        Assert.Equal(3, (PDecimal)12 / 4);
        Assert.Equal(new PDecimal(5, 999), new PDecimal(1000) / 2);
        
        Assert.Equal(PDecimal.PositiveInfinity,  1 / new PDecimal());
        Assert.Equal(PDecimal.NegativeInfinity, -1 / new PDecimal());
        Assert.Equal(PDecimal.NegativeInfinity, PDecimal.PositiveInfinity / -1);
        Assert.Equal(0, 1 / PDecimal.PositiveInfinity);
        
        Assert.Throws<ArithmeticException>(() => PDecimal.PositiveInfinity / PDecimal.PositiveInfinity);
        Assert.Throws<ArithmeticException>(() => (PDecimal)0 / 0);
    }

    [Fact]
    public void PDecimal_ModuloOperator()
    {
        Assert.Equal(1, new PDecimal(1000) % 3);
        Assert.Equal(-1, new PDecimal(-1, 1000) % 3);
        
        Assert.Throws<ArithmeticException>(() => new PDecimal(1000) % PDecimal.PositiveInfinity);
        Assert.Throws<ArithmeticException>(() => PDecimal.PositiveInfinity % new PDecimal(1000));
        Assert.Throws<ArithmeticException>(() => new PDecimal(1000) % 0);
    }

    [Fact]
    public void PDecimal_ModuloMethod()
    {
        Assert.Equal(1, PDecimal.Mod(new PDecimal(1000), 3));
        Assert.Equal(2, PDecimal.Mod(new PDecimal(-100, 0), 3));
    }

    [Fact]
    public void PDecimal_SquareMethod()
    {
        Assert.Equal(16, PDecimal.Square(4));
        Assert.Equal(16, PDecimal.Square(-4));
        Assert.Equal(new PDecimal(2000), PDecimal.Square(new PDecimal(1000)));
        Assert.Equal(PDecimal.PositiveInfinity, PDecimal.Square(PDecimal.PositiveInfinity));
    }

    [Fact]
    public void PDecimal_IntPowMethod()
    {
        Assert.Equal(16, PDecimal.IntPow(4, 2));
        Assert.Equal(1d/16, PDecimal.IntPow(4, -2));
        Assert.Equal(0, PDecimal.IntPow(PDecimal.PositiveInfinity, -1));
        Assert.Equal(1, PDecimal.IntPow(PDecimal.NegativeInfinity, 0));
    }

    [Fact]
    public void PDecimal_SqrtMethod()
    {
        Assert.Equal(4, PDecimal.Sqrt(new PDecimal(16, 0)));
        Assert.Equal(10, PDecimal.Sqrt(new PDecimal(100, 0)));
        Assert.Equal(new PDecimal(500), PDecimal.Sqrt(new PDecimal(1000)));
        Assert.Equal(PDecimal.PositiveInfinity, PDecimal.Sqrt(PDecimal.PositiveInfinity));
        Assert.Throws<ArithmeticException>(() => PDecimal.Sqrt(PDecimal.NegativeInfinity));
    }

    [Fact]
    public void PDecimal_Atan2Method()
    {
        
    }

    [Fact]
    public void PDecimal_CosMethod()
    {
        
    }

    [Fact]
    public void PDecimal_SinMethod()
    {
        
    }

    [Fact]
    public void PDecimal_TanMethod()
    {
        
    }

    [Fact]
    public void PDecimal_AbsMethod()
    {
        
    }

    [Fact]
    public void PDecimal_MinMethod()
    {
        
    }

    [Fact]
    public void PDecimal_MaxMethod()
    {
        
    }

    [Fact]
    public void PDecimal_RoundMethod()
    {
        
    }

    [Fact]
    public void PDecimal_FloorMethod()
    {
        
    }
    
    [Fact]
    public void PDecimal_CeilingMethod()
    {
        
    }

    [Fact]
    public void PDecimal_MinMagnitudeNumberMethod()
    {
        
    }

    [Fact]
    public void PDecimal_MaxMagnitudeNumberMethod()
    {
        
    }

    [Fact]
    public void PDecimal_ClampMethod()
    {
        
    }

    [Fact]
    public void PDecimal_MapMethod()
    {
        
    }

    [Fact]
    public void PDecimal_EqualityOperator()
    {
        
    }

    [Fact]
    public void PDecimal_InequalityOperator()
    {
        
    }
    
    [Fact]
    public void PDecimal_GreaterThanOperator()
    {
        
    }

    [Fact]
    public void PDecimal_GreaterThanOrEqualToOperator()
    {
        
    }

    [Fact]
    public void PDecimal_LessThanOperator()
    {
        
    }

    [Fact]
    public void PDecimal_LessThanOrEqualToOperator()
    {
        
    }

    [Fact]
    public void PDecimal_FromIntCast()
    {
        
    }

    [Fact]
    public void PDecimal_FromUIntCast()
    {
        
    }

    [Fact]
    public void PDecimal_FromLongCast()
    {
        
    }

    [Fact]
    public void PDecimal_FromFloatCast()
    {
        
    }

    [Fact]
    public void PDecimal_FromDoubleCast()
    {
        
    }

    [Fact]
    public void PDecimal_ToIntCast()
    {
        
    }

    [Fact]
    public void PDecimal_ToUIntCast()
    {
        
    }

    [Fact]
    public void PDecimal_ToLongCast()
    {
        
    }

    [Fact]
    public void PDecimal_ToFloatCast()
    {
        
    }

    [Fact]
    public void PDecimal_ToDoubleCast()
    {
        
    }

    [Fact]
    public void PDecimal_IsIntegerMethod()
    {
        
    }

    [Fact]
    public void PDecimal_IsEvenIntegerMethod()
    {
        
    }

    [Fact]
    public void PDecimal_IsOddIntegerMethod()
    {
        
    }

    [Fact]
    public void PDecimal_IsInfinityMethod()
    {
        
    }

    [Fact]
    public void PDecimal_IsPositiveInfinityMethod()
    {
        
    }

    [Fact]
    public void PDecimal_IsNegativeInfinityMethod()
    {
        
    }

    [Fact]
    public void PDecimal_ToStringMethod()
    {
        
    }

    [Fact]
    public void PDecimal_TryFormatMethod()
    {
        
    }

    [Fact]
    public void PDecimal_ParseMethod()
    {
        
    }

    [Fact]
    public void PDecimal_TryParseMethod()
    {
        
    }

    [Fact]
    public void PDecimal_CompareToMethod()
    {
        
    }

    [Fact]
    public void PDecimal_EqualsMethod()
    {
        
    }
}