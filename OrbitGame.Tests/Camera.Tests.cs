using System;
using System.Numerics;
using Xunit;

namespace OrbitGame.Tests;

public class Camera_Tests
{
    private class TestKinematicObject(string identifier, SpatialInfo spatialInfo, Action<string> destructor)
        : KinematicObject(identifier, spatialInfo, destructor)
    {
        public class TestTemplate : KinematicObjectTemplate
        {
            public TestKinematicObject CreateTestInstance(string identifier, SpatialInfo spatialInfo)
            {
                TestKinematicObject instance = new TestKinematicObject(identifier, spatialInfo, DestroyInstance);
                AddInstance(identifier, instance);
                return instance;
            }
            
            public new static Dictionary<string, TestKinematicObject> AllInstances { get; } = new();
            
            protected override void AddInstance(string identifier, KinematicObject instance)
            {
                base.AddInstance(identifier, instance);
                if (!AllInstances.TryAdd(identifier, (TestKinematicObject)instance))
                    throw new ArgumentException($"KinematicObject with identifier '{identifier}' has already been added");
            }

            public override void DestroyInstance(string identifier)
            {
                base.DestroyInstance(identifier);
                AllInstances.Remove(identifier);
                Instances.Remove(identifier);
            }
        }
    }
    
    private readonly (int width, int height) _testScreenSize = (1600, 900);
    private readonly Camera _testSquareCamera;
    private readonly Camera _testCamera;
    private readonly SpatialInfo _testCameraSpatialInfo = new(new Vec2<SDecimal>(0, 10), Math.PI);
    
    private readonly KinematicObject _testTrackingObject;

    private readonly TrackingCameraScheme _testTrackingCameraScheme;
    private readonly TrackingFixedCameraScheme _testTrackingFixedCameraScheme;
    private readonly SurfaceCameraScheme _testSurfaceCameraScheme;

    private readonly SDecimal _testCameraZoomScale = new(10);
    
    public Camera_Tests()
    {
        KinematicObject.KinematicObjectTemplate.DestroyAll();
        
        TestKinematicObject.TestTemplate testTemplate = new();
        _testTrackingObject = 
            testTemplate.CreateTestInstance("Tracking Object", new(new Vec2<SDecimal>(10, 0), Math.PI / 2));
        KinematicObject testSurfaceObject = 
            testTemplate.CreateTestInstance("Surface Object", new(Vec2<SDecimal>.Zero, 3 * Math.PI / 2));
        
        _testTrackingCameraScheme = new TrackingCameraScheme(_testCameraSpatialInfo, _testTrackingObject);
        _testTrackingFixedCameraScheme = new TrackingFixedCameraScheme(_testCameraSpatialInfo, _testTrackingObject);
        _testSurfaceCameraScheme = new SurfaceCameraScheme(_testCameraSpatialInfo, testSurfaceObject, _testTrackingObject);
        
        _testSquareCamera = new Camera("Test Square Camera", _testCameraSpatialInfo, 10, 10, 
            _testScreenSize.width, _testScreenSize.height, _testTrackingCameraScheme);
        _testCamera = new Camera("Test Camera", _testCameraSpatialInfo, 16, 9, 
            _testScreenSize.width, _testScreenSize.height, _testTrackingCameraScheme);
    }

    [Fact]
    public void Camera_Constructor()
    {
        Assert.Equal(16, _testCamera.Width);
        Assert.Equal(9, _testCamera.Height);
        Assert.Equal(new(8, 14.5), _testCamera.BottomLeft);
        Assert.Equal(new(-8, 14.5), _testCamera.BottomRight);
        Assert.Equal(new(8, 5.5), _testCamera.TopLeft);
        Assert.Equal(new(-8, 5.5), _testCamera.TopRight);
        Assert.Equal(337, _testCamera.MaximumRadiusSquared, Assert.Epsilon);
        Assert.Equal(Math.Sqrt(337), _testCamera.MaximumRadius, Assert.Epsilon);
    }

