using System;
using Microsoft.Xna.Framework;
using Xunit;
using Xunit.Abstractions;

namespace OrbitGame.Tests;

public class Ship_Tests
{
    private readonly ITestOutputHelper _output;

    private readonly Planet.PlanetTemplate _testPlanetTemplate = new(new Material(0, 0, 0));
    private readonly Planet _planet;
    private readonly Ship.ShipTemplate _testShipTemplate = new([
        new DVector2<SDecimal>(2, 2),
        new DVector2<SDecimal>(2, -2),
        new DVector2<SDecimal>(-2, -2),
        new DVector2<SDecimal>(-2, 2)
    ], new Material(0, 0, 0));
    private readonly Ship _ship;

    private readonly (DVector2<SDecimal> thrust, DVector2<SDecimal> displacement) _centerThrust = 
        (new(1000, 0), DVector2<SDecimal>.Zero);
    private readonly (DVector2<SDecimal> thrust, DVector2<SDecimal> displacement) _displacedThrust = 
        (new(1000, 0), new(0, 5));
    private readonly SpatialInfo _newPlanetSpatialInfo = new(new(500, 0), Math.PI);

    public Ship_Tests(ITestOutputHelper output)
    {
        _output = output;
        _planet = _testPlanetTemplate
            .CreateInstance("Test Planet", new(DVector2<SDecimal>.Zero, Math.PI / 2), 0, 0, Color.White, null);
        _ship = _testShipTemplate
            .CreateInstance("Test Ship", new(new DVector2<SDecimal>(100, 0)), 100, Color.White, _planet);
    }

    [Fact]
    public void Ship_DrawMethod()
    {
        _ship.Draw();
        throw new NotImplementedException();
    }

    [Fact]
    public void Ship_DrawColliderMethod()
    {
        _ship.DrawCollider();
        throw new NotImplementedException();
    }

    [Fact]
    public void Ship_UpdateShipKeplerianOrbitMethod()
    {
        _ship.UpdateShipKeplerianOrbit([_planet], 0);
        throw new NotImplementedException();
    }

    [Fact]
    public void Ship_UpdatePosition_LandedMethod()
    {
        _planet.SpatialInfo = _newPlanetSpatialInfo;
        _ship.SetLandingState(_planet);
        _ship.UpdatePosition_Landed();
        Assert.Equal(new DVector2<SDecimal>(400, 0), _ship.Position);
        Assert.Equal(-Math.PI / 2, _ship.Angle);
    }

    [Fact]
    public void Ship_SetLandingStateMethod()
    {
        _ship.SetLandingState(_planet);
        Assert.NotNull(_ship.LandingState);
        if (_ship.LandingState == null) throw new NullReferenceException();
        Assert.Equal(new DVector2<SDecimal>(0, -100), _ship.LandingState.Value.RelativePosition);
        Assert.Equal(-Math.PI / 2, _ship.LandingState.Value.RelativeAngle);
    }

    [Fact]
    public void Ship_ApplyThrustMethod()
    {
        _ship.ApplyThrust(_centerThrust.thrust, _centerThrust.displacement);
        Assert.Equal(new DVector2<SDecimal>(10, 0), _ship.CalculateNetAcceleration());
        
        _ship.ApplyThrust(_centerThrust.thrust, _centerThrust.displacement);
        Assert.Equal(new DVector2<SDecimal>(20, 0), _ship.CalculateNetAcceleration());
        
        _ship.ApplyThrust(_displacedThrust.thrust, _displacedThrust.displacement);
        Assert.Equal(new DVector2<SDecimal>(30, 0), _ship.CalculateNetAcceleration());
        Assert.Equal(50, _ship.AngularAcceleration);
    }

    [Fact]
    public void Ship_DestroyMethod()
    {
        Assert.True(_ship.MarkedForRemoval);
    }
}