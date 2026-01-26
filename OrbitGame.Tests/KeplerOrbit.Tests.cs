using SkiaSharp;

namespace OrbitGame.Tests;

public class KeplerOrbit_Tests
{
    private static readonly Planet TestStar = new (
            1000, 
            Vector2.Zero, 
            Vector2.Zero, 
            0, 
            new Material(0), 
            SKColor.Empty, 
            null,
            "TestStar"
            );
    
    private static readonly Planet TestPlanet = new (
        100, 
        new Vector2(10, 0), 
        new Vector2(0, 1), 
        0, 
        new Material(0), 
        SKColor.Empty, 
        null,
        "TestPlanet"
    );
    
    private readonly KeplerOrbit _circularOrbit = new(TestPlanet, TestStar, 0, Math.PI / 4, 100);
    private readonly KeplerOrbit _ellipticOrbit = new(TestPlanet, TestStar, 0.5f, Math.PI / 4, 100);
    private readonly KeplerOrbit _parabolicOrbit = new(TestPlanet, TestStar, 1, Math.PI / 4, 100);
    private readonly KeplerOrbit _hyperbolicOrbit = new(TestPlanet, TestStar, 2, Math.PI / 4, 100);
    
    [Fact]
    public void KeplerOrbit_EquationProperty()
    {
        AssertExtensions.Equal(100, _circularOrbit.Equation(Math.PI / 4));
        AssertExtensions.Equal(100, _circularOrbit.Equation(3 * Math.PI / 4));
        
        AssertExtensions.Equal(66 + 2d/3, _ellipticOrbit.Equation(Math.PI / 4));
        AssertExtensions.Equal(100, _ellipticOrbit.Equation(3 * Math.PI / 4));
        
        AssertExtensions.Equal(50, _parabolicOrbit.Equation(Math.PI / 4));
        AssertExtensions.Equal(100, _parabolicOrbit.Equation(3 * Math.PI / 4));
        
        AssertExtensions.Equal(33 + 1d/3, _hyperbolicOrbit.Equation(Math.PI / 4));
        AssertExtensions.Equal(100, _hyperbolicOrbit.Equation(3 * Math.PI / 4));
    }

    [Fact]
    public void KeplerOrbit_SemiMajorAxisProperty()
    {
        AssertExtensions.Equal(100, _circularOrbit.SemiMajorAxis);
        AssertExtensions.Equal(133 + 1d/3, _ellipticOrbit.SemiMajorAxis);
        Assert.Equal(ScientificDecimal.PosInfinity, _parabolicOrbit.SemiMajorAxis);
        AssertExtensions.Equal(-33 - 1d/3, _hyperbolicOrbit.SemiMajorAxis);
    }

    [Fact]
    public void KeplerOrbit_SemiMinorAxisProperty()
    {
        AssertExtensions.Equal(100, _circularOrbit.SemiMinorAxis);
        AssertExtensions.Equal(200 * Math.Sqrt(3) / 3, _ellipticOrbit.SemiMinorAxis);
        Assert.Equal(ScientificDecimal.PosInfinity, _parabolicOrbit.SemiMinorAxis);
        Assert.Equal(ScientificDecimal.PosInfinity, _hyperbolicOrbit.SemiMinorAxis);
    }

    [Fact]
    public void KeplerOrbit_SphereOfInfluenceRadiusProperty()
    {
        Assert.NotNull(_circularOrbit.SphereOfInfluenceRadius);
        Assert.NotNull(_ellipticOrbit.SphereOfInfluenceRadius);
        Assert.Null(_parabolicOrbit.SphereOfInfluenceRadius);
        Assert.Null(_hyperbolicOrbit.SphereOfInfluenceRadius);
        
        AssertExtensions.Equal(100 * Math.Pow(0.1, 2f/5), _circularOrbit.SphereOfInfluenceRadius.Value);
        AssertExtensions.Equal((133 + 1d/3) * Math.Pow(0.1, 2f/5), _ellipticOrbit.SphereOfInfluenceRadius.Value);
    }
    
    [Fact]
    public void KeplerOrbit_PeriodProperty()
    {
        AssertExtensions.Equal(Math.Tau * (Math.Pow(100, 3) / Constants.G / 1000).Sqrt(), _circularOrbit.Period);
        AssertExtensions.Equal(Math.Tau * (Math.Pow(133 + 1d/3, 3) / Constants.G / 1000).Sqrt(), _ellipticOrbit.Period);
        Assert.Equal(ScientificDecimal.PosInfinity, _parabolicOrbit.Period);
        Assert.Equal(ScientificDecimal.PosInfinity, _hyperbolicOrbit.Period);
    }

    [Fact]
    public void KeplerOrbit_CenterProperty()
    {
        Assert.NotNull(_circularOrbit.Center);
        Assert.NotNull(_ellipticOrbit.Center);
        Assert.Null(_parabolicOrbit.Center);
        Assert.Null(_hyperbolicOrbit.Center);
        
        AssertExtensions.Equal(Vector2.Zero, _circularOrbit.Center.Value);
        AssertExtensions.Equal(new Vector2(-Math.Cos(Math.PI / 4), -Math.Sin(Math.PI / 4)) * (66 + 2d/3), 
            _ellipticOrbit.Center.Value);
    }
}