using System;
using System.Numerics;
using Xunit;

namespace qQEngine.Tests;

public class SDecimal_Tests
{
    [Fact]
    public void SDecimal_Constructor()
    {
        SDecimal finiteSDecimal = new SDecimal();
        Assert.Equal(0, finiteSDecimal.Mantissa);
        Assert.Equal(0, finiteSDecimal.Exponent);
        
        finiteSDecimal = new SDecimal(1, 0);
        Assert.Equal(1, finiteSDecimal.Mantissa);
        Assert.Equal(0, finiteSDecimal.Exponent);
        
        finiteSDecimal = new SDecimal(10, 0);
        Assert.Equal(1, finiteSDecimal.Mantissa);
        Assert.Equal(1, finiteSDecimal.Exponent);

        finiteSDecimal = new SDecimal(100);
        Assert.Equal(1, finiteSDecimal.Mantissa);
        Assert.Equal(100, finiteSDecimal.Exponent);

        finiteSDecimal = new SDecimal(-5, -1);
        Assert.Equal(-5, finiteSDecimal.Mantissa);
        Assert.Equal(-1, finiteSDecimal.Exponent);

        SDecimal infiniteSDecimal = SDecimal.PositiveInfinity;
        Assert.Throws<Exception>(() => infiniteSDecimal.Mantissa);
        Assert.Throws<Exception>(() => infiniteSDecimal.Exponent);
    }

    [Fact]
    public void SDecimal_AddOperator()
    {
        SDecimal leftSDecimal = new SDecimal(1);
        SDecimal rightSDecimal = new SDecimal(0);
        
        Assert.Equal(11, leftSDecimal + rightSDecimal);
    }

