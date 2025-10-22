namespace OrbitGame;

public delegate void CameraEventHandler(object sender, EventArgs e);

public class Camera(Vector2 position, ScientificDecimal width, ScientificDecimal height)
{
    private Vector2 _localPosition = position;
    private Vector2 _origin = Vector2.Zero;
    public Vector2 Position => _localPosition + _origin;
    public ScientificDecimal Width { get; private set; } = width;
    public ScientificDecimal Height { get; private set; } = height;
    public ScientificDecimal Left => Position.X - Width * 0.5f;
    public ScientificDecimal Top => Position.Y - Height * 0.5f;
    public ScientificDecimal Right => Position.X + Width * 0.5f;
    public ScientificDecimal Bottom => Position.Y + Height * 0.5f;

    public void MoveTo(Vector2 position) => _localPosition = position;
    
    public void MoveBy(Vector2 position) => _localPosition += position;
    
    public void ScaleZoom(ScientificDecimal scale)
    {
        Width *= scale;
        Height *= scale;
    }
    
    public void SetOrigin(Vector2 origin) => _origin = origin;
}