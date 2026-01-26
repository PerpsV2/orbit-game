using SkiaSharp;
namespace OrbitGame;

public class Camera
{
    private Vector2 _localPosition;
    private Vector2 _origin = Vector2.Zero;
    public Vector2 AbsolutePosition => _localPosition + _origin;

    private double _angle;
    public double Angle
    {
        get => Utils.UnsignedMod(_angle, Math.Tau);
        private set
        {
            _angle = value;
            UpdateViewMatrix();
        }
    }

    private ScientificDecimal _width;
    public ScientificDecimal Width
    {
        get => _width;
        private set
        {
            _width = value;
            UpdateViewMatrix(); 
        }
    }

    private ScientificDecimal _height;

    public ScientificDecimal Height
    {
        get => _height;
        private set
        {
            _height = value;
            UpdateViewMatrix();
        }
    }
    
    private readonly int _screenWidth;
    private readonly int _screenHeight;
    
    public ScientificDecimal Left => AbsolutePosition.X - Width * 0.5f;
    public ScientificDecimal Top => AbsolutePosition.Y - Height * 0.5f;
    public ScientificDecimal Right => AbsolutePosition.X + Width * 0.5f;
    public ScientificDecimal Bottom => AbsolutePosition.Y + Height * 0.5f;

    public Matrix3X3 ViewMatrix;
    
    public Camera(Vector2 position, double angle, ScientificDecimal width, ScientificDecimal height,
        int screenWidth, int screenHeight)
    {
        _localPosition = position;
        _width = width;
        _height = height;
        Angle = angle;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        UpdateViewMatrix();
    }

    public Camera(Vector2 position, double angle, ScientificDecimal width, ScientificDecimal height) :
        this(position, angle, width, height, Options.ScreenSize.width, Options.ScreenSize.height) { }
    
    public Camera(Vector2 position, ScientificDecimal width, ScientificDecimal height) :
        this(position, 0, width, height) { }

    public void MoveTo(Vector2 position) => _localPosition = position;
    
    public void MoveBy(Vector2 position) => _localPosition += position;

    public void MoveBy(ScientificDecimal distance, double angle)
        => _localPosition += new Vector2(distance * Math.Cos(angle), distance * Math.Sin(angle));

    public void SetRotation(float angle) => Angle = angle;
    
    public void RotateBy(float angle) => Angle += angle;
    
    public void ScaleZoom(ScientificDecimal scale)
    {
        Width *= scale;
        Height *= scale;
    }
    
    public void SetOrigin(Vector2 origin) => _origin = origin;
    
    public void UpdateViewMatrix()
    {
        ViewMatrix = Matrix3X3.Scale(_screenWidth / Width, _screenHeight / Height) *
                     Matrix3X3.Translation(Width / 2, Height / 2) *
                     Matrix3X3.Rotation(-Angle) *
                     Matrix3X3.Scale(1, -1);
    }
    
    public Vector2 SD_ConvertToScreenCoordinates(Vector2 point)
        => ViewMatrix * (point - AbsolutePosition);
    
    public SKPoint ConvertToScreenCoordinates(Vector2 point)
    {
        Vector2 transformedPoint = SD_ConvertToScreenCoordinates(point);
        return new((float)transformedPoint.X, (float)transformedPoint.Y);
    }

    public ScientificDecimal SD_ConvertToScreenDistance(ScientificDecimal distance, bool xAxis = true)
    {
        if (xAxis) return distance / (Right - Left) * _screenWidth;
        return distance / (Bottom - Top) * _screenHeight;
    }
    
    public float ConvertToScreenDistance(ScientificDecimal distance, bool xAxis = true)
    {
        if (xAxis) return (float)SD_ConvertToScreenDistance(distance);
        return (float)SD_ConvertToScreenDistance(distance, false);
    }
}