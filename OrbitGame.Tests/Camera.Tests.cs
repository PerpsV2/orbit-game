namespace OrbitGame.Tests;

public class Camera_Tests
{
    private readonly Camera _camera = new Camera(
        new Vector2(
            -new ScientificDecimal(10), 
            new ScientificDecimal(11)
            ), Math.PI / 4,
        new ScientificDecimal(4, 10), 
        new ScientificDecimal(2, 10),
        1000, 600
    );
    
    [Fact]
    public void Camera_ViewMatrixMethod()
    {
        Matrix3X3 expected = new Matrix3X3([
            new ScientificDecimal(1.767766952966370000000000m, -8),
            new ScientificDecimal(-1.767766952966370000000000m, -8),
            new ScientificDecimal(5, 2),
            new ScientificDecimal(-2.12132034355964400000000m, -8),
            new ScientificDecimal(-2.12132034355964400000000m, -8),
            new ScientificDecimal(3, 2),
            0, 0, 1
        ]);
        
        _camera.UpdateViewMatrix();
        AssertExtensions.Equal(expected, _camera.ViewMatrix);
    }

    [Fact]
    public void Camera_SD_ConvertToScreenCoordinatesMethod()
    {
        Vector2 point = new Vector2(
            new ScientificDecimal(-1, 10),
            new ScientificDecimal(1, 11)
        );
        
        AssertExtensions.Equal(new Vector2(500, 300), _camera.SD_ConvertToScreenCoordinates(point));
    }

    [Fact]
    public void Camera_SD_ConvertToScreenDistanceMethod()
    {
        AssertExtensions.Equal(250, _camera.SD_ConvertToScreenDistance(new ScientificDecimal(10)));
        AssertExtensions.Equal(300, _camera.SD_ConvertToScreenDistance(new ScientificDecimal(10), false));
    }
}