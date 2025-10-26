using SkiaSharp;

namespace OrbitGame;

public class Planet(ScientificDecimal mass, Vector2 position, Vector2 velocity, ScientificDecimal radius, SKColor colour, string name) 
    : Body(mass, position, velocity, name)
{
    private readonly ScientificDecimal Radius = radius;
    private readonly SKColor Colour = colour;

    public void Draw(SKCanvas canvas, Camera camera)
    {
        SKPaint paint = new SKPaint
        {
            Color = Colour
        };
        canvas.GS_DrawCircle(camera, Position, Radius, paint);
    }
}