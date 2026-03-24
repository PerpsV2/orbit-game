using Microsoft.Xna.Framework;
using Xunit.Abstractions;

namespace OrbitGame.Tests;

public class KeplerOrbitPathPoint_Tests
{
    private readonly ITestOutputHelper _output;
    
    private readonly Planet.PlanetTemplate _testPlanetTemplate = new(new Material(0, 0, 0));
    private readonly KeplerOrbitPoint _circularOrbitPoint;
    private readonly KeplerOrbitPoint _ellipticOrbitPoint;
    private readonly KeplerOrbitPoint _hyperbolicOrbitPoint;

    private readonly SDecimal _initialTime = 100;

    public KeplerOrbitPathPoint_Tests(ITestOutputHelper output)
    {
        _output = output;
        Constants.SetGravitationalConstant((SDecimal)1);
        var parent = _testPlanetTemplate.CreateInstance("Test Parent", 
            new SpatialInfo(DVector2<SDecimal>.Zero, DVector2<SDecimal>.Zero), 1, 0, Color.White, null);
        var circularBody = _testPlanetTemplate.CreateInstance("Circular Orbit Body", new SpatialInfo(
            new DVector2<SDecimal>(0, 1),
            new DVector2<SDecimal>(Math.Sqrt(1 + 0), 0)
        ), 1, 0, Color.White, parent);
        var ellipticBody = _testPlanetTemplate.CreateInstance("Elliptic Orbit Body", new SpatialInfo(
            new DVector2<SDecimal>(0, 1),
            new DVector2<SDecimal>(Math.Sqrt(1 + 0.96), 0)
        ), 1, 0, Color.White, parent);
        var hyperbolicBody = _testPlanetTemplate.CreateInstance("Hyperbolic Orbit Body", new SpatialInfo(
            new DVector2<SDecimal>(0, 1),
            new DVector2<SDecimal>(Math.Sqrt(1 + 2.6), 0)
        ), 1, 0, Color.White, parent);
        
        circularBody.GenerateKeplerianOrbit(_initialTime);
        ellipticBody.GenerateKeplerianOrbit(_initialTime);
        hyperbolicBody.GenerateKeplerianOrbit(_initialTime);

        _circularOrbitPoint = new KeplerOrbitPoint(circularBody.KeplerOrbitPath, Math.PI);
        _ellipticOrbitPoint = new KeplerOrbitPoint(ellipticBody.KeplerOrbitPath, Math.PI);
        _hyperbolicOrbitPoint = new KeplerOrbitPoint(hyperbolicBody.KeplerOrbitPath, 0);
    }

    [Fact]
    public void KeplerOrbitPathPoint_GetTimeAtPoint()
    {
        //Assert.Equal(_initialTime + 3*Math.PI/2, _circularOrbitPathPoint.GetTimeAtPoint(_initialTime), Assert.Epsilon);
        Assert.Equal(_initialTime + 125 * Math.PI, _ellipticOrbitPoint.GetTimeAtPoint(_initialTime), Assert.Epsilon);
        Assert.Equal(_initialTime, _hyperbolicOrbitPoint.GetTimeAtPoint(_initialTime));
    }
}