using SkiaSharp;
namespace OrbitGame;

public class Ship(ScientificDecimal mass, Vector2 position, Vector2 velocity, SKColor colour, string name) 
    : Body(mass, position, velocity, name)
{
    private readonly SKColor Colour = colour;
    
    /// <summary>
    /// initialize with position and velocity relative to a parent body
    /// </summary>
    public Ship(ScientificDecimal mass, Vector2 position, Vector2 velocity, SKColor colour, Body parent, string name) 
        : this(mass, position, velocity, colour, name)
    {
        Position = parent.Position + position;
        Velocity = parent.Velocity + velocity;
    }

    public override void Draw(SKCanvas canvas, Camera camera)
    {
        SKPaint paint = new SKPaint
        {
            Color = Colour,
            StrokeWidth = 4
        };
        
        canvas.GS_DrawRect(camera, Position + new Vector2(-1, 1), Position + new Vector2(1, -1), paint);
        (float x, float y) screenPosition = camera.ConvertToScreenCoordinates(Position);
        canvas.DrawLine(screenPosition.x, screenPosition.y, screenPosition.x + 10, screenPosition.y + 10, paint);
        canvas.DrawLine(screenPosition.x, screenPosition.y, screenPosition.x - 10, screenPosition.y + 10, paint);
        canvas.DrawLine(screenPosition.x, screenPosition.y, screenPosition.x + 10, screenPosition.y - 10, paint);
        canvas.DrawLine(screenPosition.x, screenPosition.y, screenPosition.x - 10, screenPosition.y - 10, paint);
    }
}