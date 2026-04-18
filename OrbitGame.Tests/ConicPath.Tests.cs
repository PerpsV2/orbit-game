using System;
using Microsoft.Xna.Framework;
using Xunit;
using Xunit.Abstractions;

namespace OrbitGame.Tests;

public class ConicPath_Tests
{
    private ITestOutputHelper _output;
    private DebugGraphicsHandler _graphics;
    
    private readonly Planet.PlanetTemplate _testPlanetTemplate = new(new Material(0, 0, 0));
    private readonly Planet _circularBody;
    private ConicPath ConicPath => _circularBody.OrbitPath.Conics[0];
    
    public ConicPath_Tests(ITestOutputHelper output)
    {
        KinematicObject.KinematicObjectTemplate.DestroyAll();
        _output = output;
        DebugGraphicsHandler debugGraphics = new();
        OrbitGame.Graphics = debugGraphics;
        _graphics = debugGraphics;
        Constants.SetGravitationalConstant((SDecimal)1);
        var parent = _testPlanetTemplate.CreateInstance("Test Parent", 
            new SpatialInfo(Vec2<SDecimal>.Zero, Vec2<SDecimal>.Zero), 1, 0, Color.White, null, 0);
        _circularBody = _testPlanetTemplate.CreateInstance("Circular Orbit Body", new SpatialInfo(
            new Vec2<SDecimal>(0, 1),
            new Vec2<SDecimal>(Math.Sqrt(1 + 0), 0)
        ), 1, 0, Color.White, parent, 0);
    }

    [Fact]
    public void ConicPath_DrawMethod()
    {
        ConicPath.Draw();
        Assert.Equal(1, _graphics.DrawMeshCalls.Count);
    }

    [Fact]
    public void ConicPath_MouseDownEvent()
    {
        
    }

    [Fact]
    public void ConicPath_MouseHoverEvent()
    {
        
    }

    [Fact]
    public void ConicPath_UpdateFrameEvent()
    {
        
    }
}