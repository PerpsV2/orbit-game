using Microsoft.Xna.Framework;
using Xunit.Abstractions;

namespace OrbitGame.Tests;

public class KeplerOrbitPath_Tests
{
    private readonly ITestOutputHelper _output;

    private readonly DebugGraphicsHandler _graphics;
    private readonly (int width, int height) _screenSize = new(100, 100);
    private readonly OrbitMesh _orbitMesh;
    
    private readonly Camera _fullOrbitCamera;
    private readonly Camera _partialOrbitCamera;
    
    private readonly Planet.PlanetTemplate _testPlanetTemplate = new(new Material(0, 0, 0));
    private readonly Planet _circularBody;
    private readonly Planet _ellipticBody;
    private readonly Planet _hyperbolicBody;

    private readonly KeplerOrbitPoint _selectedOrbitPoint;

    public KeplerOrbitPath_Tests(ITestOutputHelper output)
    {
        _output = output;
        Constants.SetGravitationalConstant((SDecimal)1);
        _graphics = new DebugGraphicsHandler();
        OrbitGame.Graphics = _graphics;
        _orbitMesh = new OrbitMesh();
        var parent = _testPlanetTemplate.CreateInstance("Test Parent", 
            new SpatialInfo(DVector2<SDecimal>.Zero, DVector2<SDecimal>.Zero), 1, 0, Color.White, null);
        _circularBody = _testPlanetTemplate.CreateInstance("Circular Orbit Body", new SpatialInfo(
            new DVector2<SDecimal>(0, 1),
            new DVector2<SDecimal>(Math.Sqrt(1 + 0), 0)
        ), 1, 0, Color.White, parent);
        _ellipticBody = _testPlanetTemplate.CreateInstance("Elliptic Orbit Body", new SpatialInfo(
            new DVector2<SDecimal>(0, 1),
            new DVector2<SDecimal>(Math.Sqrt(1 + 0.96), 0)
        ), 1, 0, Color.White, parent);
        _hyperbolicBody = _testPlanetTemplate.CreateInstance("Hyperbolic Orbit Body", new SpatialInfo(
            new DVector2<SDecimal>(0, 1),
            new DVector2<SDecimal>(Math.Sqrt(1 + 2.6), 0)
        ), 1, 0, Color.White, parent);
        
        _circularBody.GenerateKeplerianOrbit(0);
        _ellipticBody.GenerateKeplerianOrbit(0);
        _hyperbolicBody.GenerateKeplerianOrbit(0);

        _selectedOrbitPoint = new KeplerOrbitPoint(_circularBody.KeplerOrbitPath, 0);

        SpatialInfo cameraSpatialInfo = new(position: DVector2<SDecimal>.Zero);
        ICameraMovementScheme cameraMovementScheme = new TrackingCameraScheme(cameraSpatialInfo, parent);
        _fullOrbitCamera = new Camera("Full Orbit Camera", cameraSpatialInfo, 2, 2, 
            _screenSize.width, _screenSize.height, cameraMovementScheme);
        
        cameraSpatialInfo = new(position: new DVector2<SDecimal>(0, -49));
        cameraMovementScheme = new TrackingCameraScheme(cameraSpatialInfo, parent);
        _partialOrbitCamera = new Camera("Partial Orbit Camera", cameraSpatialInfo, 1, 1, 
            _screenSize.width, _screenSize.height, cameraMovementScheme);
    }

    [Fact]
    public void KeplerOrbitPath_DrawMethod()
    {
        OrbitGame.Camera = _fullOrbitCamera;
        _circularBody.KeplerOrbitPath.Draw(_orbitMesh, Color.White);
        Assert.Equal(1, _graphics.DrawMeshCalls.Count);
        Assert.Equal(0, _graphics.DrawPointCalls.Count);
        _graphics.ResetCalls();
        KeplerOrbitPath.SelectedPoint = _selectedOrbitPoint;
        _circularBody.KeplerOrbitPath.Draw(_orbitMesh, Color.White);
        Assert.Equal(1, _graphics.DrawPointCalls.Count);
        _graphics.ResetCalls();
        KeplerOrbitPath.HoverPoint = _selectedOrbitPoint;
        _circularBody.KeplerOrbitPath.Draw(_orbitMesh, Color.White);
        Assert.Equal(2, _graphics.DrawPointCalls.Count);
        _graphics.ResetCalls();

        OrbitGame.Camera = _partialOrbitCamera;
        _ellipticBody.KeplerOrbitPath.Draw(_orbitMesh, Color.White);
        Assert.True(_graphics.DrawLineCalls.Count > 0);
        _graphics.ResetCalls();

        OrbitGame.Camera = _fullOrbitCamera;
        _hyperbolicBody.KeplerOrbitPath.Draw(_orbitMesh, Color.White);
        Assert.True(_graphics.DrawLineCalls.Count > 0);
        _graphics.ResetCalls();
    }

    [Fact]
    public void KeplerOrbitPath_MouseEvents()
    {
        
    }
}