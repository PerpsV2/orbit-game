using Microsoft.Xna.Framework;
using Xunit.Abstractions;

namespace OrbitGame.Tests;

public class Ship_Tests
{
    private readonly ITestOutputHelper _output;

    private readonly Planet.PlanetTemplate _testPlanetTemplate = new(new Material(0, 0, 0));
    private readonly Planet _planet;
    private readonly Ship.ShipTemplate _testShipTemplate = new([
        new SD_Vector2(+2, +2),
        new SD_Vector2(+2, -2),
        new SD_Vector2(-2, -2),
        new SD_Vector2(-2, +2)
    ], new Material(0, 0, 0));
    private readonly Ship _ship;

    public Ship_Tests(ITestOutputHelper output)
    {
        _output = output;
        _planet = _testPlanetTemplate.CreateInstance("Test Planet", new(), 10000, 100, Color.White, null);
        _ship = _testShipTemplate.CreateInstance("Test Ship", new(new SD_Vector2(1000, 0)), 10, Color.White, _planet);
    }

    [Fact]
    public void Ship_UpdateShipKeplerianOrbitMethod()
    {
        
    }

    [Fact]
    public void Ship_UpdatePosition_LandedMethod()
    {
        
    }

    [Fact]
    public void Ship_SetLandingStateMethod()
    {
        
    }

    [Fact]
    public void Ship_DisturbLandingStateMethod()
    {
        
    }

    [Fact]
    public void Ship_ApplyThrustMethod()
    {
        
    }

    [Fact]
    public void Ship_ResetThrustMethod()
    {
        _ship.ApplyThrust(new SD_Vector2(100, 0), SD_Vector2.Zero);
        _ship.ResetThrust();
        _ship.CalculateNetAcceleration();
        Assert.Equal(1, 1);
    }

    [Fact]
    public void Ship_CalculateNetAccelerationMethod()
    {
        _ship.CalculateNetAcceleration();
        Assert.Equal(1, 1);
    }

    [Fact]
    public void Ship_DestroyMethod()
    {
        _ship.Destroy();
        Assert.True(_ship.MarkedForRemoval);
    }
}