    [Fact]
    public void Camera_ScaleZoomMethod()
    {
        _testCamera.ScaleZoom(_testCameraZoomScale);
        Assert.Equal(new SDecimal(16, 10), _testCamera.Width);
        Assert.Equal(new SDecimal(9, 10), _testCamera.Height);
        
        _testSquareCamera.ScaleZoom(_testCameraZoomScale);
        Assert.Equal(new SDecimal(10, 10), _testSquareCamera.Width);
        Assert.Equal(new SDecimal(10, 10), _testSquareCamera.Height);
    }

    [Fact]
    public void Camera_SD_ConvertToWorldCoordinatesMethod()
    {
        Assert.Equal(new Vec2<SDecimal>(-8, 14.5),
            _testCamera.SD_ConvertToWorldCoordinates(new Vec2<SDecimal>(1600, 900)));
        Assert.Equal(new Vec2<SDecimal>(-5, 15), 
            _testSquareCamera.SD_ConvertToWorldCoordinates(new Vec2<SDecimal>(1600, 900)));
    }

    [Fact]
    public void Camera_ConvertToWorldCoordinatesMethod()
    {
        Assert.Equal(new Vec2<SDecimal>(-5, 5), _testSquareCamera.ConvertToWorldCoordinates(new Vector2(1600, 0)));
    }

    [Fact]
    public void Camera_SD_ConvertToScreenCoordinatesMethod()
    {
        Assert.Equal(new Vec2<SDecimal>(1600, 900), _testCamera.SD_ConvertToScreenCoordinates(new Vec2<SDecimal>(-8, 14.5)));
    }

    [Fact]
    public void Camera_ConvertToScreenCoordinatesMethod()
    {
        Assert.Equal(new Vector2(1600, 0), _testSquareCamera.ConvertToScreenCoordinates(new Vec2<SDecimal>(-5, 5)));
    }

    [Fact]
    public void Camera_SD_ConvertToScreenDistanceMethod()
    {
        Assert.Equal(1600, _testCamera.SD_ConvertToScreenDistance(16));
        Assert.Equal(1600, _testCamera.SD_ConvertToScreenDistance(16, false));
        
        Assert.Equal(1600, _testSquareCamera.SD_ConvertToScreenDistance(10));
        Assert.Equal(900, _testSquareCamera.SD_ConvertToScreenDistance(10, false));
    }
    
    [Fact]
    public void Camera_ConvertToScreenDistanceMethod()
    {
        Assert.Equal(1600, _testSquareCamera.ConvertToScreenDistance(10));
        Assert.Equal(900, _testSquareCamera.ConvertToScreenDistance(10, false));
    }

    #region Tracking Camera Scheme
    
    [Fact]
    public void TrackingCameraScheme_FocusMethod()
    {
        _testCamera.MovementScheme = _testTrackingCameraScheme;
        _testCamera.Focus();
        _testCamera.Update();
        Assert.Equal(_testTrackingObject.Position, _testCamera.Position);
        Assert.Equal(Math.PI, _testCamera.Angle);
    }
    
    [Fact]
    public void TrackingCameraScheme_MoveParallelMethod()
    {
        _testCamera.MovementScheme = _testTrackingCameraScheme;
        _testCamera.MoveParallel(10);
        _testCamera.Update();
        Assert.Equal(new Vec2<SDecimal>(0, 0), _testCamera.Position);
        Assert.Equal(Math.PI, _testCamera.Angle);
    }

    [Fact]
    public void TrackingCameraScheme_MovePerpendicularMethod()
    {
        _testCamera.MovementScheme = _testTrackingCameraScheme;
        _testCamera.MovePerpendicular(10);
        _testCamera.Update();
        Assert.Equal(new Vec2<SDecimal>(-10, 10), _testCamera.Position);
        Assert.Equal(Math.PI, _testCamera.Angle);
    }
    
