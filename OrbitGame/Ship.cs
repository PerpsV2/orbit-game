using SkiaSharp;
namespace OrbitGame;

public class Ship : Body
{
    private readonly Vector2[] _mesh;
    
    public Ship(
        ScientificDecimal mass, Vector2 position, Vector2 velocity, Material material, SKColor colour, Body? parent,
        Vector2[] mesh, string name)
        : base(mass, position, velocity, colour, name, parent)
    {
        LinkedList<int> colliderIndices = Vector2.GetConvexHullIndices(mesh);
        _mesh = colliderIndices.Select(x => mesh[x]).ToArray();
        Collider = new ConvexCollider(_mesh, this, material);
    }

    public override void Draw(SKCanvas canvas, Camera camera)
    {
        using SKPaint paint = new SKPaint();
        paint.Color = Colour;
        paint.StrokeWidth = 4;

        Vector2[] polyPoints = _mesh.Select(ObjectToWorldSpace).ToArray();
        canvas.GS_DrawPoly(camera, polyPoints, paint);
        
        SKPoint screenPosition = camera.ConvertToScreenCoordinates(Position);
        canvas.DrawLine(screenPosition, screenPosition + new SKPoint(10, 10), paint);
        canvas.DrawLine(screenPosition, screenPosition + new SKPoint(10, -10), paint);
        canvas.DrawLine(screenPosition, screenPosition + new SKPoint(-10, -10), paint);
        canvas.DrawLine(screenPosition, screenPosition + new SKPoint(-10, 10), paint);
    }

    public override void DrawCollider(SKCanvas canvas, Camera camera)
    {
        using SKPaint paint = new SKPaint();
        paint.Color = Colour;
        paint.StrokeWidth = 4;

        Vector2[] polyPoints = _mesh.Select(ObjectToWorldSpace).ToArray();
        canvas.GS_DrawPoly(camera, polyPoints, paint, false);
    }
}