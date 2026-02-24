using System.Globalization;
using Microsoft.Xna.Framework;
using Xunit.Abstractions;
using static OrbitGame.Planet;
namespace OrbitGame.Tests;

public class KeplerOrbit_Tests
{
    private readonly ITestOutputHelper _output;
    
    private readonly PlanetTemplate _testPlanetTemplate = new(new Material(0, 0, 0));
    private readonly Planet _parent;
    private readonly Planet _body;
    
    public KeplerOrbit_Tests(ITestOutputHelper output)
    {
        _output = output;
        _parent = _testPlanetTemplate.CreateInstance("Test Parent", 
            new SpatialInfo(SD_Vector2.Zero, SD_Vector2.Zero), 1000, 0, Color.White, null);
        _body = _testPlanetTemplate.CreateInstance("Test Body", new SpatialInfo(
            new SD_Vector2(0, 1),
            new SD_Vector2(new(2, -4), 0)
            ), 1, 0, Color.White, _parent);
    }

    [Fact]
    public void KeplerOrbit_Constructor()
    {
        Assert.NotNull(_body.KeplerOrbitPath.Orbit);
        KeplerOrbit orbit = _body.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        
        Assert.Same(_body, orbit.Body);
        Assert.Same(_parent, orbit.Parent);
        Assert.Equal(3 * Math.PI / 2, orbit.Periapsis);
        Assert.Equal(1, orbit.Equation(orbit.Periapsis + Math.PI), new ScientificDecimal(1,-8));
        Assert.Equal(0.400686214285, orbit.Eccentricity, new ScientificDecimal(1,-8));
        Assert.Equal(0.599313785715, orbit.SemiLatusRectum, new ScientificDecimal(1,-8));
        Assert.Equal(0.713935776457, orbit.SemiMajorAxis, new ScientificDecimal(1,-8));
        Assert.Equal(0.654118913435, orbit.SemiMinorAxis, new ScientificDecimal(1,-8));
        Assert.Equal(14671.20295878, orbit.Period, new ScientificDecimal(1,-8));
        Assert.Equal(7335.60147939, orbit.InitialTime);
    }

    [Fact]
    public void KeplerOrbit_CalculateTrueAnomalyFromTimeFromPeriapsis()
    {
        KeplerOrbit orbit = _body.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();

        Assert.Equal(7335.60147939, orbit.CalculateTimeSincePeriapsisFromTrueAnomaly(Math.PI), new ScientificDecimal(1,-8));
        Assert.Equal(Math.PI, orbit.CalculateTrueAnomalyFromTimeSincePeriapsis(7335.60147939));
    }
}