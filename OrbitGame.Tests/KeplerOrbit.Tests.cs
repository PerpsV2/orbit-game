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

    private readonly SDecimal _largeTimeFrame = 100;

    public KeplerOrbit_Tests(ITestOutputHelper output)
    {
        KinematicObject.KinematicObjectTemplate.DestroyAll();
        
        _output = output;
        Constants.SetGravitationalConstant((SDecimal)1);
        _parent = _testPlanetTemplate.CreateInstance("Test Parent", 
            new SpatialInfo(Vec2<SDecimal>.Zero, Vec2<SDecimal>.Zero), 1, 0, Color.White, null, 0);
        _circularBody = _testPlanetTemplate.CreateInstance("Circular Orbit Body", new SpatialInfo(
            new Vec2<SDecimal>(0, 1),
            new Vec2<SDecimal>(Math.Sqrt(1 + 0), 0)
        ), 1, 0, Color.White, _parent, 0);
        _ellipticBody = _testPlanetTemplate.CreateInstance("Elliptic Orbit Body", new SpatialInfo(
            new Vec2<SDecimal>(0, 1),
            new Vec2<SDecimal>(Math.Sqrt(1 + 0.96), 0)
        ), 1, 0, Color.White, _parent, 0);
        _hyperbolicBody = _testPlanetTemplate.CreateInstance("Hyperbolic Orbit Body", new SpatialInfo(
            new Vec2<SDecimal>(0, 1),
            new Vec2<SDecimal>(Math.Sqrt(1 + 2.6), 0)
        ), 1, 0, Color.White, _parent, 0);
        
        _circularBody.GenerateOrbitPath(0);
        _ellipticBody.GenerateOrbitPath(0);
        _hyperbolicBody.GenerateOrbitPath(0);
    }

    [Fact]
    public void KeplerOrbit_CircularConstructor()
    {
        Assert.NotNull(_circularBody.OrbitPath.Conics[0].Orbit);
        KeplerOrbit orbit = _circularBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        
        Assert.Same(_circularBody, orbit.Body);
        Assert.Same(_parent, orbit.Parent);
        
        Assert.Equal(0, orbit.Eccentricity, Assert.Epsilon);
        Assert.Equal(1, orbit.SemiLatusRectum, Assert.Epsilon);
        Assert.Equal(1, orbit.SemiMajorAxis, Assert.Epsilon);
        Assert.Equal(1, orbit.SemiMinorAxis, Assert.Epsilon);
        Assert.Equal(false, orbit.Prograde);

        Assert.Equal(Vec2<SDecimal>.Zero, orbit.Center);
        Assert.Equal(1, orbit.SphereOfInfluenceRadius);
        
        Assert.Equal(Math.Tau, orbit.Period, Assert.Epsilon);
        // Time since periapsis for this orbit is the same as arc angle between periapsis and starting position
        Assert.Equal(Utils.WrapAngle(Math.PI / 2 - orbit.Periapsis), orbit.InitialTimeSincePeriapsis);
    }

    [Fact]
    public void KeplerOrbit_EllipticConstructor()
    {
        KinematicObject.KinematicObjectTemplate.DestroyAll();
        
        Assert.NotNull(_ellipticBody.OrbitPath.Conics[0].Orbit);
        KeplerOrbit orbit = _ellipticBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        
        Assert.Same(_ellipticBody, orbit.Body);
        Assert.Same(_parent, orbit.Parent);
        
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
        Assert.NotNull(_hyperbolicBody.OrbitPath.Conics[0].Orbit);
        KeplerOrbit orbit = _hyperbolicBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        
        Assert.Same(_hyperbolicBody, orbit.Body);
        Assert.Same(_parent, orbit.Parent);
        
        Assert.Equal(Math.PI / 2, orbit.Periapsis, Assert.Epsilon);
        Assert.Equal(2.6, orbit.Eccentricity, Assert.Epsilon);
        Assert.Equal(3.6, orbit.SemiLatusRectum, Assert.Epsilon);
        Assert.Equal(-0.625, orbit.SemiMajorAxis, Assert.Epsilon);
        Assert.Equal(1.5, orbit.SemiMinorAxis, Assert.Epsilon);
        Assert.Equal(false, orbit.Prograde);
        
        Assert.Equal(SDecimal.PositiveInfinity, orbit.Period, Assert.Epsilon);
        Assert.Equal(0, orbit.InitialTimeSincePeriapsis);
    }

    [Fact]
    public void KeplerOrbit_CalculateTrueAnomalyFromTimeSincePeriapsis()
    {
        KeplerOrbit orbit = _circularBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        for (int i = 0; i < 10; ++i)
            Assert.Equal(Utils.WrapAngle(i), orbit.GetTrueAnomalyFromTimeSincePeriapsis(i));

        orbit = _ellipticBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        Assert.Equal(Math.PI, orbit.GetTrueAnomalyFromTimeSincePeriapsis(Math.Tau * 62.5), Assert.Epsilon);

        orbit = _hyperbolicBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        Assert.Equal(Math.Acos(-1d / orbit.Eccentricity), orbit.GetTrueAnomalyFromTimeSincePeriapsis(_largeTimeFrame), 
            Assert.Epsilon);
    }

    [Fact]
    public void KeplerOrbit_CalculateTimeSincePeriapsisFromTrueAnomaly()
    {
        KeplerOrbit orbit = _circularBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        for (double i = 0; i < Math.Tau; i += Math.Tau / 10)
            Assert.Equal(i, orbit.GetTimeSincePeriapsisFromTrueAnomaly(i), Assert.Epsilon);

        orbit = _ellipticBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        Assert.Equal(Math.Tau * 62.5, orbit.GetTimeSincePeriapsisFromTrueAnomaly(Math.PI), Assert.Epsilon);
        
        orbit = _hyperbolicBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        Assert.Equal(Double.NaN, orbit.GetTimeSincePeriapsisFromTrueAnomaly(Math.Acos(-1d / orbit.Eccentricity)).Mantissa);
    }

    [Fact]
    public void KeplerOrbit_GetOrbitPositionFromTrueAnomaly()
    {
        KeplerOrbit orbit = _circularBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        for (int i = 0; i < 10; ++i)
            Assert.Equal(Vec2<SDecimal>.FromPolar(i + orbit.Periapsis), orbit.GetOrbitPositionFromTrueAnomaly(i));

        orbit = _ellipticBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        Assert.Equal(new Vec2<SDecimal>(-1.96, 0), orbit.GetOrbitPositionFromTrueAnomaly(Math.PI / 2));

        orbit = _hyperbolicBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        Assert.Equal(new Vec2<SDecimal>(-3.6, 0), orbit.GetOrbitPositionFromTrueAnomaly(Math.PI / 2));
    }

    [Fact]
    public void KeplerOrbit_GetStateAtTime()
    {
        KeplerOrbit orbit = _circularBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        SpatialInfo bodyState = orbit.GetSpatialInfoAtTime(Math.PI);
        Assert.Equal(new Vec2<SDecimal>(0, -1), bodyState.Position);
        Assert.Equal(new Vec2<SDecimal>(-1, 0), bodyState.Velocity); 
        bodyState = orbit.GetSpatialInfoAtTime(0);
        Assert.Equal(new Vec2<SDecimal>(0, 1), bodyState.Position);
        Assert.Equal(new Vec2<SDecimal>(1, 0), bodyState.Velocity);
        
        orbit = _ellipticBody.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        bodyState = orbit.GetSpatialInfoAtTime(Math.Tau * 62.5);
        Assert.Equal(new Vec2<SDecimal>(0, -49), bodyState.Position);
        Assert.Equal(new Vec2<SDecimal>(-Math.Sqrt(2d/49-1d/25), 0), bodyState.Velocity);
        bodyState = orbit.GetSpatialInfoAtTime(0);
        Assert.Equal(new Vec2<SDecimal>(0, 1), bodyState.Position, 0.05);
        Assert.Equal(new Vec2<SDecimal>(Math.Sqrt(1.96), 0), bodyState.Velocity, 0.05);
    }
}