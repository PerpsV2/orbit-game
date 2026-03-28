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
    private readonly Planet _testPlanet;
    private readonly Camera _testFullPlanetCamera;
    private readonly Camera _testSurfaceCamera;
    private readonly Camera _testMarkerCamera;

    public Planet_Tests(ITestOutputHelper output)
    {
        DebugGraphicsHandler debugGraphics = new();
        OrbitGame.Graphics = debugGraphics;
        _graphics = debugGraphics;
        _output = output;
        _testPlanet = _testPlanetTemplate.CreateInstance("Test Planet", new(Vec2<SDecimal>.Zero, Math.PI / 2), 
            1, 10000, Color.White, null);
        
        SpatialInfo cameraSpatialInfo = new(position: new Vec2<SDecimal>(10000, 0));
        ICameraMovementScheme surfaceCameraMovement = new TrackingCameraScheme(cameraSpatialInfo, null);
        _testSurfaceCamera = new Camera("Test Surface Camera", cameraSpatialInfo, 16, 9, 1600, 900,
            surfaceCameraMovement);

        cameraSpatialInfo = new(position: Vec2<SDecimal>.Zero);
        ICameraMovementScheme markerCameraMovement = new TrackingCameraScheme(cameraSpatialInfo, null);
        _testMarkerCamera = new Camera("Test Marker Camera", cameraSpatialInfo, 16000000, 9000000, 1600, 900,
            markerCameraMovement);

        cameraSpatialInfo = new(position: Vec2<SDecimal>.Zero);
        ICameraMovementScheme fullPlanetCameraMovement = new TrackingCameraScheme(cameraSpatialInfo, null);
        _testFullPlanetCamera = new Camera("Test Full Planet Camera", cameraSpatialInfo, 16000, 9000, 1600, 900,
            fullPlanetCameraMovement);
    }
    
    [Fact]
    public void Planet_DrawMethod()
    {
        OrbitGame.Camera = _testFullPlanetCamera;
        _testPlanet.Draw();
        Assert.Equal(1, _graphics.DrawMeshCalls.Count);
        _graphics.ResetCalls();

        OrbitGame.Camera = _testSurfaceCamera;
        _testPlanet.Draw();
        Assert.Equal(1, _graphics.DrawPolyCalls.Count);
        _graphics.ResetCalls();

        OrbitGame.Camera = _testMarkerCamera;
        _testPlanet.Draw();
        Assert.Equal(4, _graphics.DrawLineCalls.Count);
        _graphics.ResetCalls();
    }
    
    [Fact]
    public void Planet_DrawColliderMethod()
    {
        _testPlanet.DrawCollider();
        throw new NotImplementedException();
    }
}