namespace OrbitGame.Tests;

public class Utils_Tests
{
    [Fact]
    public void Utils_ClampMethod()
    {
        int clampedInt = Utils.Clamp(-10, -20, 30);
        double clampedDouble = Utils.Clamp(-30, 20, 30);
        ScientificDecimal clampedScientificDecimal = Utils.Clamp(
            ScientificDecimal.NegInfinity, 
            ScientificDecimal.NegInfinity, 
            30
            );
        
        Assert.Equal(-10, clampedInt);
        Assert.Equal(20, clampedDouble);
        Assert.Equal(ScientificDecimal.NegInfinity, clampedScientificDecimal);
    }

    [Fact]
    public void Utils_UnsignedModMethod()
    {
        double unsignedMod1 = Utils.UnsignedMod(0.5, 1);
        double unsignedMod2 = Utils.UnsignedMod(5 * Math.PI, Math.Tau);
        double unsignedMod3 = Utils.UnsignedMod(-Math.PI / 2, Math.Tau);
        
        AssertExtensions.Equal(0.5, unsignedMod1);
        AssertExtensions.Equal(Math.PI, unsignedMod2);
        AssertExtensions.Equal(3 * Math.PI / 2, unsignedMod3);
    }

    [Fact]
    public void Utils_DecimalSqrtMethod()
    {
        AssertExtensions.Equal(4, Utils.DecimalSqrt(16));
        AssertExtensions.Equal(Math.Sqrt(2d), Utils.DecimalSqrt(2));
        Assert.Throws<ArithmeticException>(() => Utils.DecimalSqrt(-1));
    }

    [Fact]
    public void Utils_CalculateTriangleAreaMethod()
    {
        Assert.Equal(3, Utils.CalculateTriangleArea(new(-1, 5), new(2, 5), new(0, 3)));
        Assert.Equal(3, Utils.CalculateTriangleArea(new(-1, 5), new(0, 3), new(2, 5)));
        Assert.Equal(0, Utils.CalculateTriangleArea(new(-1, 5), new(0, 3), new(0, 3)));
    }
}