    [Fact]
    public void TrackingCameraScheme_RotateByMethod()
    {
        _testCamera.MovementScheme = _testTrackingCameraScheme;
        _testCamera.RotateBy(Math.PI);
        _testCamera.Update();
        Assert.Equal(new Vec2<SDecimal>(0, 10), _testCamera.Position);
        Assert.Equal(0, _testCamera.Angle);
    }
    
    #endregion
    
    #region Tracking Fixed Camera Scheme
    
    [Fact]
    public void TrackingFixedCameraScheme_FocusMethod()
    {
        _testCamera.MovementScheme = _testTrackingFixedCameraScheme;
        _testCamera.Focus();
        _testCamera.Update();
        Assert.Equal(_testTrackingObject.Position, _testCamera.Position);
        Assert.Equal(3 * Math.PI / 2, _testCamera.Angle);
    }
    
    [Fact]
    public void TrackingFixedCameraScheme_MoveParallelMethod()
    {
        _testCamera.MovementScheme = _testTrackingFixedCameraScheme;
        _testCamera.MoveParallel(10);
        _testCamera.Update();
        Assert.Equal(new Vec2<SDecimal>(0, 0), _testCamera.Position);
        Assert.Equal(Math.PI, _testCamera.Angle);
    }

    [Fact]
    public void TrackingFixedCameraScheme_MovePerpendicularMethod()
    {
        _testCamera.MovementScheme = _testTrackingFixedCameraScheme;
        _testCamera.MovePerpendicular(10);
        _testCamera.Update();
        Assert.Equal(new Vec2<SDecimal>(-10, 10), _testCamera.Position);
        Assert.Equal(Math.PI, _testCamera.Angle);
    }
    
    [Fact]
    public void TrackingFixedCameraScheme_RotateByMethod()
    {
        _testCamera.MovementScheme = _testTrackingFixedCameraScheme;
        _testCamera.RotateBy(Math.PI);
        _testCamera.Update();
        Assert.Equal(new Vec2<SDecimal>(0, 10), _testCamera.Position);
        Assert.Equal(0, _testCamera.Angle);
    }
    
    #endregion
    
    #region Surface Camera Scheme
    
    [Fact]
    public void SurfaceCameraScheme_FocusMethod()
    {
        _testCamera.MovementScheme = _testSurfaceCameraScheme;
        _testCamera.Focus();
        _testCamera.Update();
        Assert.Equal(_testTrackingObject.Position, _testCamera.Position);
        Assert.Equal(Math.PI / 2, _testCamera.Angle);
    }
    
    [Fact]
    public void SurfaceCameraScheme_MoveParallelMethod()
    {
        _testCamera.MovementScheme = _testSurfaceCameraScheme;
        _testCamera.MoveParallel(10);
        _testCamera.Update();
        Assert.Equal(new Vec2<SDecimal>(0, 20), _testCamera.Position);
        Assert.Equal(Math.PI, _testCamera.Angle);
        
        _testCamera.MoveParallel(-20);
        _testCamera.Update();
        Assert.Equal(new Vec2<SDecimal>(0, 0), _testCamera.Position);
        Assert.Equal(Math.PI / 2, _testCamera.Angle);
    }

    [Fact]
    public void SurfaceCameraScheme_MovePerpendicularMethod()
    {
        _testCamera.MovementScheme = _testSurfaceCameraScheme;
        _testCamera.MovePerpendicular(20 * Math.PI);
        _testCamera.Update();
        Assert.Equal(new Vec2<SDecimal>(0, 10), _testCamera.Position);
        Assert.Equal(Math.PI, _testCamera.Angle, Assert.Epsilon);
    }
    
    [Fact]
    public void SurfaceCameraScheme_RotateByMethod()
    {
        _testCamera.MovementScheme = _testSurfaceCameraScheme;
        _testCamera.RotateBy(Math.PI);
        _testCamera.Update();
        Assert.Equal(new Vec2<SDecimal>(0, 10), _testCamera.Position);
        Assert.Equal(0, _testCamera.Angle);
    }
    
    #endregion
}