using ArithmeticException = System.ArithmeticException;

namespace OrbitGame.Tests;

public class SDecimal_Tests
{
    [Fact]
    public void SDecimal_Constructor()
    {
        SDecimal sDecimal = new SDecimal(1, 5);
        Assert.Equal(1, sDecimal.Mantissa);
        Assert.Equal(5, sDecimal.Exponent);

        sDecimal = new SDecimal(5);
        Assert.Equal(1, sDecimal.Mantissa);
        Assert.Equal(5, sDecimal.Exponent);

        sDecimal = new SDecimal();
        Assert.Equal(0, sDecimal.Mantissa);
        Assert.Equal(0, sDecimal.Exponent);

        sDecimal = new SDecimal(10, 5);
        Assert.Equal(1, sDecimal.Mantissa);
        Assert.Equal(6, sDecimal.Exponent);

        sDecimal = new SDecimal(0.1, 5);
        Assert.Equal(1, sDecimal.Mantissa);
        Assert.Equal(4, sDecimal.Exponent);
    }

    [Fact]
    public void SDecimal_Properties()
    {
        SDecimal positiveInfiniteSDecimal = SDecimal.PositiveInfinity;
        
        Assert.Throws<ArithmeticException>(() => positiveInfiniteSDecimal.Mantissa);
        Assert.Throws<ArithmeticException>(() => positiveInfiniteSDecimal.Exponent);
        
        Assert.True(positiveInfiniteSDecimal.Positive);
        Assert.False(positiveInfiniteSDecimal.Negative);

        Assert.Equal(0, SDecimal.Zero);
        Assert.Equal(1, SDecimal.One);
    }

