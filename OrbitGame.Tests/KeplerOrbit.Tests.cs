using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit.Abstractions;
using static OrbitGame.Planet;
namespace OrbitGame.Tests;

public class KeplerOrbit_Tests
{
    private readonly ITestOutputHelper _output;
    
    private readonly PlanetTemplate _testPlanetTemplate = new(new Material(0, 0, 0));
    private readonly Planet _parent;
    private readonly Planet _circularBody;
    private readonly Planet _ellipticBody;
    private readonly Planet _hyperbolicBody;

    public KeplerOrbit_Tests(ITestOutputHelper output)
    {
        _output = output;
        Constants.G = 1;
        _parent = _testPlanetTemplate.CreateInstance("Test Parent", 
            new SpatialInfo(SD_Vector2.Zero, SD_Vector2.Zero), 1, 0, Color.White, null);
        _circularBody = _testPlanetTemplate.CreateInstance("Circular Orbit Body", new SpatialInfo(
            new SD_Vector2(0, 1),
            new SD_Vector2(Math.Sqrt(1 + 0), 0)
        ), 1, 0, Color.White, _parent);
        _ellipticBody = _testPlanetTemplate.CreateInstance("Elliptic Orbit Body", new SpatialInfo(
            new SD_Vector2(0, 1),
            new SD_Vector2(Math.Sqrt(1 + 0.96), 0)
        ), 1, 0, Color.White, _parent);
        _hyperbolicBody = _testPlanetTemplate.CreateInstance("Hyperbolic Orbit Body", new SpatialInfo(
            new SD_Vector2(0, 1),
            new SD_Vector2(Math.Sqrt(1 + 2.6), 0)
        ), 1, 0, Color.White, _parent);
        
        _circularBody.GenerateKeplerianOrbit(0);
        _ellipticBody.GenerateKeplerianOrbit(0);
        _hyperbolicBody.GenerateKeplerianOrbit(0);
    }

    [Fact]
    public void KeplerOrbit_CircularConstructor()
    {
        Assert.NotNull(_circularBody.KeplerOrbitPath.Orbit);
        KeplerOrbit orbit = _circularBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        
        Assert.Same(_circularBody, orbit.Body);
        Assert.Same(_parent, orbit.Parent);
        
        // Circular orbit has equal distance independent of true anomaly
        Assert.Equal(1, orbit.Equation(orbit.Periapsis), Assert.Epsilon);
        Assert.Equal(1, orbit.Equation(orbit.Periapsis + Math.PI), Assert.Epsilon);
        
        Assert.Equal(0, orbit.Eccentricity, Assert.Epsilon);
        Assert.Equal(1, orbit.SemiLatusRectum, Assert.Epsilon);
        Assert.Equal(1, orbit.SemiMajorAxis, Assert.Epsilon);
        Assert.Equal(1, orbit.SemiMinorAxis, Assert.Epsilon);
        
        Assert.Equal(Math.Tau, orbit.Period, Assert.Epsilon);
        // Time since periapsis for this orbit is the same as arc angle between periapsis and starting position
        Assert.Equal(Utils.WrapAngle(Math.PI / 2 - orbit.Periapsis), orbit.InitialTimeSincePeriapsis);
    }

    [Fact]
    public void KeplerOrbit_EllipticConstructor()
    {
        Assert.NotNull(_ellipticBody.KeplerOrbitPath.Orbit);
        KeplerOrbit orbit = _ellipticBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        
        Assert.Same(_ellipticBody, orbit.Body);
        Assert.Same(_parent, orbit.Parent);
        
        Assert.Equal(1, orbit.Equation(orbit.Periapsis), Assert.Epsilon);
        Assert.Equal(49, orbit.Equation(orbit.Periapsis + Math.PI), Assert.Epsilon);
        
        Assert.Equal(Math.PI / 2, orbit.Periapsis, Assert.Epsilon);
        Assert.Equal(0.96, orbit.Eccentricity, Assert.Epsilon);
        Assert.Equal(1.96, orbit.SemiLatusRectum, Assert.Epsilon);
        Assert.Equal(25, orbit.SemiMajorAxis, Assert.Epsilon);
        Assert.Equal(7, orbit.SemiMinorAxis, Assert.Epsilon);
        
        Assert.Equal(Math.Tau * 125, orbit.Period, Assert.Epsilon);
        Assert.Equal(0, orbit.InitialTimeSincePeriapsis);
    }

    [Fact]
    public void KeplerOrbit_HyperbolicConstructor()
    {
        Assert.NotNull(_hyperbolicBody.KeplerOrbitPath.Orbit);
        KeplerOrbit orbit = _hyperbolicBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        
        Assert.Same(_hyperbolicBody, orbit.Body);
        Assert.Same(_parent, orbit.Parent);
        
        Assert.Equal(1, orbit.Equation(orbit.Periapsis), Assert.Epsilon);
        
        Assert.Equal(Math.PI / 2, orbit.Periapsis, Assert.Epsilon);
        Assert.Equal(2.6, orbit.Eccentricity, Assert.Epsilon);
        Assert.Equal(3.6, orbit.SemiLatusRectum, Assert.Epsilon);
        Assert.Equal(-0.625, orbit.SemiMajorAxis, Assert.Epsilon);
        Assert.Equal(1.5, orbit.SemiMinorAxis, Assert.Epsilon);
        
        Assert.Equal(ScientificDecimal.PosInfinity, orbit.Period, Assert.Epsilon);
        Assert.Equal(0, orbit.InitialTimeSincePeriapsis);
    }

    [Fact]
    public void KeplerOrbit_CalculateTrueAnomalyFromTimeFromPeriapsis()
    { 
        
    }
}