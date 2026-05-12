namespace qQEngine.Tests;

public class UnitTest1
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
}