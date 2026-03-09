using System;
using Microsoft.Xna.Framework;
using Xunit;
using Xunit.Abstractions;

namespace OrbitGame.Tests;

public class KeplerOrbitPathPoint_Tests
{
    private readonly ITestOutputHelper _output;
    
    private readonly Planet.PlanetTemplate _testPlanetTemplate = new(new Material(0, 0, 0));
    private readonly KeplerOrbitPathPoint _circularOrbitPathPoint;
    private readonly KeplerOrbitPathPoint _ellipticOrbitPathPoint;
    private readonly KeplerOrbitPathPoint _hyperbolicOrbitPathPoint;

    private readonly ScientificDecimal _initialTime = 100;

    public KeplerOrbitPathPoint_Tests(ITestOutputHelper output)
    {
        _output = output;
        Constants.G = 1;
        var parent = _testPlanetTemplate.CreateInstance("Test Parent", 
            new SpatialInfo(SD_Vector2.Zero, SD_Vector2.Zero), 1, 0, Color.White, null);
        var circularBody = _testPlanetTemplate.CreateInstance("Circular Orbit Body", new SpatialInfo(
            new SD_Vector2(0, 1),
            new SD_Vector2(Math.Sqrt(1 + 0), 0)
        ), 1, 0, Color.White, parent);
        var ellipticBody = _testPlanetTemplate.CreateInstance("Elliptic Orbit Body", new SpatialInfo(
            new SD_Vector2(0, 1),
            new SD_Vector2(Math.Sqrt(1 + 0.96), 0)
        ), 1, 0, Color.White, parent);
        var hyperbolicBody = _testPlanetTemplate.CreateInstance("Hyperbolic Orbit Body", new SpatialInfo(
            new SD_Vector2(0, 1),
            new SD_Vector2(Math.Sqrt(1 + 2.6), 0)
        ), 1, 0, Color.White, parent);
        
        circularBody.GenerateKeplerianOrbit(_initialTime);
        ellipticBody.GenerateKeplerianOrbit(_initialTime);
        hyperbolicBody.GenerateKeplerianOrbit(_initialTime);

        _circularOrbitPathPoint = new KeplerOrbitPathPoint(circularBody.KeplerOrbitPath, Math.PI);
        _ellipticOrbitPathPoint = new KeplerOrbitPathPoint(ellipticBody.KeplerOrbitPath, Math.PI);
        _hyperbolicOrbitPathPoint = new KeplerOrbitPathPoint(hyperbolicBody.KeplerOrbitPath, 0);
    }

    [Fact]
    public void KeplerOrbitPathPoint_GetTimeAtPoint()
    {
        Assert.Equal(_initialTime + 3*Math.PI/2, _circularOrbitPathPoint.GetTimeAtPoint(_initialTime), Assert.Epsilon);
        Assert.Equal(_initialTime + 125 * Math.PI, _ellipticOrbitPathPoint.GetTimeAtPoint(_initialTime), Assert.Epsilon);
        Assert.Equal(_initialTime, _hyperbolicOrbitPathPoint.GetTimeAtPoint(_initialTime));
    }
}