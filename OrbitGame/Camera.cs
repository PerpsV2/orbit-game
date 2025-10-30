namespace OrbitGame;

public class Camera(Vector2 position, ScientificDecimal width, ScientificDecimal height)
{
    public Camera(Vector2 position, float rotation, ScientificDecimal width, ScientificDecimal height) :
        this(position, width, height)
    {
        Rotation = rotation;
    }
    
    private Vector2 _localPosition = position;
    private Vector2 _origin = Vector2.Zero;
    public Vector2 AbsolutePosition => _localPosition + _origin;

    private double _rotation = 0;
    public double Rotation
    {
        get => _rotation % Math.Tau;
        private set => _rotation = value;
    }

    public ScientificDecimal Width { get; private set; } = width;
    public ScientificDecimal Height { get; private set; } = height;
    public ScientificDecimal Left => AbsolutePosition.X - Width * 0.5f;
    public ScientificDecimal Top => AbsolutePosition.Y - Height * 0.5f;
    public ScientificDecimal Right => AbsolutePosition.X + Width * 0.5f;
    public ScientificDecimal Bottom => AbsolutePosition.Y + Height * 0.5f;

    public void MoveTo(Vector2 position) => _localPosition = position;
    
    public void MoveBy(Vector2 position) => _localPosition += position;

    public void MoveBy(ScientificDecimal distance, double angle)
        => _localPosition += new Vector2(distance * Math.Cos(angle), distance * Math.Sin(angle));

    public void RotateTo(double angle) => Rotation = angle % Math.Tau;
    
    public void RotateBy(double angle) => Rotation = (Rotation + angle) % Math.Tau;
    
    public void ScaleZoom(ScientificDecimal scale)
    {
        Width *= scale;
        Height *= scale;
    }
    
    public void SetOrigin(Vector2 origin) => _origin = origin;

    // TODO: take into account camera rotation when converting to screen coordinates
    public (float x, float y) ConvertToScreenCoordinates(Vector2 point)
    {
        return ((float)((point.X - Left) / (Right - Left)) * Options.ScreenSize.width,
            (float)((point.Y - Top) / (Bottom - Top)) * Options.ScreenSize.height);
    }
    
    public Vector2 ConvertToScreenCoordinatesSD(Vector2 point)
    {
        return new Vector2((point.X - Left)* Options.ScreenSize.width / (Right - Left) ,
            (point.Y - Top)* Options.ScreenSize.height / (Bottom - Top));
    }

    public float ConvertToScreenDistance(ScientificDecimal distance, bool xAxis = true)
    {
        if (xAxis) return (float)((distance - Left) / (Right - Left)) * Options.ScreenSize.width;
        return (float)((distance - Top) / (Bottom - Top)) * Options.ScreenSize.height;
    }

    public ScientificDecimal ConvertToScreenDistanceSD(ScientificDecimal distance, bool xAxis = true)
    {
        if (xAxis) return distance / (Right - Left) * Options.ScreenSize.width;
        return distance / (Bottom - Top) * Options.ScreenSize.height;
    }
}