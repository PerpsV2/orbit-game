using System.Numerics;
using SkiaSharp;

namespace OrbitGame;

public static class Constants
{
    public static readonly ScientificDecimal G = new(6.6743m, -11);
}

public enum RotationDirection
{
    Counterclockwise = -1,
    None = 0,
    Clockwise = 1
};

public static class Utils
{
    public static float UnsignedMod(float a, float b)
    {
        return a - b * (float)Math.Floor(a / b);
    }
    
    public static decimal DecimalSqrt(decimal x, decimal epsilon = 0.0M)
    {
        if (x < 0) throw new OverflowException("Cannot calculate square root from a negative number");

        decimal current = (decimal)Math.Sqrt((double)x), previous;
        do
        {
            previous = current;
            if (previous == 0.0M) return 0;
            current = (previous + x / previous) / 2;
        }
        while (Math.Abs(previous - current) > epsilon);
        return current;
    }

    public static void GS_DrawCircle(
        this SKCanvas canvas, 
        Camera camera, 
        Vector2 centre, 
        ScientificDecimal radius, 
        SKPaint paint)
    {
        SKPoint screenPosition = camera.ConvertToScreenCoordinates(centre);
        SKSize screenRadius = new SKSize(camera.ConvertToScreenDistance(radius), 
            camera.ConvertToScreenDistance(radius, false));
        
        canvas.DrawOval(screenPosition, screenRadius, paint);
    }

    public static void GS_DrawRect(
        this SKCanvas canvas,
        Camera camera,
        Vector2 topLeft,
        Vector2 bottomRight,
        SKPaint paint)
    {
        SKPoint topLeftScreenPosition = camera.ConvertToScreenCoordinates(topLeft);
        SKPoint bottomRightScreenPosition = camera.ConvertToScreenCoordinates(bottomRight);
        SKPoint topRightScreenPosition = camera.ConvertToScreenCoordinates(new(bottomRight.X, topLeft.Y));
        SKPoint bottomLeftScreenPosition = camera.ConvertToScreenCoordinates(new(topLeft.X, bottomRight.Y));
        
        SKPath path = new SKPath();
        path.MoveTo(topLeftScreenPosition);
        path.LineTo(topRightScreenPosition);
        path.LineTo(bottomRightScreenPosition);
        path.LineTo(bottomLeftScreenPosition);
        path.Close();
        canvas.DrawPath(path, paint);
    }

    public static void GS_DrawPoly(
        this SKCanvas canvas,
        Camera camera,
        IList<Vector2> points,
        SKPaint paint,
        bool filled = true
        )
    {
        if (points.Count < 3)
            throw new ArgumentException("A minimum of three points should be provided when drawing a polygon");
        SKPath path = new SKPath();
        path.MoveTo(camera.ConvertToScreenCoordinates(points[0]));
        for (int i = 1; i < points.Count; i++)
            path.LineTo(camera.ConvertToScreenCoordinates(points[i]));
        paint.Style = filled ? SKPaintStyle.Fill : SKPaintStyle.Stroke;
        path.Close();
        canvas.DrawPath(path, paint);
    }
}