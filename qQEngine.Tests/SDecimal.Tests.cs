namespace qQEngine.Tests;

public class SDecimal_Tests
{
    [Fact]
    public void SDecimal_Constructor()
    {
        SDecimal sDecimal = new SDecimal();
        Assert.Equal(0, sDecimal.Mantissa);
        Assert.Equal(0, sDecimal.Exponent);
        Assert.Equal(SDecimal.Zero, sDecimal);

        sDecimal = new SDecimal(0, 0);
        Assert.Equal(0, sDecimal.Mantissa);
        Assert.Equal(0, sDecimal.Exponent);
        Assert.Equal(SDecimal.Zero, sDecimal);

        sDecimal = new SDecimal(1, 0);
        Assert.Equal(0, sDecimal.Mantissa);
        Assert.Equal(0, sDecimal.Exponent);
        Assert.Equal(SDecimal.One, sDecimal);
    }

    [Fact]
    public void SDecimal_AddOperator()
    {
        
    }

    [Fact]
    public void SDecimal_SubtractOperator()
    {
        
    }

    [Fact]
    public void SDecimal_MultiplyOperator()
    {
        
    }

    [Fact]
    public void SDecimal_DivideOperator()
    {
        
    }
}