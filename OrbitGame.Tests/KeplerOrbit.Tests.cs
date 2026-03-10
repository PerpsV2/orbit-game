using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;
using Xunit.Abstractions;
using static OrbitGame.Planet;
using NullReferenceException = System.NullReferenceException;

namespace OrbitGame.Tests;

public class KeplerOrbit_Tests
{
    private readonly ITestOutputHelper _output;
    
    private readonly PlanetTemplate _testPlanetTemplate = new(new Material(0, 0, 0));
    private readonly Planet _parent;
    private readonly Planet _circularBody;
    private readonly Planet _ellipticBody;
    private readonly Planet _hyperbolicBody;

    private readonly ScientificDecimal _largeTimeFrame = 10000;

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
        Assert.Equal(false, orbit.Prograde);

        Assert.Equal(SD_Vector2.Zero, orbit.Center);
        Assert.Equal(1, orbit.SphereOfInfluenceRadius);
        
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
        Assert.Equal(false, orbit.Prograde);
        
        Assert.Equal(new(0, -24), orbit.Center);
        Assert.Equal(25, orbit.SphereOfInfluenceRadius, Assert.Epsilon);
        
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
        Assert.Equal(false, orbit.Prograde);
        
        Assert.Equal(ScientificDecimal.PosInfinity, orbit.Period, Assert.Epsilon);
        Assert.Equal(0, orbit.InitialTimeSincePeriapsis);
    }

    [Fact]
    public void KeplerOrbit_CalculateTrueAnomalyFromTimeSincePeriapsis()
    {
        KeplerOrbit orbit = _circularBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        for (int i = 0; i < 10; ++i)
            Assert.Equal(Utils.WrapAngle(i), orbit.CalculateTrueAnomalyFromTimeSincePeriapsis(i));

        orbit = _ellipticBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        Assert.Equal(Math.PI, orbit.CalculateTrueAnomalyFromTimeSincePeriapsis(Math.Tau * 62.5), Assert.Epsilon);

        orbit = _hyperbolicBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        Assert.Equal(Math.Acos(-1d / orbit.Eccentricity), orbit.CalculateTrueAnomalyFromTimeSincePeriapsis(_largeTimeFrame), 
            Assert.Epsilon);
    }

    [Fact]
    public void KeplerOrbit_CalculateTimeSincePeriapsisFromTrueAnomaly()
    {
        KeplerOrbit orbit = _circularBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        for (double i = 0; i < Math.Tau; i += Math.Tau / 10)
            Assert.Equal(i, orbit.CalculateTimeSincePeriapsisFromTrueAnomaly(i), Assert.Epsilon);

        orbit = _ellipticBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        Assert.Equal(Math.Tau * 62.5, orbit.CalculateTimeSincePeriapsisFromTrueAnomaly(Math.PI), Assert.Epsilon);
        
        orbit = _hyperbolicBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        Assert.True(orbit.CalculateTimeSincePeriapsisFromTrueAnomaly(Math.Acos(-1d / orbit.Eccentricity)) > _largeTimeFrame);
    }

    [Fact]
    public void KeplerOrbit_GetOrbitPositionFromTrueAnomaly()
    {
        KeplerOrbit orbit = _circularBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        for (int i = 0; i < 10; ++i)
            Assert.Equal(SD_Vector2.FromPolar(i + orbit.Periapsis), orbit.GetOrbitPositionFromTrueAnomaly(i));

        orbit = _ellipticBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        Assert.Equal(new SD_Vector2(-1.96, 0), orbit.GetOrbitPositionFromTrueAnomaly(Math.PI / 2));

        orbit = _hyperbolicBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        Assert.Equal(new SD_Vector2(-3.6, 0), orbit.GetOrbitPositionFromTrueAnomaly(Math.PI / 2));
    }

    [Fact]
    public void KeplerOrbit_GetStateAtTime()
    {
        KeplerOrbit orbit = _circularBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        SpatialInfo bodyState = orbit.GetStateAtTime(Math.PI);
        Assert.Equal(new SD_Vector2(0, -1), bodyState.Position);
        Assert.Equal(new SD_Vector2(-1, 0), bodyState.Velocity); 
        bodyState = orbit.GetStateAtTime(0);
        Assert.Equal(new SD_Vector2(0, 1), bodyState.Position);
        Assert.Equal(new SD_Vector2(1, 0), bodyState.Velocity);
        
        orbit = _ellipticBody.KeplerOrbitPath.Orbit ?? throw new NullReferenceException();
        bodyState = orbit.GetStateAtTime(Math.Tau * 62.5);
        Assert.Equal(new SD_Vector2(0, -49), bodyState.Position);
        Assert.Equal(new SD_Vector2(-Math.Sqrt(2d/49-1d/25), 0), bodyState.Velocity);
        bodyState = orbit.GetStateAtTime(0);
        Assert.Equal(new SD_Vector2(0, 1), bodyState.Position);
        Assert.Equal(new SD_Vector2(Math.Sqrt(1.96), 0), bodyState.Velocity);
    }
}