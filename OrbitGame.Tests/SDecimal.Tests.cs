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
        
        Assert.Equal(true, positiveInfiniteSDecimal.Positive);
        Assert.Equal(false, positiveInfiniteSDecimal.Negative);

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
        Assert.Throws<ArithmeticException>(() => SDecimal.FromDouble(double.NaN));
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
        
        Assert.Equal(100000, SDecimal.ToDouble(posSDecimal));
        Assert.Equal(-0.00001, SDecimal.ToDouble(negSDecimal));
        Assert.Equal(double.PositiveInfinity, SDecimal.ToDouble(SDecimal.PositiveInfinity));
        Assert.Equal(double.NegativeInfinity, SDecimal.ToDouble(SDecimal.NegativeInfinity));
        Assert.Throws<OverflowException>(() => SDecimal.ToDouble(new SDecimal(1000)));
        Assert.Equal(0, SDecimal.ToDouble(new SDecimal(-1000)));
    }

    [Fact]
    public void SDecimal_AddOperator()
    {
        Assert.Equal(7, new SDecimal(3, 0) + new SDecimal(4, 0));
        Assert.Equal(-1, new SDecimal(3, 0) + new SDecimal(-4, 0));
        Assert.Equal(100001, new SDecimal(5) + new SDecimal(0));
        Assert.Equal(SDecimal.PositiveInfinity, new SDecimal(5, 0) + SDecimal.PositiveInfinity);
        Assert.Equal(SDecimal.NegativeInfinity, new SDecimal(5, 0) + SDecimal.NegativeInfinity);
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.PositiveInfinity + SDecimal.PositiveInfinity);
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.NegativeInfinity + SDecimal.NegativeInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.PositiveInfinity + SDecimal.NegativeInfinity);
    }

    [Fact]
    public void SDecimal_SubtractOperator()
    {
        Assert.Equal(-1, new SDecimal(3, 0) - new SDecimal(4, 0));
        Assert.Equal(7, new SDecimal(3, 0) - new SDecimal(-4, 0));
        Assert.Equal(99999, new SDecimal(5) - new SDecimal(0));
        Assert.Equal(SDecimal.NegativeInfinity, new SDecimal(5, 0) - SDecimal.PositiveInfinity);
        Assert.Equal(SDecimal.PositiveInfinity, new SDecimal(5, 0) - SDecimal.NegativeInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.PositiveInfinity - SDecimal.PositiveInfinity);
    }

    [Fact]
    public void SDecimal_MultiplyOperator()
    {
        Assert.Equal(12, new SDecimal(3, 0) * new SDecimal(4, 0));
        Assert.Equal(-12, new SDecimal(3, 0) * new SDecimal(-4, 0));
        Assert.Equal(new SDecimal(2, 1000), new SDecimal(1000) * new SDecimal(2, 0));
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.NegativeInfinity * SDecimal.NegativeInfinity);
        Assert.Equal(0, SDecimal.PositiveInfinity * 0);
    }

    [Fact]
    public void SDecimal_DivideOperator()
    {
        Assert.Equal(3, new SDecimal(12, 0) / new SDecimal(4, 0));
        Assert.Equal(-4, new SDecimal(12, 0) / new SDecimal(-3, 0));
        Assert.Equal(new SDecimal(5, 999), new SDecimal(1000) / new SDecimal(2, 0));
        Assert.Equal(SDecimal.PositiveInfinity, new SDecimal(1, 0) / new SDecimal());
        Assert.Equal(SDecimal.NegativeInfinity, new SDecimal(-1, 0) / new SDecimal());
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.PositiveInfinity / new SDecimal(1000));
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.PositiveInfinity / new SDecimal(-1, 0));
        Assert.Equal(0, new SDecimal(1000) / SDecimal.PositiveInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.PositiveInfinity / SDecimal.PositiveInfinity);
        Assert.Throws<ArithmeticException>(() => new SDecimal() / new SDecimal());
    }

    [Fact]
    public void SDecimal_ModuloOperator()
    {
        
    }

    [Fact]
    public void SDecimal_SquareMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IntPowMethod()
    {
        
    }

    [Fact]
    public void SDecimal_SqrtMethod()
    {
        
    }

    [Fact]
    public void SDecimal_Atan2Method()
    {
        
    }

    [Fact]
    public void SDecimal_CosMethod()
    {
        
    }

    [Fact]
    public void SDecimal_SinMethod()
    {
        
    }

    [Fact]
    public void SDecimal_TanMethod()
    {
        
    }

    [Fact]
    public void SDecimal_AbsMethod()
    {
        
    }

    [Fact]
    public void SDecimal_MinMethod()
    {
        
    }

    [Fact]
    public void SDecimal_MaxMethod()
    {
        
    }

    [Fact]
    public void SDecimal_RoundMethod()
    {
        
    }

    [Fact]
    public void SDecimal_FloorMethod()
    {
        
    }

    [Fact]
    public void SDecimal_CeilingMethod()
    {
        
    }

    [Fact]
    public void SDecimal_MinMagnitudeNumberMethod()
    {
        
    }

    [Fact]
    public void SDecimal_MaxMagnitudeNumberMethod()
    {
        
    }

    [Fact]
    public void SDecimal_ClampMethod()
    {
        
    }

    [Fact]
    public void SDecimal_MapMethod()
    {
        
    }

    [Fact]
    public void SDecimal_EqualityOperator()
    {
        
    }

    [Fact]
    public void SDecimal_InequalityOperator()
    {
        
    }

    [Fact]
    public void SDecimal_GreaterThanOperator()
    {
        
    }

    [Fact]
    public void SDecimal_GreaterThanOrEqualToOperator()
    {
        
    }

    [Fact]
    public void SDecimal_LessThanOperator()
    {
        
    }

    [Fact]
    public void SDecimal_LessThanOrEqualToOperator()
    {
        
    }

    [Fact]
    public void SDecimal_FromIntCast()
    {
        
    }

    [Fact]
    public void SDecimal_FromDoubleCast()
    {
        
    }

    [Fact]
    public void SDecimal_FromFloatCast()
    {
        
    }

    [Fact]
    public void SDecimal_ToDoubleCast()
    {
        
    }

    [Fact]
    public void SDecimal_ToFloatCast()
    {
        
    }

    [Fact]
    public void SDecimal_ToIntCast()
    {
        
    }

    [Fact]
    public void SDecimal_ToUIntCast()
    {
        
    }

    [Fact]
    public void SDecimal_ToLongCast()
    {
        
    }

    [Fact]
    public void SDecimal_ToPDecimalCast()
    {
        
    }

    [Fact]
    public void SDecimal_IsZero()
    {
        
    }

    [Fact]
    public void SDecimal_IsFinite()
    {
        
    }

    [Fact]
    public void SDecimal_IsRealNumber()
    {
        
    }

    [Fact]
    public void SDecimal_IsImaginaryNumber()
    {
        
    }
}