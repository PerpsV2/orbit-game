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
    }

    [Fact]
    public void SDecimal_ModuloMethod_InfinityHandling()
    {
        
    }

    [Fact]
    public void SDecimal_ModuloMethod_DivideByZeroExceptions()
    {
        
    }

    [Fact]
    public void SDecimal_SquareMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IntPowMethod_PositiveExponent()
    {
        
    }

    [Fact]
    public void SDecimal_IntPowMethod_NegativeExponent()
    {
        
    }

    [Fact]
    public void SDecimal_IntPowMethod_ZeroToThePowerOfZeroException()
    {
        
    }

    [Fact]
    public void SDecimal_IntPowMethod_InfinityHandling()
    {
        
    }

    [Fact]
    public void SDecimal_SqrtMethod_PositiveRadicand()
    {
        
    }

    [Fact]
    public void SDecimal_SqrtMethod_NegativeRadicand()
    {
        
    }

    [Fact]
    public void SDecimal_SqrtMethod_InfinityHandling()
    {
        
    }

    [Fact]
    public void SDecimal_Atan2Method()
    {
        
    }

    [Fact]
    public void SDecimal_Atan2Method_QuadrantalAngles()
    {
        
    }

    [Fact]
    public void SDecimal_Atan2Method_DivideByZeroException()
    {
        
    }

    [Fact]
    public void SDecimal_Atan2Method_InfinityHandling()
    {
        
    }

    [Fact]
    public void SDecimal_CosMethod()
    {
        
    }

    [Fact]
    public void SDecimal_CosMethod_InfinityHandling()
    {
        
    }

    [Fact]
    public void SDecimal_SinMethod()
    {
        
    }

    [Fact]
    public void SDecimal_SinMethod_InfinityHandling()
    {
        
    }

    [Fact]
    public void SDecimal_TanMethod()
    {
        
    }

    [Fact]
    public void SDecimal_TanMethod_InfinityHandling()
    {
        
    }

    [Fact]
    public void SDecimal_AbsMethod()
    {
        
    }

    [Fact]
    public void SDecimal_AbsMethod_InfinityHandling()
    {
        
    }

    [Fact]
    public void SDecimal_MinMethod()
    {
        
    }

    [Fact]
    public void SDecimal_MinMethod_InfinityHandling()
    {
        
    }

    [Fact]
    public void SDecimal_MaxMethod()
    {
        
    }

    [Fact]
    public void SDecimal_MaxMethod_InfinityHandling()
    {
        
    }

    [Fact]
    public void SDecimal_RoundMethod()
    {
        
    }

    [Fact]
    public void SDecimal_RoundMethod_LargeNumberRounding()
    {
        
    }

    [Fact]
    public void SDecimal_RoundMethod_SmallNumberRounding()
    {
        
    }

    [Fact]
    public void SDecimal_RoundMethod_InfinityHandling()
    {
        
    }

    [Fact]
    public void SDecimal_FloorMethod()
    {
        
    }

    [Fact]
    public void SDecimal_FloorMethod_LargeNumberRounding()
    {
        
    }

    [Fact]
    public void SDecimal_FloorMethod_SmallNumberRounding()
    {
        
    }

    [Fact]
    public void SDecimal_FloorMethod_InfinityHandling()
    {
        
    }

    [Fact]
    public void SDecimal_CeilingMethod()
    {
        
    }

    [Fact]
    public void SDecimal_CeilingMethod_LargeNumberRounding()
    {
        
    }

    [Fact]
    public void SDecimal_CeilingMethod_SmallNumberRounding()
    {
        
    }

    [Fact]
    public void SDecimal_CeilingMethod_InfinityHandling()
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
    public void SDecimal_ClampMethod_InfinityHandling()
    {
        
    }
    
    [Fact]
    public void SDecimal_FromUIntCast()
    {
        
    }

    [Fact]
    public void SDecimal_FromIntCast()
    {
        
    }

    [Fact]
    public void SDecimal_FromLongCast()
    {
        
    }

    [Fact]
    public void SDecimal_FromFloatCast()
    {
        
    }

    [Fact]
    public void SDecimal_FromDoubleCast()
    {
        
    }

    [Fact]
    public void SDecimal_ToUIntCast()
    {
        
    }

    [Fact]
    public void SDecimal_ToIntCast()
    {
        
    }

    [Fact]
    public void SDecimal_ToLongCast()
    {
        
    }

    [Fact]
    public void SDecimal_ToFloatCast()
    {
        
    }

    [Fact]
    public void SDecimal_ToDoubleCast()
    {
        
    }

    [Fact]
    public void SDecimal_IsZeroMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IsPositiveMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IsNegativeMethod()
    {
        
    }

    [Fact]
    public void SDecimal_IsFiniteMethod()
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
    public void SDecimal_TryConvertFromChecked_Double()
    {
        
    }

    [Fact]
    public void SDecimal_TryConvertFromChecked_InvalidType()
    {
        
    }

    [Fact]
    public void SDecimal_TryConvertFromSaturating_Double()
    {
        
    }

    [Fact]
    public void SDecimal_TryConvertFromSaturating_InvalidType()
    {
        
    }
}