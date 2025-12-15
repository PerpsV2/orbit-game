using SkiaSharp;

namespace OrbitGame;

public class Planet : Body
{
    public readonly ScientificDecimal Radius;
    public readonly SKColor Colour;
    
    public Planet(ScientificDecimal mass, Vector2 position, Vector2 velocity, ScientificDecimal radius, SKColor colour, string name) : base(mass, position, velocity, name)
    {
        Radius = radius;
        Colour = colour;
        Collider = new CircularCollider(Radius, this);
    }

    public override void Draw(SKCanvas canvas, Camera camera)
    {
        SKPaint paint = new SKPaint
        {
            Color = Colour
        };
        
        // if the planet is too large to draw on screen as a circle, draw its intersection with the camera as a line
        if (camera.Height <= Radius / Options.SurfaceApproximationRadiusZoomFraction)
        {
            Vector2 screenPosition = camera.SD_ConvertToScreenCoordinates(Position);
            
            float h = Options.ScreenSize.height;
            float w = Options.ScreenSize.width;
            ScientificDecimal p1 = screenPosition.Y;
            ScientificDecimal p2 = screenPosition.X;
            ScientificDecimal r = camera.SD_ConvertToScreenDistance(Radius);

            ScientificDecimal topDiscriminant = 2 * h * p1 - p1 * p1 - h * h + r * r;
            ScientificDecimal bottomDiscriminant = r * r - p1 * p1;
            ScientificDecimal rightDiscriminant = 2 * w * p2 - p2 * p2 - w * w + r * r;
            ScientificDecimal leftDiscriminant = r * r - p2 * p2;
            ScientificDecimal radical;

            List<Vector2> intersectionPoints = new();
            
            if (topDiscriminant >= 0)
            {
                radical = ScientificDecimal.Sqrt(topDiscriminant);
                if (!(p2 - radical < 0 && p2 + radical < 0) && !(p2 - radical > w && p2 + radical > w))
                {
                    intersectionPoints.Add(new Vector2(ScientificDecimal.Clamp(p2 - radical, 0, w), h));
                    intersectionPoints.Add(new Vector2(ScientificDecimal.Clamp(p2 + radical, 0, w), h));
                }
            }
            if (rightDiscriminant >= 0)
            {
                radical = ScientificDecimal.Sqrt(rightDiscriminant);
                if (!(p1 - radical < 0 && p1 + radical < 0) && !(p1 - radical > h && p1 + radical > h))
                {
                    intersectionPoints.Add(new Vector2(w, ScientificDecimal.Clamp(p1 + radical, 0, h)));
                    intersectionPoints.Add(new Vector2(w, ScientificDecimal.Clamp(p1 - radical, 0, h)));
                }
            }
            if (bottomDiscriminant >= 0)
            {
                radical = ScientificDecimal.Sqrt(bottomDiscriminant);
                if (!(p2 - radical < 0 && p2 + radical < 0) && !(p2 - radical > w && p2 + radical > w))
                {
                    intersectionPoints.Add(new Vector2(ScientificDecimal.Clamp(p2 + radical, 0, w), 0));
                    intersectionPoints.Add(new Vector2(ScientificDecimal.Clamp(p2 - radical, 0, w), 0));
                }
            }
            if (leftDiscriminant >= 0)
            {
                radical = ScientificDecimal.Sqrt(leftDiscriminant);
                if (!(p1 - radical < 0 && p1 + radical < 0) && !(p1 - radical > h && p1 + radical > h))
                {
                    intersectionPoints.Add(new Vector2(0, ScientificDecimal.Clamp(p1 - radical, 0, h)));
                    intersectionPoints.Add(new Vector2(0, ScientificDecimal.Clamp(p1 + radical, 0, h)));
                }
            }

            if (intersectionPoints.Count == 0) return;
            
            intersectionPoints = intersectionPoints.GroupBy(z => z).Select(z => z.First()).ToList();

            SKPath path = new SKPath();
            path.MoveTo(new SKPoint((float)intersectionPoints[0].X, (float)intersectionPoints[0].Y));
            foreach (var point in intersectionPoints)
                path.LineTo(new SKPoint((float)point.X, (float)point.Y));
            path.Close();
            canvas.DrawPath(path, paint);
        }
        else canvas.GS_DrawCircle(camera, Position, Radius, paint);
    }

    public override void DrawCollider(SKCanvas canvas, Camera camera)
    {
        Draw(canvas, camera);
    }
}