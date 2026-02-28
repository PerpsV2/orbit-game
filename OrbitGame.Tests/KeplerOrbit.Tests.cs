using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit.Abstractions;
using static OrbitGame.Planet;
namespace OrbitGame.Tests;

public class KeplerOrbit_Tests
{
    private readonly ITestOutputHelper _output;
    
    private readonly OrbitGame _orbitGame = new();
    private readonly PlanetTemplate _testPlanetTemplate = new(new Material(0, 0, 0));
    private readonly Planet _parent;
    private readonly Planet _ellipticBody;
    private readonly Planet _hyperbolicBody;

    public KeplerOrbit_Tests(ITestOutputHelper output)
    {
        _output = output;
        _parent = _testPlanetTemplate.CreateInstance("Test Parent", 
            new SpatialInfo(SD_Vector2.Zero, SD_Vector2.Zero), 1000, 0, Color.White, null);
        _ellipticBody = _testPlanetTemplate.CreateInstance("Elliptic Body", new SpatialInfo(
            new SD_Vector2(0, 1),
            new SD_Vector2(new(2, -4), 0)
            ), 1, 0, Color.White, _parent);
        _hyperbolicBody = _testPlanetTemplate.CreateInstance("Hyperbolic Body", new SpatialInfo(
            new SD_Vector2(0, 1),
            new SD_Vector2(new(4, -4), 0)
        ), 1, 0, Color.White, _parent);
        
        _ellipticBody.GenerateKeplerianOrbit(0);
        _hyperbolicBody.GenerateKeplerianOrbit(0);
    }

    [Fact]
    public void KeplerOrbit_Constructor()
    {
        Assert.NotNull(_ellipticBody.KeplerOrbitPath.Orbit);
        KeplerOrbit ellipticOrbit = _ellipticBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        
        Assert.Same(_ellipticBody, ellipticOrbit.Body);
        Assert.Same(_parent, ellipticOrbit.Parent);
        Assert.Equal(3 * Math.PI / 2, ellipticOrbit.Periapsis);
        Assert.Equal(1, ellipticOrbit.Equation(ellipticOrbit.Periapsis + Math.PI), new ScientificDecimal(1,-8));
        Assert.Equal(0.400686214285, ellipticOrbit.Eccentricity, new ScientificDecimal(1,-8));
        Assert.Equal(0.599313785715, ellipticOrbit.SemiLatusRectum, new ScientificDecimal(1,-8));
        Assert.Equal(0.713935776457, ellipticOrbit.SemiMajorAxis, new ScientificDecimal(1,-8));
        Assert.Equal(0.654118913435, ellipticOrbit.SemiMinorAxis, new ScientificDecimal(1,-8));
        Assert.Equal(14671.20295878, ellipticOrbit.Period, new ScientificDecimal(1,-8));
        Assert.Equal(7335.60147939, ellipticOrbit.InitialTimeSincePeriapsis, new ScientificDecimal(1, -8));
        
        Assert.NotNull(_hyperbolicBody.KeplerOrbitPath.Orbit);
        KeplerOrbit hyperbolicOrbit = _hyperbolicBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        
        Assert.Same(_hyperbolicBody, hyperbolicOrbit.Body);
        Assert.Same(_parent, hyperbolicOrbit.Parent);
        Assert.Equal(Math.PI / 2, hyperbolicOrbit.Periapsis);
        Assert.Equal(1, hyperbolicOrbit.Equation(hyperbolicOrbit.Periapsis), new ScientificDecimal(1,-8));
        Assert.Equal(1.397255142861, hyperbolicOrbit.Eccentricity, new ScientificDecimal(1,-8));
        Assert.Equal(2.397255142861, hyperbolicOrbit.SemiLatusRectum, new ScientificDecimal(1, -8));
        Assert.Equal(-2.51727389304, hyperbolicOrbit.SemiMajorAxis, new ScientificDecimal(1,-8));
        Assert.Equal(2.456531657861, hyperbolicOrbit.SemiMinorAxis, new ScientificDecimal(1, -8));
        Assert.Equal(ScientificDecimal.PosInfinity, hyperbolicOrbit.Period, new ScientificDecimal(1,-8));
        Assert.Equal(0, hyperbolicOrbit.InitialTimeSincePeriapsis, new ScientificDecimal(1, -8));
    }

    [Fact]
    public void KeplerOrbit_CalculateTrueAnomalyFromTimeFromPeriapsis()
    { 
        KeplerOrbit ellipticOrbit = _ellipticBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();

        Assert.Equal(7335.60147939, ellipticOrbit.CalculateTimeSincePeriapsisFromTrueAnomaly(Math.PI), new ScientificDecimal(1,-8));
        Assert.Equal(Math.PI, ellipticOrbit.CalculateTrueAnomalyFromTimeSincePeriapsis(7335.60147939), new ScientificDecimal(1,-8));
        
        KeplerOrbit hyperbolicOrbit = _hyperbolicBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        
        Assert.Equal(0, hyperbolicOrbit.CalculateTimeSincePeriapsisFromTrueAnomaly(0), new ScientificDecimal(1,-8));
        double asymptoteTrueAnomaly = Math.Acos(-1 / hyperbolicOrbit.Eccentricity);
        // TODO: very bad unit tests but they work
        // true anomaly approaches asymptote after arbitrarily large time frame
        Assert.Equal(asymptoteTrueAnomaly, hyperbolicOrbit.CalculateTrueAnomalyFromTimeSincePeriapsis(1000000), new ScientificDecimal(1,-8));
        // time since periapsis approaches infinity as true anomaly approaches asymptote
        Assert.True(hyperbolicOrbit.CalculateTimeSincePeriapsisFromTrueAnomaly(asymptoteTrueAnomaly - 0.000001) > 1000000000);
    }
}