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

        // if the planet is too large to draw on screen as a circle, draw its intersection with the camera as a line
        if (camera.Height <= Radius * 100)
        {
            Vector2 screenPosition = camera.ConvertToScreenCoordinatesSD(position) - new Vector2(Options.ScreenSize.width, Options.ScreenSize.height) / 2;
            ScientificDecimal screenRadius = camera.ConvertToScreenDistanceSD(Radius);
            //Console.WriteLine($"Position: {screenPosition}, Radius: {screenRadius}");
            
            ScientificDecimal discriminantY = 1 - ScientificDecimal.Square(screenPosition.X / screenRadius);
            ScientificDecimal discriminantX = 1 - ScientificDecimal.Square(screenPosition.Y / screenRadius);

            ScientificDecimal? yIntercept2, xIntercept1, xIntercept2;
            ScientificDecimal? yIntercept1 = yIntercept2 = xIntercept1 = xIntercept2 = null;
            if (discriminantY >= 0)
            {
                yIntercept1 = screenPosition.Y + ScientificDecimal.Abs(screenRadius) * ScientificDecimal.Sqrt(discriminantY);
                yIntercept2 = screenPosition.Y - ScientificDecimal.Abs(screenRadius) * ScientificDecimal.Sqrt(discriminantY);
            }

            if (discriminantX >= 0)
            {
                xIntercept1 = screenPosition.X + ScientificDecimal.Abs(screenRadius) * ScientificDecimal.Sqrt(discriminantX);
                xIntercept2 = screenPosition.X - ScientificDecimal.Abs(screenRadius) * ScientificDecimal.Sqrt(discriminantX);
            }
            
            if (screenPosition.Y != 0)
            {
                ScientificDecimal slope = -screenPosition.X / screenPosition.Y;
                canvas.DrawLine(0, (float)slope * -Options.ScreenSize.width / 2 + Options.ScreenSize.height / 2, Options.ScreenSize.width,
                    (float)slope * Options.ScreenSize.width / 2 + Options.ScreenSize.height / 2, paint);
            }

            if ((yIntercept1 >= -Options.ScreenSize.height / 2 && yIntercept1 <= Options.ScreenSize.height / 2) ||
                (yIntercept2 >= -Options.ScreenSize.height / 2 && yIntercept2 <= Options.ScreenSize.height / 2) ||
                (xIntercept1 >= -Options.ScreenSize.width / 2 && xIntercept1 <= Options.ScreenSize.width / 2) ||
                (xIntercept2 >= -Options.ScreenSize.width / 2 && xIntercept2 <= Options.ScreenSize.width / 2))
            {
                Console.WriteLine($"INTERSECT x1:{xIntercept1} y1:{yIntercept1} x2:{xIntercept2} y2:{yIntercept2}");
            }
            else Console.WriteLine($"NO INTERSECT x1:{xIntercept1} y1:{yIntercept1} x2:{xIntercept2} y2:{yIntercept2}");
        }
    }
}