    [Fact]
    public void SDecimal_FromDoubleMethod()
    {
        double posValue = 100000;
        double negValue = -0.00001;
        
        Assert.Equal(new SDecimal(1, 5), SDecimal.FromDouble(posValue));
        Assert.Equal(new SDecimal(-1, 0), SDecimal.FromDouble(negValue, 5));
        Assert.Throws<ArgumentException>(() => SDecimal.FromDouble(double.NaN));
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.FromDouble(double.PositiveInfinity));
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.FromDouble(double.NegativeInfinity));
        Assert.Equal(SDecimal.Zero, SDecimal.FromDouble(double.NegativeZero));
        Assert.Equal(SDecimal.DoubleEpsilon, SDecimal.FromDouble(double.Epsilon));
    }

    [Fact]
    public void SDecimal_ToDoubleMethod()
    {
        SDecimal posSDecimal = new SDecimal(1, 5);
        SDecimal negSDecimal = new SDecimal(-1, -5);
        
        Assert.Equal(100000, SDecimal.ConvertToDouble(posSDecimal));
        Assert.Equal(-0.00001, SDecimal.ConvertToDouble(negSDecimal));
        Assert.Equal(double.PositiveInfinity, SDecimal.ConvertToDouble(SDecimal.PositiveInfinity));
        Assert.Equal(double.NegativeInfinity, SDecimal.ConvertToDouble(SDecimal.NegativeInfinity));
        Assert.Throws<OverflowException>(() => SDecimal.ConvertToDouble(new SDecimal(1000)));
        Assert.Equal(0, SDecimal.ConvertToDouble(new SDecimal(-1000)));
    }

    [Fact]
    public void SDecimal_ToDoubleSafeMethod()
    {
        SDecimal posSDecimal = new SDecimal(1, 5);
        SDecimal negSDecimal = new SDecimal(-1, -5);
        
        Assert.Equal(100000, SDecimal.ConvertToDoubleSaturating(posSDecimal));
        Assert.Equal(-0.00001, SDecimal.ConvertToDoubleSaturating(negSDecimal));
        Assert.Equal(double.PositiveInfinity, SDecimal.ConvertToDoubleSaturating(SDecimal.PositiveInfinity));
        Assert.Equal(double.NegativeInfinity, SDecimal.ConvertToDoubleSaturating(SDecimal.NegativeInfinity));
        Assert.Equal(double.MaxValue, SDecimal.ConvertToDoubleSaturating(new SDecimal(1000)));
        Assert.Equal(double.MinValue, SDecimal.ConvertToDoubleSaturating(new SDecimal(-1, 1000)));
        Assert.Equal(0, SDecimal.ConvertToDouble(new SDecimal(-1000)));
    }

    [Fact]
    public void SDecimal_AddOperator()
    {
        Assert.Equal(7, (SDecimal)3 + 4);
        Assert.Equal(100001, new SDecimal(5) + 1);
        Assert.Equal(SDecimal.PositiveInfinity, 1 + SDecimal.PositiveInfinity);
        Assert.Equal(SDecimal.NegativeInfinity, 1 + SDecimal.NegativeInfinity);
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.PositiveInfinity + SDecimal.PositiveInfinity);
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.NegativeInfinity + SDecimal.NegativeInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.PositiveInfinity + SDecimal.NegativeInfinity);
    }

    [Fact]
    public void SDecimal_SubtractOperator()
    {
        Assert.Equal(-1, (SDecimal)3 - 4);
        Assert.Equal(99999, new SDecimal(5) - 1);
        Assert.Equal(SDecimal.NegativeInfinity, 1 - SDecimal.PositiveInfinity);
        Assert.Equal(SDecimal.PositiveInfinity, 1 - SDecimal.NegativeInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.PositiveInfinity - SDecimal.PositiveInfinity);
    }

    [Fact]
    public void SDecimal_MultiplyOperator()
    {
        Assert.Equal(12, (SDecimal)3 * 4);
        Assert.Equal(-12, (SDecimal)3 * -4);
        Assert.Equal(new SDecimal(2, 1000), new SDecimal(1000) * 2);
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.NegativeInfinity * SDecimal.NegativeInfinity);
        Assert.Equal(0, SDecimal.PositiveInfinity * 0);
    }

    [Fact]
    public void SDecimal_DivideOperator()
    {
        Assert.Equal(3, (SDecimal)12 / 4);
        Assert.Equal(-4, (SDecimal)12 / -3);
        Assert.Equal(new SDecimal(5, 999), new SDecimal(1000) / 2);
        Assert.Equal(SDecimal.PositiveInfinity, 1 / new SDecimal());
        Assert.Equal(SDecimal.NegativeInfinity, -1 / new SDecimal());
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.PositiveInfinity / new SDecimal(1000));
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.PositiveInfinity / -1);
        Assert.Equal(0, new SDecimal(1000) / SDecimal.PositiveInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.PositiveInfinity / SDecimal.PositiveInfinity);
        Assert.Throws<DivideByZeroException>(() => new SDecimal() / new SDecimal());
    }

    [Fact]
    public void SDecimal_ModuloOperator()
    {
        Assert.Equal(1, (SDecimal)100 % 3);
        Assert.Equal(-1, (SDecimal)(-100) % 3);
        Assert.Throws<ArithmeticException>(() => new SDecimal(100, 0) % 0);
        Assert.Throws<ArithmeticException>(() => SDecimal.PositiveInfinity % 3);
        Assert.Throws<ArithmeticException>(() => new SDecimal(100, 0) % SDecimal.PositiveInfinity);
    }

    [Fact]
    public void SDecimal_ModuloMethod()
    {
        Assert.Equal(1, SDecimal.Mod(100, 3));
        Assert.Equal(2, SDecimal.Mod(-100, 3));
        Assert.Throws<ArithmeticException>(() => SDecimal.Mod(100, 0));
        Assert.Throws<ArithmeticException>(() => SDecimal.Mod(SDecimal.PositiveInfinity, 3));
        Assert.Throws<ArithmeticException>(() => SDecimal.Mod(100, SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_SquareMethod()
    {
        Assert.Equal(16, SDecimal.Square(4));
        Assert.Equal(16, SDecimal.Square(-4));
        Assert.Equal(new SDecimal(2000), SDecimal.Square(new SDecimal(1000)));
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.Square(SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_IntPowMethod()
    {
        Assert.Equal(16, SDecimal.IntPow(4, 2));
        Assert.Equal(1d/16, SDecimal.IntPow(4, -2));
        Assert.Equal(0, SDecimal.IntPow(SDecimal.PositiveInfinity, -1));
        Assert.Equal(1, SDecimal.IntPow(SDecimal.NegativeInfinity, 0));
    }

    [Fact]
    public void SDecimal_SqrtMethod()
    {
        Assert.Equal(4, SDecimal.Sqrt(16));
        Assert.Equal(10, SDecimal.Sqrt(100));
        Assert.Equal(new SDecimal(500), SDecimal.Sqrt(new SDecimal(1000)));
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.Sqrt(SDecimal.PositiveInfinity));
        Assert.Throws<ArithmeticException>(() => SDecimal.Sqrt(SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_Atan2Method()
    {
        Assert.Equal(Math.PI / 4, SDecimal.Atan2(1, 1));
        Assert.Equal(-Math.PI / 4, SDecimal.Atan2(-1, 1));
        Assert.Equal(3 * Math.PI / 4, SDecimal.Atan2(1, -1));
        Assert.Equal(-3 * Math.PI / 4, SDecimal.Atan2(-1, -1));
        Assert.Equal(Math.PI / 2, SDecimal.Atan2(1, 0));
        Assert.Equal(-Math.PI / 2, SDecimal.Atan2(-1, 0));
        Assert.Throws<DivideByZeroException>(() => SDecimal.Atan2(0, 0));
    }

    [Fact]
    public void SDecimal_CosMethod()
    {
        Assert.Equal(1, SDecimal.Cos(0));
        Assert.Equal(0, SDecimal.Cos(Math.PI / 2), Assert.Epsilon);
        Assert.Equal(-1, SDecimal.Cos(Math.PI), Assert.Epsilon);
        Assert.Equal(0, SDecimal.Cos(3 * Math.PI / 2), Assert.Epsilon);
        Assert.Equal(0, SDecimal.Cos(-Math.PI / 2), Assert.Epsilon);
        Assert.Equal(1, SDecimal.Cos(new SDecimal(Math.Tau, 10)), Assert.Epsilon);
    }

    [Fact]
    public void SDecimal_SinMethod()
    {
        Assert.Equal(0, SDecimal.Sin(0));
        Assert.Equal(1, SDecimal.Sin(Math.PI / 2), Assert.Epsilon);
        Assert.Equal(0, SDecimal.Sin(Math.PI), Assert.Epsilon);
        Assert.Equal(-1, SDecimal.Sin(3 * Math.PI / 2), Assert.Epsilon);
        Assert.Equal(-1, SDecimal.Sin(-Math.PI / 2), Assert.Epsilon);
        Assert.Equal(0, SDecimal.Sin(new SDecimal(Math.Tau, 10)), Assert.Epsilon);
    }

    [Fact]
    public void SDecimal_TanMethod()
    {
        Assert.Equal(0, SDecimal.Tan(0));
        Assert.Equal(Math.Sqrt(3), SDecimal.Tan(Math.PI / 3), Assert.Epsilon);
        Assert.Equal(-Math.Sqrt(3), SDecimal.Tan(-Math.PI / 3), Assert.Epsilon);
        Assert.Equal(0, SDecimal.Tan(new SDecimal(Math.PI, 10)), Assert.Epsilon);
    }

    [Fact]
    public void SDecimal_AbsMethod()
    {
        Assert.Equal(1, SDecimal.Abs(1));
        Assert.Equal(1, SDecimal.Abs(-1));
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.Abs(SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_MinMethod()
    {
        Assert.Equal(-1, SDecimal.Min(1, -1));
        Assert.Equal(1, SDecimal.Min(1, SDecimal.PositiveInfinity));
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.Min(1, SDecimal.NegativeInfinity));
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.Min(SDecimal.PositiveInfinity, SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_MaxMethod()
    {
        Assert.Equal(1, SDecimal.Max(1, -1));
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.Max(1, SDecimal.PositiveInfinity));
        Assert.Equal(1, SDecimal.Max(1, SDecimal.NegativeInfinity));
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.Max(SDecimal.PositiveInfinity, SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_RoundMethod()
    {
        Assert.Equal(0, SDecimal.Round(0));
        Assert.Equal(0, SDecimal.Round(0.1));
        Assert.Equal(0, SDecimal.Round(-0.1));
        Assert.Equal(1, SDecimal.Round(1));
        
        Assert.Equal(0, SDecimal.Round(0.5, MidpointRounding.ToZero));
        Assert.Equal(0, SDecimal.Round(-0.5, MidpointRounding.ToZero));
        
        Assert.Equal(1, SDecimal.Round(0.5, MidpointRounding.AwayFromZero));
        Assert.Equal(-1, SDecimal.Round(-0.5, MidpointRounding.AwayFromZero));
        
        Assert.Equal(0, SDecimal.Round(0.5, MidpointRounding.ToNegativeInfinity));
        Assert.Equal(-1, SDecimal.Round(-0.5, MidpointRounding.ToNegativeInfinity));
        
        Assert.Equal(1, SDecimal.Round(0.5, MidpointRounding.ToPositiveInfinity));
        Assert.Equal(0, SDecimal.Round(-0.5, MidpointRounding.ToPositiveInfinity));
        
        Assert.Equal(2, SDecimal.Round(2.5));
        Assert.Equal(2, SDecimal.Round(1.5));
        
        Assert.Throws<ArithmeticException>(() => SDecimal.Round(SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_FloorMethod()
    {
        Assert.Equal(0, SDecimal.Floor(0.9));
        Assert.Equal(1, SDecimal.Floor(1.1));
        Assert.Equal(-1, SDecimal.Floor(-0.1));
        Assert.Equal(10000, SDecimal.Floor(10000.9));
        
        Assert.Throws<ArithmeticException>(() => SDecimal.Floor(SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_CeilingMethod()
    {
        Assert.Equal(1, SDecimal.Ceiling(0.9));
        Assert.Equal(2, SDecimal.Ceiling(1.1));
        Assert.Equal(0, SDecimal.Ceiling(-0.9));
        Assert.Equal(10001, SDecimal.Ceiling(10000.1));
        
        Assert.Throws<ArithmeticException>(() => SDecimal.Ceiling(SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_MinMagnitudeNumberMethod()
    {
        Assert.Equal(-1, SDecimal.MinMagnitudeNumber(1, -1));
        Assert.Equal(1, SDecimal.MinMagnitudeNumber(1, SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_MaxMagnitudeNumberMethod()
    {
        Assert.Equal(1, SDecimal.MaxMagnitudeNumber(1, -1));
        Assert.Equal(1, SDecimal.MaxMagnitudeNumber(1, SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_ClampMethod()
    {
        Assert.Equal(-1, SDecimal.Clamp(-5, -1, 1));
        Assert.Equal(-1, SDecimal.Clamp(SDecimal.NegativeInfinity, -1, 1));
        Assert.Equal(SDecimal.PositiveInfinity,
            SDecimal.Clamp(SDecimal.PositiveInfinity, -1, SDecimal.PositiveInfinity)
        );
        Assert.Throws<ArgumentException>(() => SDecimal.Clamp(0, 1, -1));
    }

    [Fact]
    public void SDecimal_MapMethod()
    {
        Assert.Equal(new PDecimal(1000), new SDecimal(1000).Map<PDecimal>());
        Assert.Equal(new SDecimal(1000), new SDecimal(1000).Map<SDecimal>());
    }

    [Fact]
    public void SDecimal_EqualityOperator()
    {
        Assert.True(100 == new SDecimal(100, 0));
    }

    [Fact]
    public void SDecimal_InequalityOperator()
    {
        Assert.False(100 != new SDecimal(100, 0));
    }

    [Fact]
    public void SDecimal_GreaterThanOperator()
    {
        Assert.True((SDecimal)1 > -1);
        Assert.True(1 > SDecimal.NegativeInfinity);
        Assert.True(SDecimal.PositiveInfinity > SDecimal.NegativeInfinity);
        Assert.True(SDecimal.PositiveInfinity > new SDecimal(1000));
        Assert.True(new SDecimal(1000) > 0);
        Assert.False((SDecimal)1 > 1);
    }

    [Fact]
    public void SDecimal_GreaterThanOrEqualToOperator()
    {
        Assert.True((SDecimal)1 >= -1);
        Assert.True(1 >= SDecimal.NegativeInfinity);
        Assert.True(SDecimal.PositiveInfinity >= SDecimal.NegativeInfinity);
        Assert.True(SDecimal.PositiveInfinity >= new SDecimal(1000));
        Assert.True(new SDecimal(1000) >= 0);
        Assert.True((SDecimal)1 >= 1);
    }

    [Fact]
    public void SDecimal_LessThanOperator()
    {
        Assert.False((SDecimal)1 < -1);
        Assert.False(1 < SDecimal.NegativeInfinity);
        Assert.False(SDecimal.PositiveInfinity < SDecimal.NegativeInfinity);
        Assert.False(SDecimal.PositiveInfinity < new SDecimal(1000));
        Assert.False(new SDecimal(1000) < 0);
        Assert.False((SDecimal)1 < 1);
    }

    [Fact]
    public void SDecimal_LessThanOrEqualToOperator()
    {
        Assert.False((SDecimal) 1 <= -1);
        Assert.False(1 <= SDecimal.NegativeInfinity);
        Assert.False(SDecimal.PositiveInfinity <= SDecimal.NegativeInfinity);
        Assert.False(SDecimal.PositiveInfinity <= new SDecimal(1000));
        Assert.False(new SDecimal(1000) <= 0);
        Assert.True((SDecimal)1 <= 1);
    }

    [Fact]
    public void SDecimal_FromIntCast()
    {
        Assert.Equal(new SDecimal(1, 0), 1);
        Assert.Equal(new SDecimal(-1, 0), -1);
    }

    [Fact]
    public void SDecimal_FromDoubleCast()
    {
        Assert.Equal(new SDecimal(2.5, -1), 0.25);
        Assert.Equal(SDecimal.PositiveInfinity, double.PositiveInfinity);
        Assert.Equal(SDecimal.NegativeInfinity, double.NegativeInfinity);
        Assert.Equal(SDecimal.DoubleEpsilon, double.Epsilon);
        Assert.Throws<ArgumentException>(() => (SDecimal)double.NaN);
    }

    [Fact]
    public void SDecimal_FromFloatCast()
    {
        Assert.Equal(new SDecimal(2.5, -1), 0.25f);
        Assert.Equal(SDecimal.PositiveInfinity, float.PositiveInfinity);
        Assert.Equal(SDecimal.NegativeInfinity, float.NegativeInfinity);
        Assert.Throws<ArgumentException>(() => (SDecimal)float.NaN);
    }

    [Fact]
    public void SDecimal_ToDoubleCast()
    {
        Assert.Equal(0.25, (double)new SDecimal(2.5, -1));
        Assert.Equal(double.PositiveInfinity, (double)SDecimal.PositiveInfinity);
        Assert.Equal(double.NegativeInfinity, (double)SDecimal.NegativeInfinity);
        Assert.Throws<OverflowException>(() => (double)new SDecimal(1000));
    }

    [Fact]
    public void SDecimal_ToFloatCast()
    {
        Assert.Equal(0.25f, (float)new SDecimal(2.5, -1));
        Assert.Equal(float.PositiveInfinity, (float)SDecimal.PositiveInfinity);
        Assert.Equal(float.NegativeInfinity, (float)SDecimal.NegativeInfinity);
        Assert.Throws<OverflowException>(() => (float)new SDecimal(1000));
    }

    [Fact]
    public void SDecimal_ToIntCast()
    {
        Assert.Equal(0, (int)new SDecimal(2.5, -1));
        Assert.Equal(0, (int)new SDecimal(7.5, -1));
        Assert.Throws<OverflowException>(() => (int)new SDecimal(1000));
    }

    [Fact]
    public void SDecimal_ToUIntCast()
    {
        Assert.Equal(0u, (uint)new SDecimal(2.5, -1));
        Assert.Equal(1u, (uint)new SDecimal(1.5, 0));
        Assert.Equal(0u, (uint)new SDecimal(-1, 0));
        Assert.Throws<OverflowException>(() => (uint)new SDecimal(1000));
    }

    [Fact]
    public void SDecimal_ToLongCast()
    {
        Assert.Equal(0, (int)new SDecimal(2.5, -1));
        Assert.Equal(0, (int)new SDecimal(7.5, -1));
        Assert.Throws<OverflowException>(() => (int)new SDecimal(1000));
    }

    [Fact]
    public void SDecimal_IsIntegerMethod()
    {
        Assert.True(SDecimal.IsInteger(1));
        Assert.True(SDecimal.IsInteger(-1));
        Assert.True(SDecimal.IsInteger(new SDecimal(1000)));
        
        Assert.False(SDecimal.IsInteger(0.1));
        Assert.False(SDecimal.IsInteger(SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_IsEvenIntegerMethod()
    {
        Assert.False(SDecimal.IsEvenInteger(1));
        Assert.True(SDecimal.IsEvenInteger(2));

        Assert.False(SDecimal.IsEvenInteger(0.1));
        Assert.False(SDecimal.IsEvenInteger(SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_IsOddIntegerMethod()
    {
        Assert.True(SDecimal.IsOddInteger(1));
        Assert.False(SDecimal.IsOddInteger(2));

        Assert.False(SDecimal.IsOddInteger(0.1));
        Assert.False(SDecimal.IsOddInteger(SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_IsInfinityMethod()
    {
        Assert.True(SDecimal.IsInfinity(SDecimal.PositiveInfinity));
        Assert.True(SDecimal.IsInfinity(SDecimal.NegativeInfinity));
        Assert.False(SDecimal.IsInfinity(1));
        Assert.False(SDecimal.IsInfinity(new SDecimal(1000)));
    }

    [Fact]
    public void SDecimal_IsPositiveInfinityMethod()
    {
        Assert.True(SDecimal.IsPositiveInfinity(SDecimal.PositiveInfinity));
        Assert.False(SDecimal.IsPositiveInfinity(SDecimal.NegativeInfinity));
        Assert.False(SDecimal.IsPositiveInfinity(1));
        Assert.False(SDecimal.IsPositiveInfinity(new SDecimal(1000)));
    }

    [Fact]
    public void SDecimal_IsNegativeInfinityMethod()
    {
        Assert.False(SDecimal.IsNegativeInfinity(SDecimal.PositiveInfinity));
        Assert.True(SDecimal.IsNegativeInfinity(SDecimal.NegativeInfinity));
        Assert.False(SDecimal.IsNegativeInfinity(-1));
        Assert.False(SDecimal.IsNegativeInfinity(new SDecimal(-1, 1000)));
    }

    [Fact]
    public void SDecimal_ToStringMethod()
    {
        SDecimal posInteger = new SDecimal(1, 0);
        SDecimal posRational = new SDecimal(1, -1);
        SDecimal posInfinity = SDecimal.PositiveInfinity;
        SDecimal largePosNumber = new SDecimal(1000);
        SDecimal smallPosNumber = new SDecimal(-1000);
        SDecimal precisePosNumber = new SDecimal(1.23456789, 2);
        
        void SDecimal_ToStringGeneralMethod()
        {
            Assert.Equal("1.0000e+0", posInteger.ToString());
            Assert.Equal("1.0000e-1", posRational.ToString());
            Assert.Equal("-1.0000e-1", (-posRational).ToString());
            Assert.Equal("PositiveInfinity", posInfinity.ToString());
            Assert.Equal("NegativeInfinity", (-posInfinity).ToString());
            Assert.Equal("1.0000e+1000", largePosNumber.ToString());
            Assert.Equal("1.0000e-1000", smallPosNumber.ToString());
            Assert.Equal("1.2346e+2", precisePosNumber.ToString());
            
            Assert.Equal("1e+2", precisePosNumber.ToString("G1"));
            Assert.Equal("1.2e+2", precisePosNumber.ToString("G2"));
            Assert.Equal("1.23456789000000e+2", precisePosNumber.ToString("G15"));
            Assert.Throws<FormatException>(() => precisePosNumber.ToString("G0"));
        }
        
        void SDecimal_ToStringNumberMethod()
        {
            Assert.Equal("1.0000", posInteger.ToString("N"));
            Assert.Equal("0.10000", posRational.ToString("N"));
            Assert.Equal("-0.10000", (-posRational).ToString("N"));
            Assert.Equal("PositiveInfinity", posInfinity.ToString("N"));
            Assert.Equal("NegativeInfinity", (-posInfinity).ToString("N"));
            Assert.Equal("1" + new string('0', 1000), largePosNumber.ToString("N"));
            Assert.Equal("0." + new string('0', 999) + "10000", smallPosNumber.ToString("N"));
            Assert.Equal("123.46", precisePosNumber.ToString("N"));
            
            Assert.Equal("100", precisePosNumber.ToString("N1"));
            Assert.Equal("120", precisePosNumber.ToString("N2"));
            Assert.Equal("123.456789000000", precisePosNumber.ToString("N15"));
            Assert.Throws<FormatException>(() => precisePosNumber.ToString("N0"));
        }
        
        SDecimal_ToStringGeneralMethod();
        SDecimal_ToStringNumberMethod();
    }

    [Fact]
    public void SDecimal_TryFormatMethod()
    {
        
    }

    [Fact]
    public void SDecimal_ParseMethod()
    {
        
    }

    [Fact]
    public void SDecimal_TryParseMethod()
    {
        
    }

    [Fact]
    public void SDecimal_CompareToMethod()
    {
        
    }

    [Fact]
    public void SDecimal_EqualsMethod()
    {
        
    }
}