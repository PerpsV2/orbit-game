using SkiaSharp;
namespace OrbitGame;

public class Ship(
    ScientificDecimal mass,
    Vector2 position,
    Vector2 velocity,
    SKColor colour,
    Vector2[] mesh,
    string name)
    : Body(mass, position, velocity, name)
{
    private readonly LinkedList<int> _colliderIndices = Vector2.GetConvexHullIndices(mesh);
    private readonly Vector2[] _mesh = mesh;
    private readonly SKColor _colour = colour;

    /// <summary>
    /// initialize with position and velocity relative to a parent body
    /// </summary>
    public Ship(
        ScientificDecimal mass, Vector2 position, Vector2 velocity, SKColor colour, Body parent,
        Vector2[] mesh, string name)
        : this(mass, position, velocity, colour, mesh, name)
    {
        Position = parent.Position + position;
        Velocity = parent.Velocity + velocity;
    }

    public override void Draw(SKCanvas canvas, Camera camera)
    {
        SKPaint paint = new SKPaint
        {
            Color = _colour,
            StrokeWidth = 4
        };
        
        //canvas.GS_DrawRect(camera, Position + new Vector2(-1, 1), Position + new Vector2(1, -1), paint);
        Vector2[] polyPoints = _mesh.Select(x => x + Position).ToArray();
        canvas.GS_DrawPoly(camera, polyPoints, paint);
        
        SKPoint screenPosition = camera.ConvertToScreenCoordinates(Position);
        canvas.DrawLine(screenPosition, screenPosition + new SKPoint(10, 10), paint);
        canvas.DrawLine(screenPosition, screenPosition + new SKPoint(10, -10), paint);
        canvas.DrawLine(screenPosition, screenPosition + new SKPoint(-10, -10), paint);
        canvas.DrawLine(screenPosition, screenPosition + new SKPoint(-10, 10), paint);
    }

    public override void DrawCollider(SKCanvas canvas, Camera camera)
    {
        SKPaint paint = new SKPaint
        {
            Color = _colour,
            StrokeWidth = 4
        };
        
        Vector2[] convexHull = _colliderIndices.Select(x => _mesh[x]).ToArray();
        Vector2[] polyPoints = convexHull.Select(x => x + Position).ToArray();
        canvas.GS_DrawPoly(camera, polyPoints, paint, false);
    }
}