    [Fact]
    public void SDecimal_AddOperator_InfinityHandling()
    {
        SDecimal leftSDecimal = new SDecimal(1);
        
        Assert.Equal(SDecimal.PositiveInfinity, leftSDecimal + SDecimal.PositiveInfinity);
        Assert.Equal(SDecimal.NegativeInfinity, leftSDecimal + SDecimal.NegativeInfinity);
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.PositiveInfinity + SDecimal.PositiveInfinity);
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.NegativeInfinity + SDecimal.NegativeInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.PositiveInfinity + SDecimal.NegativeInfinity);
    }

    [Fact]
    public void SDecimal_SubtractOperator()
    {
        SDecimal leftSDecimal = new SDecimal(1);
        SDecimal rightSDecimal = new SDecimal(0);
        
        Assert.Equal(9, leftSDecimal - rightSDecimal);
    }

    [Fact]
    public void SDecimal_SubtractOperator_InfinityHandling()
    {
        SDecimal leftSDecimal = new SDecimal(1);
        
        Assert.Equal(SDecimal.NegativeInfinity, leftSDecimal - SDecimal.PositiveInfinity);
        Assert.Equal(SDecimal.PositiveInfinity, leftSDecimal - SDecimal.NegativeInfinity);
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.PositiveInfinity - SDecimal.NegativeInfinity);
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.NegativeInfinity - SDecimal.PositiveInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.PositiveInfinity - SDecimal.PositiveInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.NegativeInfinity - SDecimal.NegativeInfinity);
    }

    [Fact]
    public void SDecimal_MultiplyOperator()
    {
        SDecimal leftSDecimal = new SDecimal(1);
        SDecimal rightSDecimal = new SDecimal(5, 0);
        
        Assert.Equal(50, leftSDecimal * rightSDecimal);
        Assert.Equal(SDecimal.Zero, leftSDecimal * SDecimal.Zero);
    }

    [Fact]
    public void SDecimal_MultiplyOperator_InfinityHandling()
    {
        SDecimal leftSDecimal = new SDecimal(1);
        
        Assert.Equal(SDecimal.PositiveInfinity, leftSDecimal * SDecimal.PositiveInfinity);
        Assert.Equal(SDecimal.NegativeInfinity, leftSDecimal * SDecimal.NegativeInfinity);
        Assert.Equal(SDecimal.Zero, SDecimal.PositiveInfinity * SDecimal.Zero);
        Assert.Equal(SDecimal.Zero, SDecimal.NegativeInfinity * SDecimal.Zero);
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.PositiveInfinity * SDecimal.PositiveInfinity);
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.PositiveInfinity * SDecimal.NegativeInfinity);
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.NegativeInfinity * SDecimal.NegativeInfinity);
    }

    [Fact]
    public void SDecimal_DivideOperator()
    {
        SDecimal leftSDecimal = new SDecimal(1);
        SDecimal rightSDecimal = new SDecimal(5, 0);
        
        Assert.Equal(2, leftSDecimal / rightSDecimal);
        Assert.Equal(SDecimal.Zero, SDecimal.Zero / leftSDecimal);
    }

    [Fact]
    public void SDecimal_DivideOperator_InfinityHandling()
    {
        SDecimal leftSDecimal = new SDecimal(1);
        SDecimal rightSDecimal = new SDecimal(5, 0);
        
        Assert.Equal(SDecimal.PositiveInfinity, leftSDecimal / SDecimal.Zero);
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.PositiveInfinity / rightSDecimal);
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.NegativeInfinity / rightSDecimal);
        Assert.Equal(SDecimal.Zero, leftSDecimal / SDecimal.PositiveInfinity);
        Assert.Equal(SDecimal.Zero, leftSDecimal / SDecimal.NegativeInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.PositiveInfinity / SDecimal.PositiveInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.NegativeInfinity / SDecimal.NegativeInfinity);
    }

    [Fact]
    public void SDecimal_DivideOperator_DivideZeroByZeroException()
    {
        Assert.Throws<DivideByZeroException>(() => SDecimal.Zero / SDecimal.Zero);
    }

    [Fact]
    public void SDecimal_RemainderOperator()
    {
        SDecimal leftSDecimal = new SDecimal(3, 0);
        SDecimal rightSDecimal = new SDecimal(2, 0);
        SDecimal fractionSDecimal = new SDecimal(0.4, 0);
        
        Assert.Equal(1, leftSDecimal % rightSDecimal);
        Assert.Equal(-1, -leftSDecimal % rightSDecimal);
        Assert.Equal(1, leftSDecimal % -rightSDecimal);
        Assert.Equal(-1, -leftSDecimal % -rightSDecimal);
        Assert.Equal(0.2, leftSDecimal % fractionSDecimal);
        Assert.Equal(0.4, fractionSDecimal % rightSDecimal);
        Assert.Equal(SDecimal.Zero, SDecimal.Zero % rightSDecimal);
    }
    
    [Fact]
    public void SDecimal_RemainderOperator_LargeNumber()
    {
        SDecimal leftSDecimal = new SDecimal(20);
        SDecimal rightSDecimal = new SDecimal(43, 0);
        
        Assert.Equal(13, leftSDecimal % rightSDecimal);
    }

    [Fact]
    public void SDecimal_RemainderOperator_InfinityHandling()
    {
        SDecimal leftSDecimal = new SDecimal(3, 0);
        SDecimal rightSDecimal = new SDecimal(2, 0);
        
        Assert.Equal(leftSDecimal, leftSDecimal % SDecimal.PositiveInfinity);
        Assert.Equal(leftSDecimal, leftSDecimal % SDecimal.NegativeInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.PositiveInfinity % rightSDecimal);
        Assert.Throws<ArithmeticException>(() => SDecimal.NegativeInfinity % rightSDecimal);
        Assert.Throws<ArithmeticException>(() => SDecimal.PositiveInfinity % SDecimal.PositiveInfinity);
    }

    [Fact]
    public void SDecimal_RemainderOperator_DivideByZeroExceptions()
    {
        SDecimal leftSDecimal = new SDecimal(3, 0);
        
        Assert.Throws<DivideByZeroException>(() => leftSDecimal % SDecimal.Zero);
        Assert.Throws<DivideByZeroException>(() => SDecimal.Zero % SDecimal.Zero);
    }

    [Fact]
    public void SDecimal_ModuloMethod()
    {
        SDecimal leftSDecimal = new SDecimal(3, 0);
        SDecimal rightSDecimal = new SDecimal(2, 0);
        SDecimal fractionSDecimal = new SDecimal(0.4, 0);
        
        Assert.Equal(1, SDecimal.Mod(leftSDecimal, rightSDecimal));
        Assert.Equal(1, SDecimal.Mod(-leftSDecimal, rightSDecimal));
        Assert.Equal(-1, SDecimal.Mod(leftSDecimal, -rightSDecimal));
        Assert.Equal(-1, SDecimal.Mod(-leftSDecimal, -rightSDecimal));
        Assert.Equal(0.2, SDecimal.Mod(leftSDecimal, fractionSDecimal));
        Assert.Equal(0.4, SDecimal.Mod(fractionSDecimal, rightSDecimal));
    }

    [Fact]
    public void SDecimal_ModuloMethod_LargeNumber()
    {
        SDecimal leftSDecimal = new SDecimal(20);
        SDecimal rightSDecimal = new SDecimal(43, 0);
        
        Assert.Equal(13, SDecimal.Mod(leftSDecimal, rightSDecimal));
    }

    [Fact]
    public void SDecimal_ModuloMethod_InfinityHandling()
    {
        SDecimal leftSDecimal = new SDecimal(3, 0);
        SDecimal rightSDecimal = new SDecimal(2, 0);
        
        Assert.Equal(leftSDecimal, SDecimal.Mod(leftSDecimal, SDecimal.PositiveInfinity));
        Assert.Equal(leftSDecimal, SDecimal.Mod(leftSDecimal, SDecimal.NegativeInfinity));
        Assert.Throws<ArithmeticException>(() => SDecimal.Mod(SDecimal.PositiveInfinity, rightSDecimal));
        Assert.Throws<ArithmeticException>(() => SDecimal.Mod(SDecimal.NegativeInfinity, rightSDecimal));
        Assert.Throws<ArithmeticException>(() => SDecimal.Mod(SDecimal.PositiveInfinity, SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_ModuloMethod_DivideByZeroExceptions()
    {
        SDecimal leftSDecimal = new SDecimal(3, 0);

        Assert.Throws<DivideByZeroException>(() => SDecimal.Mod(leftSDecimal, SDecimal.Zero));
        Assert.Throws<DivideByZeroException>(() => SDecimal.Mod(SDecimal.Zero, SDecimal.Zero));
    }

    [Fact]
    public void SDecimal_SquareMethod()
    {
        SDecimal sDecimal = new SDecimal(2, 0);
        SDecimal smallSDecimal = new SDecimal(-10);
        SDecimal largeSDecimal = new SDecimal(10);
        
        Assert.Equal(4, SDecimal.Square(sDecimal));
        Assert.Equal(new SDecimal(-20), SDecimal.Square(smallSDecimal));
        Assert.Equal(new SDecimal(20), SDecimal.Square(largeSDecimal));
    }

    [Fact]
    public void SDecimal_IntPowMethod_PositiveExponent()
    {
        SDecimal positiveSDecimal = new SDecimal(2, 0);
        SDecimal negativeSDecimal = new SDecimal(-2, 0);
        
        Assert.Equal(1, SDecimal.IntPow(positiveSDecimal, 0));
        Assert.Equal(2, SDecimal.IntPow(positiveSDecimal, 1));
        Assert.Equal(4, SDecimal.IntPow(positiveSDecimal, 2));
        Assert.Equal(8, SDecimal.IntPow(positiveSDecimal, 3));
        Assert.Equal(4, SDecimal.IntPow(negativeSDecimal, 2));
        Assert.Equal(-8, SDecimal.IntPow(negativeSDecimal, 3));
        Assert.Equal(0, SDecimal.IntPow(SDecimal.Zero, 10));
    }

    [Fact]
    public void SDecimal_IntPowMethod_NegativeExponent()
    {
        SDecimal positiveSDecimal = new SDecimal(2, 0);
        SDecimal negativeSDecimal = new SDecimal(-2, 0);

        Assert.Equal(0.5, SDecimal.IntPow(positiveSDecimal, -1));
        Assert.Equal(0.25, SDecimal.IntPow(positiveSDecimal, -2));
        Assert.Equal(-0.5, SDecimal.IntPow(negativeSDecimal, -1));
        Assert.Equal(0.25, SDecimal.IntPow(negativeSDecimal, -2));
        Assert.Equal(0, SDecimal.IntPow(SDecimal.Zero, -10));
    }

    [Fact]
    public void SDecimal_IntPowMethod_ZeroToThePowerOfZeroException()
    {
        Assert.Throws<ArithmeticException>(() => SDecimal.IntPow(SDecimal.Zero, 0));
    }
    
    [Fact]
    public void SDecimal_IntPowMethod_InfinityHandling()
    {
        Assert.Equal(1, SDecimal.IntPow(SDecimal.PositiveInfinity, 0));
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.IntPow(SDecimal.NegativeInfinity, 3));
        Assert.Equal(0, SDecimal.IntPow(SDecimal.PositiveInfinity, -1));
        Assert.Equal(0, SDecimal.IntPow(SDecimal.NegativeInfinity, -1));
    }

    [Fact]
    public void SDecimal_SqrtMethod_PositiveRadicand()
    {
        Assert.Equal(2, SDecimal.Sqrt(4));
        Assert.Equal(1, SDecimal.Sqrt(1));
        Assert.Equal(0, SDecimal.Sqrt(0));
        Assert.Equal(Math.Sqrt(2), SDecimal.Sqrt(2));
    }

    [Fact]
    public void SDecimal_SqrtMethod_NegativeRadicand()
    {
        Assert.Throws<ArithmeticException>(() => SDecimal.Sqrt(-1));
    }

    [Fact]
    public void SDecimal_SqrtMethod_InfinityHandling()
    {
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.Sqrt(SDecimal.PositiveInfinity));
        Assert.Throws<ArithmeticException>(() => SDecimal.Sqrt(SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_Atan2Method()
    {
        Assert.Equal(Math.PI / 4, SDecimal.Atan2(1, 1));
        Assert.Equal(3 * Math.PI / 4, SDecimal.Atan2(1, -1));
        Assert.Equal(-Math.PI / 4, SDecimal.Atan2(-1, 1));
        Assert.Equal(-3 * Math.PI / 4, SDecimal.Atan2(-1, -1));
    }

    [Fact]
    public void SDecimal_Atan2Method_QuadrantalAngles()
    {
        Assert.Equal(0, SDecimal.Atan2(0, 1));
        Assert.Equal(Math.PI, SDecimal.Atan2(0, -1));
        Assert.Equal(Math.PI / 2, SDecimal.Atan2(1, 0));
        Assert.Equal(-Math.PI / 2, SDecimal.Atan2(-1, 0));
    }

    [Fact]
    public void SDecimal_Atan2Method_DivideByZeroException()
    {
        Assert.Throws<DivideByZeroException>(() => SDecimal.Atan2(SDecimal.Zero, SDecimal.Zero));
    }

    [Fact]
    public void SDecimal_Atan2Method_InfinityHandling()
    {
        Assert.Equal(0, SDecimal.Atan2(new SDecimal(100), SDecimal.PositiveInfinity));
        Assert.Equal(Math.PI / 2, SDecimal.Atan2(SDecimal.PositiveInfinity, new SDecimal(100)));
        Assert.Throws<ArithmeticException>(() => SDecimal.Atan2(SDecimal.PositiveInfinity, SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_CosMethod()
    {
        Assert.Equal(1, SDecimal.Cos(0));
        Assert.Equal(-1, SDecimal.Cos(Math.PI));
        Assert.Equal(1, SDecimal.Cos(Math.Tau));
    }

    [Fact]
    public void SDecimal_CosMethod_LargeNumber()
    {
        Assert.Equal(1, SDecimal.Cos(Math.Tau * new SDecimal(20)));
    }

    [Fact]
    public void SDecimal_CosMethod_InfinityHandling()
    {
        Assert.Throws<ArithmeticException>(() => SDecimal.Cos(SDecimal.PositiveInfinity));
        Assert.Throws<ArithmeticException>(() => SDecimal.Cos(SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_SinMethod()
    {
        Assert.Equal(0, SDecimal.Sin(0));
        Assert.Equal(1, SDecimal.Sin(Math.PI / 2));
        Assert.Equal(-1, SDecimal.Sin(3 * Math.PI / 2));
        Assert.Equal(0, SDecimal.Sin(Math.Tau));
    }

    [Fact]
    public void SDecimal_SinMethod_LargeNumber()
    {
        Assert.Equal(0, SDecimal.Sin(Math.Tau * new SDecimal(20)));
    }

    [Fact]
    public void SDecimal_SinMethod_InfinityHandling()
    {
        Assert.Throws<ArithmeticException>(() => SDecimal.Sin(SDecimal.PositiveInfinity));
        Assert.Throws<ArithmeticException>(() => SDecimal.Sin(SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_TanMethod()
    {
        Assert.Equal(0, SDecimal.Tan(0));
        Assert.Throws<ArithmeticException>(() => SDecimal.Tan(Math.PI / 2));
        Assert.Equal(0, SDecimal.Tan(Math.PI));
    }

    [Fact]
    public void SDecimal_TanMethod_LargeNumber()
    {
        Assert.Equal(0, Math.PI * new SDecimal(20));
    }

    [Fact]
    public void SDecimal_TanMethod_InfinityHandling()
    {
        Assert.Throws<ArithmeticException>(() => SDecimal.Tan(SDecimal.PositiveInfinity));
        Assert.Throws<ArithmeticException>(() => SDecimal.Tan(SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_AbsMethod()
    {
        SDecimal positiveSDecimal = new SDecimal(5, 0);
        SDecimal negativeSDecimal = new SDecimal(-5, 0);
        
        Assert.Equal(SDecimal.Zero, SDecimal.Abs(SDecimal.Zero));
        Assert.Equal(5, SDecimal.Abs(positiveSDecimal));
        Assert.Equal(5, SDecimal.Abs(negativeSDecimal));
    }

    [Fact]
    public void SDecimal_AbsMethod_InfinityHandling()
    {
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.Abs(SDecimal.PositiveInfinity));
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.Abs(SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_MinMethod()
    {
        SDecimal leftSDecimal = new SDecimal(5, 0);
        SDecimal rightSDecimal = new SDecimal(-5, 0);
        
        Assert.Equal(-5, SDecimal.Min(leftSDecimal, rightSDecimal));
        Assert.Equal(-5, SDecimal.Min(rightSDecimal, leftSDecimal));
    }

    [Fact]
    public void SDecimal_MinMethod_InfinityHandling()
    {
        SDecimal finiteSDecimal = new SDecimal(-5, 0);
        
        Assert.Equal(finiteSDecimal, SDecimal.Min(finiteSDecimal, SDecimal.PositiveInfinity));
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.Min(finiteSDecimal, SDecimal.NegativeInfinity));
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.Min(SDecimal.PositiveInfinity, SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_MaxMethod()
    {
        SDecimal leftSDecimal = new SDecimal(5, 0);
        SDecimal rightSDecimal = new SDecimal(-5, 0);
        
        Assert.Equal(5, SDecimal.Max(leftSDecimal, rightSDecimal));
        Assert.Equal(5, SDecimal.Max(rightSDecimal, leftSDecimal));
    }

    [Fact]
    public void SDecimal_MaxMethod_InfinityHandling()
    {
        SDecimal finiteSDecimal = new SDecimal(5, 0);
        
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.Max(finiteSDecimal, SDecimal.PositiveInfinity));
        Assert.Equal(finiteSDecimal, SDecimal.Max(finiteSDecimal, SDecimal.NegativeInfinity));
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.Max(SDecimal.PositiveInfinity, SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_RoundMethod()
    {
        SDecimal fractionSDecimal = new SDecimal(0.1, 0);
        Assert.Equal(0, SDecimal.Round(fractionSDecimal));
        
        fractionSDecimal = new SDecimal(0.9, 0);
        Assert.Equal(1, SDecimal.Round(fractionSDecimal));
        
        fractionSDecimal = new SDecimal(1.1, 0);
        Assert.Equal(1, SDecimal.Round(fractionSDecimal));
    }
    
    [Fact]
    public void SDecimal_RoundMethod_MidPointRounding() 
    {
        SDecimal midPointSDecimal = new SDecimal(0.5, 0);
        
        Assert.Equal(0, SDecimal.Round(midPointSDecimal, MidpointRounding.ToEven));
        Assert.Equal(0, SDecimal.Round(midPointSDecimal, MidpointRounding.ToZero));
        Assert.Equal(1, SDecimal.Round(midPointSDecimal, MidpointRounding.AwayFromZero));
        Assert.Equal(1, SDecimal.Round(midPointSDecimal, MidpointRounding.ToPositiveInfinity));
        Assert.Equal(0, SDecimal.Round(midPointSDecimal, MidpointRounding.ToNegativeInfinity));
        
        midPointSDecimal = new SDecimal(1.5, 0);
        
        Assert.Equal(2, SDecimal.Round(midPointSDecimal, MidpointRounding.ToEven));
        Assert.Equal(1, SDecimal.Round(midPointSDecimal, MidpointRounding.ToZero));
        Assert.Equal(2, SDecimal.Round(midPointSDecimal, MidpointRounding.AwayFromZero));
        Assert.Equal(2, SDecimal.Round(midPointSDecimal, MidpointRounding.ToPositiveInfinity));
        Assert.Equal(1, SDecimal.Round(midPointSDecimal, MidpointRounding.ToNegativeInfinity));
        
        midPointSDecimal = new SDecimal(-0.5, 0);
                
        Assert.Equal(0, SDecimal.Round(midPointSDecimal, MidpointRounding.ToEven));
        Assert.Equal(0, SDecimal.Round(midPointSDecimal, MidpointRounding.ToZero));
        Assert.Equal(-1, SDecimal.Round(midPointSDecimal, MidpointRounding.AwayFromZero));
        Assert.Equal(0, SDecimal.Round(midPointSDecimal, MidpointRounding.ToPositiveInfinity));
        Assert.Equal(-1, SDecimal.Round(midPointSDecimal, MidpointRounding.ToNegativeInfinity));
    }

    [Fact]
    public void SDecimal_RoundMethod_LargeNumberRounding()
    {
        SDecimal roundDownSDecimal = new SDecimal(20) + new SDecimal(0.1, 0);
        SDecimal roundUpSDecimal = new SDecimal(20) + new SDecimal(0.9, 0);

        Assert.Equal(new SDecimal(20), SDecimal.Round(roundDownSDecimal));
        Assert.Equal(new SDecimal(20) + 1, SDecimal.Round(roundUpSDecimal));
    }

    [Fact]
    public void SDecimal_RoundMethod_InfinityHandling()
    {
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.Round(SDecimal.NegativeInfinity));
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.Round(SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_FloorMethod()
    {
        SDecimal fractionSDecimal = new SDecimal(0.1, 0);
        Assert.Equal(0, SDecimal.Floor(fractionSDecimal));
        
        fractionSDecimal = new SDecimal(0.9, 0);
        Assert.Equal(0, SDecimal.Floor(fractionSDecimal));
        
        fractionSDecimal = new SDecimal(1.1, 0);
        Assert.Equal(1, SDecimal.Floor(fractionSDecimal));
        
        fractionSDecimal = new SDecimal(-0.1, 0);
        Assert.Equal(-1, SDecimal.Floor(fractionSDecimal));
    }

    [Fact]
    public void SDecimal_FloorMethod_LargeNumberRounding()
    {
        SDecimal sDecimal = new SDecimal(20) + new SDecimal(0.9, 0);
        Assert.Equal(new SDecimal(20), SDecimal.Floor(sDecimal));
    }

    [Fact]
    public void SDecimal_FloorMethod_SmallNumberRounding()
    {
        SDecimal sDecimal = new SDecimal(-1, -20);
        Assert.Equal(-1, SDecimal.Floor(sDecimal));
    }

    [Fact]
    public void SDecimal_FloorMethod_InfinityHandling()
    {
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.Floor(SDecimal.NegativeInfinity));
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.Floor(SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_CeilingMethod()
    {
        SDecimal fractionSDecimal = new SDecimal(0.1, 0);
        Assert.Equal(1, SDecimal.Ceiling(fractionSDecimal));
        
        fractionSDecimal = new SDecimal(0.9, 0);
        Assert.Equal(1, SDecimal.Ceiling(fractionSDecimal));
        
        fractionSDecimal = new SDecimal(1.1, 0);
        Assert.Equal(2, SDecimal.Ceiling(fractionSDecimal));
        
        fractionSDecimal = new SDecimal(-0.9, 0);
        Assert.Equal(0, SDecimal.Ceiling(fractionSDecimal));
    }

    [Fact]
    public void SDecimal_CeilingMethod_LargeNumberRounding()
    {
        SDecimal sDecimal = new SDecimal(20) + new SDecimal(0.1, 0);
        Assert.Equal(new SDecimal(20), SDecimal.Ceiling(sDecimal));
    }

    [Fact]
    public void SDecimal_CeilingMethod_SmallNumberRounding()
    {
        SDecimal sDecimal = new SDecimal(1, -20);
        Assert.Equal(1, SDecimal.Ceiling(sDecimal));
    }

    [Fact]
    public void SDecimal_CeilingMethod_InfinityHandling()
    {
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.Ceiling(SDecimal.NegativeInfinity));
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.Ceiling(SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_MinMagnitudeNumberMethod()
    {
        SDecimal leftSDecimal = new SDecimal(-5, 0);
        SDecimal rightSDecimal = new SDecimal(5, 0);
        
        Assert.Equal(leftSDecimal, SDecimal.MinMagnitudeNumber(leftSDecimal, rightSDecimal));
        Assert.Equal(leftSDecimal, SDecimal.MinMagnitudeNumber(leftSDecimal, SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_MaxMagnitudeNumberMethod()
    {
        SDecimal leftSDecimal = new SDecimal(-5, 0);
        SDecimal rightSDecimal = new SDecimal(5, 0);
        
        Assert.Equal(rightSDecimal, SDecimal.MaxMagnitudeNumber(leftSDecimal, rightSDecimal));
        Assert.Equal(leftSDecimal, SDecimal.MaxMagnitudeNumber(leftSDecimal, SDecimal.PositiveInfinity));
    }

    [Fact]
    public void SDecimal_ClampMethod()
    {
        SDecimal lowerLimitSDecimal = new SDecimal(-1, 0);
        SDecimal upperLimitSDecimal = new SDecimal(1, 0);
        SDecimal sDecimal = new SDecimal(5, 0);
        
        Assert.Equal(1, SDecimal.Clamp(sDecimal, lowerLimitSDecimal, upperLimitSDecimal));
        Assert.Equal(0, SDecimal.Clamp(SDecimal.Zero, lowerLimitSDecimal, upperLimitSDecimal));
    }

    [Fact]
    public void SDecimal_ClampMethod_InfinityHandling()
    {
        SDecimal lowerLimitSDecimal = new SDecimal(-1, 0);
        SDecimal upperLimitSDecimal = new SDecimal(1, 0);
        
        Assert.Equal(1, SDecimal.Clamp(SDecimal.PositiveInfinity, lowerLimitSDecimal, upperLimitSDecimal));
        Assert.Equal(-1, SDecimal.Clamp(SDecimal.NegativeInfinity, lowerLimitSDecimal, upperLimitSDecimal));
    }

    [Fact]
    public void SDecimal_CLampMethod_LimitOutOfRangeException()
    {
        SDecimal lowerLimitSDecimal = new SDecimal(-1, 0);
        SDecimal upperLimitSDecimal = new SDecimal(1, 0);
        
        Assert.Throws<ArgumentOutOfRangeException>(() => SDecimal.Clamp(SDecimal.Zero, upperLimitSDecimal, lowerLimitSDecimal));
        Assert.Throws<ArgumentOutOfRangeException>(() => SDecimal.Clamp(SDecimal.Zero, SDecimal.PositiveInfinity, SDecimal.PositiveInfinity));
    }
    
    [Fact]
    public void SDecimal_FromUIntCast()
    {
        Assert.Equal(SDecimal.Zero, 0u);
        Assert.Equal(new SDecimal(1, 0), 1u);
    }

    [Fact]
    public void SDecimal_FromIntCast()
    {
        Assert.Equal(SDecimal.Zero, 0);
        Assert.Equal(new SDecimal(1, 0), 1);
        Assert.Equal(new SDecimal(-1, 0), -1);
    }

    [Fact]
    public void SDecimal_FromLongCast()
    {
        Assert.Equal(SDecimal.Zero, 0L);
        Assert.Equal(new SDecimal(1, 0), 1L);
        Assert.Equal(new SDecimal(18), 1_000_000_000_000_000_000);
    }

    [Fact]
    public void SDecimal_FromFloatCast()
    {
        Assert.Equal(SDecimal.Zero, 0f);
        Assert.Equal(new SDecimal(1, 0), 1f);
        Assert.Equal(SDecimal.NegativeInfinity, float.NegativeInfinity);
        Assert.Equal(SDecimal.PositiveInfinity, float.PositiveInfinity);
        Assert.Throws<ArgumentException>(() => (SDecimal)float.NaN);
    }

    [Fact]
    public void SDecimal_FromDoubleCast()
    {
        Assert.Equal(SDecimal.Zero, 0d);
        Assert.Equal(new SDecimal(1, 0), 1d);
        Assert.Equal(SDecimal.NegativeInfinity, double.NegativeInfinity);
        Assert.Equal(SDecimal.PositiveInfinity, double.PositiveInfinity);
        Assert.Throws<ArgumentException>(() => (SDecimal)double.NaN);
    }

    [Fact]
    public void SDecimal_ToUIntCast()
    {
        SDecimal positiveSDecimal = new SDecimal(1, 0);
        SDecimal negativeSDecimal = new SDecimal(-1, 0);
        SDecimal largeSDecimal = new SDecimal(50);
        SDecimal positiveFractionalSDecimal = new SDecimal(0.9, 0);
        SDecimal negativeFractionalSDecimal = new SDecimal(-0.9, 0);
        
        Assert.Equal(0u, SDecimal.Zero);
        Assert.Equal(1u, positiveSDecimal);
        Assert.Equal(0u, positiveFractionalSDecimal);
        Assert.Throws<ArgumentOutOfRangeException>(() => (uint)negativeSDecimal);
        Assert.Throws<ArgumentOutOfRangeException>(() => (uint)negativeFractionalSDecimal);
        Assert.Throws<ArgumentOutOfRangeException>(() => (uint)largeSDecimal);
        Assert.Throws<ArgumentException>(() => (uint)SDecimal.PositiveInfinity);
        Assert.Throws<ArgumentException>(() => (uint)SDecimal.NegativeInfinity);
    }

    [Fact]
    public void SDecimal_ToIntCast()
    {
        SDecimal positiveSDecimal = new SDecimal(1, 0);
        SDecimal negativeSDecimal = new SDecimal(-1, 0);
        SDecimal largeSDecimal = new SDecimal(50);
        SDecimal positiveFractionalSDecimal = new SDecimal(0.9, 0);
        SDecimal negativeFractionalSDecimal = new SDecimal(-0.9, 0);
        
        Assert.Equal(0, SDecimal.Zero);
        Assert.Equal(1, positiveSDecimal);
        Assert.Equal(0, positiveFractionalSDecimal);
        Assert.Equal(-1, negativeSDecimal);
        Assert.Equal(0, negativeFractionalSDecimal);
        Assert.Throws<ArgumentOutOfRangeException>(() => (int)largeSDecimal);
        Assert.Throws<ArgumentException>(() => (int)SDecimal.PositiveInfinity);
        Assert.Throws<ArgumentException>(() => (int)SDecimal.NegativeInfinity);
    }

    [Fact]
    public void SDecimal_ToLongCast()
    {
        SDecimal positiveSDecimal = new SDecimal(1, 0);
        SDecimal negativeSDecimal = new SDecimal(-1, 0);
        SDecimal largeSDecimal = new SDecimal(50);
        SDecimal positiveFractionalSDecimal = new SDecimal(0.9, 0);
        SDecimal negativeFractionalSDecimal = new SDecimal(-0.9, 0);
        
        Assert.Equal(0L, SDecimal.Zero);
        Assert.Equal(1L, positiveSDecimal);
        Assert.Equal(0L, positiveFractionalSDecimal);
        Assert.Equal(-1L, negativeSDecimal);
        Assert.Equal(0L, negativeFractionalSDecimal);
        Assert.Throws<ArgumentOutOfRangeException>(() => (long)largeSDecimal);
        Assert.Throws<ArgumentException>(() => (long)SDecimal.PositiveInfinity);
        Assert.Throws<ArgumentException>(() => (long)SDecimal.NegativeInfinity);
    }

    [Fact]
    public void SDecimal_ToFloatCast()
    {
        SDecimal positiveSDecimal = new SDecimal(1, 0);
        SDecimal negativeSDecimal = new SDecimal(-1, 0);
        SDecimal largeSDecimal = new SDecimal(500);
        SDecimal positiveFractionalSDecimal = new SDecimal(0.9, 0);
        SDecimal negativeFractionalSDecimal = new SDecimal(-0.9, 0);
        
        Assert.Equal(0f, SDecimal.Zero);
        Assert.Equal(1f, positiveSDecimal);
        Assert.Equal(0.9f, positiveFractionalSDecimal);
        Assert.Equal(-1f, negativeSDecimal);
        Assert.Equal(-0.9f, negativeFractionalSDecimal);
        Assert.Equal(float.PositiveInfinity, SDecimal.PositiveInfinity);
        Assert.Equal(float.NegativeInfinity, SDecimal.NegativeInfinity);
        Assert.Throws<ArgumentOutOfRangeException>(() => (float)largeSDecimal);
    }

    [Fact]
    public void SDecimal_ToDoubleCast()
    {
        SDecimal positiveSDecimal = new SDecimal(1, 0);
        SDecimal negativeSDecimal = new SDecimal(-1, 0);
        SDecimal largeSDecimal = new SDecimal(500);
        SDecimal positiveFractionalSDecimal = new SDecimal(0.9, 0);
        SDecimal negativeFractionalSDecimal = new SDecimal(-0.9, 0);
        
        Assert.Equal(0d, SDecimal.Zero);
        Assert.Equal(1d, positiveSDecimal);
        Assert.Equal(0.9d, positiveFractionalSDecimal);
        Assert.Equal(-1d, negativeSDecimal);
        Assert.Equal(-0.9d, negativeFractionalSDecimal);
        Assert.Equal(double.NegativeInfinity, SDecimal.NegativeInfinity);
        Assert.Equal(double.PositiveInfinity, SDecimal.PositiveInfinity);
        Assert.Throws<ArgumentOutOfRangeException>(() => (double)largeSDecimal);
    }

    [Fact]
    public void SDecimal_IsPositiveMethod()
    {
        Assert.True(SDecimal.IsPositive(new SDecimal(1, 0)));
        Assert.True(SDecimal.IsPositive(SDecimal.Zero));
        Assert.False(SDecimal.IsPositive(new SDecimal(-1, 0)));
        Assert.True(SDecimal.IsPositive(SDecimal.PositiveInfinity));
        Assert.False(SDecimal.IsPositive(SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_IsNegativeMethod()
    {
        Assert.False(SDecimal.IsNegative(new SDecimal(1, 0)));
        Assert.False(SDecimal.IsNegative(SDecimal.Zero));
        Assert.True(SDecimal.IsNegative(new SDecimal(-1, 0)));
        Assert.False(SDecimal.IsNegative(SDecimal.PositiveInfinity));
        Assert.True(SDecimal.IsNegative(SDecimal.NegativeInfinity));
    }

    [Fact]
    public void SDecimal_IsRealNumber()
    {
        
    }

    [Fact]
    public void SDecimal_IsImaginaryNumber()
    {
        
    }

    [Fact]
    public void SDecimal_IsComplexNumber()
    {
        
    }

    [Fact]
    public void SDecimal_IsIntegerMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IsEvenIntegerMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IsOddIntegerMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IsInfinityMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IsPositiveInfinityMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IsNegativeInfinityMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IsNaNMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IsCanonicalMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IsNormalMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IsSubnormalMethod()
    {
        
    }

    [Fact]
    public void SDecimal_TryConvertFromCheckedMethod()
    {
        
    }

    [Fact]
    public void SDecimal_TryConvertFromSaturatingMethod()
    {
        
    }

    [Fact]
    public void SDecimal_TryConvertFromTruncatingMethod()
    {
        
    }

    [Fact]
    public void SDecimal_TryConvertToCheckedMethod()
    {
        
    }

    [Fact]
    public void SDecimal_TryConvertToSaturatingMethod()
    {
        
    }

    [Fact]
    public void SDecimal_TryConvertToTruncatingMethod()
    {
        
    }
}