using System;
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
        Assert.Equal(SDecimal.PositiveInfinity, leftSDecimal / SDecimal.Zero);
        Assert.Equal(SDecimal.Zero, SDecimal.Zero / leftSDecimal);
        Assert.Equal(SDecimal.PositiveInfinity, SDecimal.PositiveInfinity / rightSDecimal);
        Assert.Equal(SDecimal.NegativeInfinity, SDecimal.NegativeInfinity / rightSDecimal);
        Assert.Equal(SDecimal.Zero, leftSDecimal / SDecimal.PositiveInfinity);
        Assert.Equal(SDecimal.Zero, leftSDecimal / SDecimal.NegativeInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.PositiveInfinity / SDecimal.PositiveInfinity);
        Assert.Throws<ArithmeticException>(() => SDecimal.NegativeInfinity / SDecimal.NegativeInfinity);
        Assert.Throws<DivideByZeroException>(() => SDecimal.Zero / SDecimal.Zero);
    }

    [Fact]
    public void SDecimal_ModuloOperator()
    {
        
    }
}