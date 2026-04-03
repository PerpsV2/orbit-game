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
        Assert.Throws<DivideByZeroException>(() => (PDecimal)0 / 0);
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
        Assert.Equal(2, PDecimal.Mod(new PDecimal(-1, 1000), 3));
        
        Assert.Throws<ArithmeticException>(() => PDecimal.Mod(new PDecimal(1000), PDecimal.PositiveInfinity));
        Assert.Throws<ArithmeticException>(() => PDecimal.Mod(PDecimal.PositiveInfinity, new PDecimal(1000)));
        Assert.Throws<ArithmeticException>(() => PDecimal.Mod(new PDecimal(1000), 0));
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
        Assert.Equal(4, PDecimal.Sqrt(16));
        Assert.Equal(10, PDecimal.Sqrt(100));
        Assert.Equal(new PDecimal(500), PDecimal.Sqrt(new PDecimal(1000)));
        Assert.Equal(PDecimal.PositiveInfinity, PDecimal.Sqrt(PDecimal.PositiveInfinity));
        Assert.Throws<ArithmeticException>(() => PDecimal.Sqrt(PDecimal.NegativeInfinity));
    }

    [Fact]
    public void PDecimal_Atan2Method()
    {
        Assert.Equal(Math.PI / 4, PDecimal.Atan2(1, 1));
        Assert.Equal(-Math.PI / 4, PDecimal.Atan2(-1, 1));
        Assert.Equal(3 * Math.PI / 4, PDecimal.Atan2(1, -1));
        Assert.Equal(-3 * Math.PI / 4, PDecimal.Atan2(-1, -1));
        Assert.Equal(Math.PI / 2, PDecimal.Atan2(1, 0));
        Assert.Equal(-Math.PI / 2, PDecimal.Atan2(-1, 0));
        Assert.Throws<DivideByZeroException>(() => PDecimal.Atan2(0, 0));
    }

    [Fact]
    public void PDecimal_CosMethod()
    {
        Assert.Equal(1, PDecimal.Cos(0));
        Assert.Equal(0, PDecimal.Cos(Math.PI / 2), Assert.Epsilon);
        Assert.Equal(-1, PDecimal.Cos(Math.PI), Assert.Epsilon);
        Assert.Equal(0, PDecimal.Cos(3 * Math.PI / 2), Assert.Epsilon);
        Assert.Equal(0, PDecimal.Cos(-Math.PI / 2), Assert.Epsilon);
        Assert.Equal(1, PDecimal.Cos(new PDecimal(Math.Tau, 10)), Assert.Epsilon);
    }

    [Fact]
    public void PDecimal_SinMethod()
    {
        Assert.Equal(0, PDecimal.Sin(0));
        Assert.Equal(1, PDecimal.Sin(Math.PI / 2), Assert.Epsilon);
        Assert.Equal(0, PDecimal.Sin(Math.PI), Assert.Epsilon);
        Assert.Equal(-1, PDecimal.Sin(3 * Math.PI / 2), Assert.Epsilon);
        Assert.Equal(-1, PDecimal.Sin(-Math.PI / 2), Assert.Epsilon);
        Assert.Equal(0, PDecimal.Sin(new PDecimal(Math.Tau, 10)), Assert.Epsilon);
    }

    [Fact]
    public void PDecimal_TanMethod()
    {
        Assert.Equal(0, PDecimal.Tan(0));
        Assert.Equal(Math.Sqrt(3), PDecimal.Tan(Math.PI / 3), Assert.Epsilon);
        Assert.Equal(-Math.Sqrt(3), PDecimal.Tan(-Math.PI / 3), Assert.Epsilon);
        Assert.Equal(0, PDecimal.Tan(new PDecimal(Math.PI, 10)), Assert.Epsilon);
    }

    [Fact]
    public void PDecimal_AbsMethod()
    {
        Assert.Equal(1, PDecimal.Abs(1));
        Assert.Equal(1, PDecimal.Abs(-1));
        Assert.Equal(PDecimal.PositiveInfinity, PDecimal.Abs(PDecimal.NegativeInfinity));
    }

    [Fact]
    public void PDecimal_MinMethod()
    {
        Assert.Equal(-1, PDecimal.Min(1, -1));
        Assert.Equal(1, PDecimal.Min(1, PDecimal.PositiveInfinity));
        Assert.Equal(PDecimal.NegativeInfinity, PDecimal.Min(1, PDecimal.NegativeInfinity));
        Assert.Equal(PDecimal.NegativeInfinity, PDecimal.Min(PDecimal.PositiveInfinity, PDecimal.NegativeInfinity));
    }

    [Fact]
    public void PDecimal_MaxMethod()
    {
        Assert.Equal(1, PDecimal.Max(1, -1));
        Assert.Equal(PDecimal.PositiveInfinity, PDecimal.Max(1, PDecimal.PositiveInfinity));
        Assert.Equal(1, PDecimal.Max(1, PDecimal.NegativeInfinity));
        Assert.Equal(PDecimal.PositiveInfinity, PDecimal.Max(PDecimal.PositiveInfinity, PDecimal.NegativeInfinity));

    }

    [Fact]
    public void PDecimal_RoundMethod()
    {
        Assert.Equal(0, PDecimal.Round(0));
        Assert.Equal(0, PDecimal.Round(0.1));
        Assert.Equal(0, PDecimal.Round(-0.1));
        Assert.Equal(1, PDecimal.Round(1));
        
        Assert.Equal(0, PDecimal.Round(0.5, MidpointRounding.ToZero));
        Assert.Equal(0, PDecimal.Round(-0.5, MidpointRounding.ToZero));
        
        Assert.Equal(1, PDecimal.Round(0.5, MidpointRounding.AwayFromZero));
        Assert.Equal(-1, PDecimal.Round(-0.5, MidpointRounding.AwayFromZero));
        
        Assert.Equal(0, PDecimal.Round(0.5, MidpointRounding.ToNegativeInfinity));
        Assert.Equal(-1, PDecimal.Round(-0.5, MidpointRounding.ToNegativeInfinity));
        
        Assert.Equal(1, PDecimal.Round(0.5, MidpointRounding.ToPositiveInfinity));
        Assert.Equal(0, PDecimal.Round(-0.5, MidpointRounding.ToPositiveInfinity));
        
        Assert.Equal(2, PDecimal.Round(2.5));
        Assert.Equal(2, PDecimal.Round(1.5));
        
        Assert.Throws<ArithmeticException>(() => PDecimal.Round(PDecimal.PositiveInfinity));
    }

    [Fact]
    public void PDecimal_FloorMethod()
    {
        Assert.Equal(0, PDecimal.Floor(0.9));
        Assert.Equal(1, PDecimal.Floor(1.1));
        Assert.Equal(-1, PDecimal.Floor(-0.1));
        Assert.Equal(10000, PDecimal.Floor(10000.9));
        
        Assert.Throws<ArithmeticException>(() => PDecimal.Floor(PDecimal.PositiveInfinity));
    }
    
    [Fact]
    public void PDecimal_CeilingMethod()
    {
        Assert.Equal(1, PDecimal.Ceiling(0.9));
        Assert.Equal(2, PDecimal.Ceiling(1.1));
        Assert.Equal(0, PDecimal.Ceiling(-0.9));
        Assert.Equal(10001, PDecimal.Ceiling(10000.1));
        
        Assert.Throws<ArithmeticException>(() => PDecimal.Ceiling(PDecimal.PositiveInfinity));
    }

    [Fact]
    public void PDecimal_MinMagnitudeNumberMethod()
    {
        Assert.Equal(-1, PDecimal.MinMagnitudeNumber(1, -1));
        Assert.Equal(1, PDecimal.MinMagnitudeNumber(1, PDecimal.NegativeInfinity));
    }

    [Fact]
    public void PDecimal_MaxMagnitudeNumberMethod()
    {
        Assert.Equal(1, PDecimal.MaxMagnitudeNumber(1, -1));
        Assert.Equal(1, PDecimal.MaxMagnitudeNumber(1, PDecimal.PositiveInfinity));
    }

    [Fact]
    public void PDecimal_ClampMethod()
    {
        Assert.Equal(-1, PDecimal.Clamp(-5, -1, 1));
        Assert.Equal(-1, PDecimal.Clamp(PDecimal.NegativeInfinity, -1, 1));
        Assert.Equal(PDecimal.PositiveInfinity,
            PDecimal.Clamp(PDecimal.PositiveInfinity, -1, PDecimal.PositiveInfinity)
        );
        Assert.Throws<ArgumentException>(() => PDecimal.Clamp(0, 1, -1));
    }

    [Fact]
    public void PDecimal_MapMethod()
    {
        Assert.Equal(new SDecimal(1000), new PDecimal(1000).Map<SDecimal>());
        Assert.Equal(new PDecimal(1000), new PDecimal(1000).Map<PDecimal>());
    }

    [Fact]
    public void PDecimal_EqualityOperator()
    {
        Assert.True(100 == new PDecimal(100, 0));
    }

    [Fact]
    public void PDecimal_InequalityOperator()
    {
        Assert.False(100 != new PDecimal(100, 0));
    }
    
    [Fact]
    public void PDecimal_GreaterThanOperator()
    {
        Assert.True((PDecimal)1 > -1);
        Assert.True(1 > PDecimal.NegativeInfinity);
        Assert.True(PDecimal.PositiveInfinity > PDecimal.NegativeInfinity);
        Assert.True(PDecimal.PositiveInfinity > new PDecimal(1000));
        Assert.True(new PDecimal(1000) > 0);
        Assert.False((PDecimal)1 > 1);
    }

    [Fact]
    public void PDecimal_GreaterThanOrEqualToOperator()
    {
        Assert.True((PDecimal)1 >= -1);
        Assert.True(1 >= PDecimal.NegativeInfinity);
        Assert.True(PDecimal.PositiveInfinity >= PDecimal.NegativeInfinity);
        Assert.True(PDecimal.PositiveInfinity >= new PDecimal(1000));
        Assert.True(new PDecimal(1000) >= 0);
        Assert.True((PDecimal)1 >= 1);
    }

    [Fact]
    public void PDecimal_LessThanOperator()
    {
        Assert.False((PDecimal)1 < -1);
        Assert.False(1 < PDecimal.NegativeInfinity);
        Assert.False(PDecimal.PositiveInfinity < PDecimal.NegativeInfinity);
        Assert.False(PDecimal.PositiveInfinity < new PDecimal(1000));
        Assert.False(new PDecimal(1000) < 0);
        Assert.False((PDecimal)1 < 1);
    }

    [Fact]
    public void PDecimal_LessThanOrEqualToOperator()
    {
        Assert.False((PDecimal) 1 <= -1);
        Assert.False(1 <= PDecimal.NegativeInfinity);
        Assert.False(PDecimal.PositiveInfinity <= PDecimal.NegativeInfinity);
        Assert.False(PDecimal.PositiveInfinity <= new PDecimal(1000));
        Assert.False(new PDecimal(1000) <= 0);
        Assert.True((PDecimal)1 <= 1);
    }

    [Fact]
    public void PDecimal_FromIntCast()
    {
        Assert.Equal(new PDecimal(1, 0), 1);
        Assert.Equal(new PDecimal(-1, 0), -1);
    }

    [Fact]
    public void PDecimal_FromUIntCast()
    {
        Assert.Equal(new PDecimal(1, 0), 1u);
    }

    [Fact]
    public void PDecimal_FromLongCast()
    {
        Assert.Equal(new PDecimal(1, 0), 1L);
        Assert.Equal(new PDecimal(-1, 0), -1L);
    }

    [Fact]
    public void PDecimal_FromFloatCast()
    {
        Assert.Equal(new PDecimal(1, -1), 0.1f);
        Assert.Equal(PDecimal.PositiveInfinity, float.PositiveInfinity);
        Assert.Equal(PDecimal.NegativeInfinity, float.NegativeInfinity);
        Assert.Throws<ArgumentException>(() => (PDecimal)float.NaN);
    }

    [Fact]
    public void PDecimal_FromDoubleCast()
    {
        Assert.Equal(new PDecimal(1, -1), 1);
        Assert.Equal(PDecimal.PositiveInfinity, double.PositiveInfinity);
        Assert.Equal(PDecimal.NegativeInfinity, double.NegativeInfinity);
        Assert.Throws<ArgumentException>(() => (PDecimal)double.NaN);
    }

    [Fact]
    public void PDecimal_ToIntCast()
    {
        Assert.Equal(0, (int)new PDecimal(1, -1));
        Assert.Equal(0, (int)new PDecimal(9, -1));
        Assert.Throws<OverflowException>(() => (int)new PDecimal(1000));
    }

    [Fact]
    public void PDecimal_ToUIntCast()
    {
        Assert.Equal(0u, (uint)new PDecimal(1, -1));
        Assert.Equal(0u, (uint)new PDecimal(9, -1));
        Assert.Throws<OverflowException>(() => (uint)new PDecimal(-1, 0));
        Assert.Throws<OverflowException>(() => (uint)new PDecimal(1000));
    }

    [Fact]
    public void PDecimal_ToLongCast()
    {
        Assert.Equal(0, (int)new PDecimal(1, -1));
        Assert.Equal(0, (int)new PDecimal(9, -1));
        Assert.Throws<OverflowException>(() => (int)new SDecimal(1000));
    }

    [Fact]
    public void PDecimal_ToFloatCast()
    {
        Assert.Equal(0.1f, (float)new PDecimal(1, -1));
        Assert.Equal(float.PositiveInfinity, (float)PDecimal.PositiveInfinity);
        Assert.Equal(float.NegativeInfinity, (float)PDecimal.NegativeInfinity);
        Assert.Throws<OverflowException>(() => (float)new PDecimal(1000));
    }

    [Fact]
    public void PDecimal_ToDoubleCast()
    {
        Assert.Equal(0.1, (double)new PDecimal(1, -1));
        Assert.Equal(double.PositiveInfinity, (double)PDecimal.PositiveInfinity);
        Assert.Equal(double.NegativeInfinity, (double)PDecimal.NegativeInfinity);
        Assert.Throws<OverflowException>(() => (double)new PDecimal(1000));
    }

    [Fact]
    public void PDecimal_IsIntegerMethod()
    {
        Assert.True(PDecimal.IsInteger(1));
        Assert.True(PDecimal.IsInteger(-1));
        Assert.True(PDecimal.IsInteger(new PDecimal(1000)));
        
        Assert.False(PDecimal.IsInteger(0.1));
        Assert.False(PDecimal.IsInteger(new PDecimal(1000) + 0.1));
        Assert.False(PDecimal.IsInteger(PDecimal.PositiveInfinity));
    }

    [Fact]
    public void PDecimal_IsEvenIntegerMethod()
    {
        Assert.False(PDecimal.IsEvenInteger(1));
        Assert.True(PDecimal.IsEvenInteger(2));
        
        Assert.False(PDecimal.IsEvenInteger(0.1));
        Assert.False(PDecimal.IsEvenInteger(new PDecimal(1000) + 0.1));
        Assert.False(PDecimal.IsEvenInteger(PDecimal.PositiveInfinity));
    }

    [Fact]
    public void PDecimal_IsOddIntegerMethod()
    {
        Assert.True(PDecimal.IsOddInteger(1));
        Assert.False(PDecimal.IsOddInteger(2));
        
        Assert.False(PDecimal.IsOddInteger(0.1));
        Assert.False(PDecimal.IsOddInteger(new PDecimal(1000) + 0.1));
        Assert.False(PDecimal.IsOddInteger(PDecimal.PositiveInfinity));
    }

    [Fact]
    public void PDecimal_IsInfinityMethod()
    {
        Assert.True(PDecimal.IsInfinity(PDecimal.PositiveInfinity));
        Assert.True(PDecimal.IsInfinity(PDecimal.NegativeInfinity));
        Assert.False(PDecimal.IsInfinity(new PDecimal(1000)));
    }

    [Fact]
    public void PDecimal_IsPositiveInfinityMethod()
    {
        Assert.True(PDecimal.IsInfinity(PDecimal.PositiveInfinity));
        Assert.False(PDecimal.IsInfinity(PDecimal.NegativeInfinity));
        Assert.False(PDecimal.IsInfinity(new PDecimal(1000)));
    }

    [Fact]
    public void PDecimal_IsNegativeInfinityMethod()
    {
        Assert.False(PDecimal.IsInfinity(PDecimal.PositiveInfinity));
        Assert.True(PDecimal.IsInfinity(PDecimal.NegativeInfinity));
        Assert.False(PDecimal.IsInfinity(new PDecimal(1000)));
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