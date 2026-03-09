using System;
using Microsoft.Xna.Framework;
using Xunit;
using Xunit.Abstractions;

namespace OrbitGame.Tests;

public class Planet_Tests
{
    private readonly ITestOutputHelper _output;
    private readonly DebugGraphicsHandler _graphics;

    private readonly Planet.PlanetTemplate _testPlanetTemplate = new(new Material(0, 0, 0));
    private readonly Planet _planet;

    public Planet_Tests(ITestOutputHelper output)
    {
        DebugGraphicsHandler debugGraphics = new();
        OrbitGame.Graphics = debugGraphics;
        _graphics = debugGraphics;
        _output = output;
        _planet = _testPlanetTemplate.CreateInstance("Test Planet", new(SD_Vector2.Zero, Math.PI / 2), 
            1, 100, Color.White, null);
    }
    
    [Fact]
    public void Planet_DrawMethod()
    {
        _planet.Draw();
        throw new NotImplementedException();
    }
    
    [Fact]
    public void Planet_DrawColliderMethod()
    {
        _planet.DrawCollider();
        throw new NotImplementedException();
    }
}