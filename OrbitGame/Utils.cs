using SkiaSharp;

namespace OrbitGame;

public static class Constants
{
    public static readonly ScientificDecimal G = new(6.6743m, -11);
}

public enum RotationDirection
{
    Counterclockwise = 1,
    None = 0,
    Clockwise = -1
};

public enum NumericalIntegrator
{
    ExplicitEuler,
    ImplicitEuler,
    RungeKutta4
}

public static class Utils
{
    public static void LogEnumerable<T>(IEnumerable<T> enumerable)
    {
        var array = enumerable as T[] ?? enumerable.ToArray();
        for (int i = 0; i < array.Length; i++)
            Console.Write($"{array.ElementAt(i)} ");
        Console.WriteLine();
    }
    
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T> 
        => value.CompareTo(max) > 0 ? max : value.CompareTo(min) < 0 ? min : value;
    
    public static double UnsignedMod(double a, double b)
        => a - b * Math.Floor(a / b);
    
    public static decimal DecimalSqrt(decimal x, decimal epsilon = 0.0M)
    {
        if (x < 0) throw new ArithmeticException("Cannot calculate square root from a negative number");

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

    public static ScientificDecimal CalculateTriangleArea(Vector2 a, Vector2 b, Vector2 c)
    {
        return (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)).Abs()/ 2;
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
    
    public static void GS_DrawEllipseOrbit(
        this SKCanvas canvas, 
        Camera camera, 
        Vector2 centre, 
        ScientificDecimal semiMajorAxis, 
        ScientificDecimal semiMinorAxis,
        double periapsisArgument,
        SKPaint paint)
    {
        SKPoint screenPosition = camera.ConvertToScreenCoordinates(centre);
        SKSize screenRadius = new SKSize(camera.ConvertToScreenDistance(semiMajorAxis), 
            camera.ConvertToScreenDistance(semiMinorAxis, false));
        paint.Style = SKPaintStyle.Stroke;
        canvas.RotateRadians(-(float)(periapsisArgument + camera.Angle), screenPosition.X, screenPosition.Y);
        canvas.DrawOval(screenPosition, screenRadius, paint);
        canvas.ResetMatrix();
    }

    public static void GS_DrawAABB(
        this SKCanvas canvas,
        Camera camera,
        Vector2 topRight,
        Vector2 bottomLeft,
        SKPaint paint)
    {
        SKPoint topRightScreenPosition = camera.ConvertToScreenCoordinates(new(topRight.X, topRight.Y));
        SKPoint bottomLeftScreenPosition = camera.ConvertToScreenCoordinates(new(bottomLeft.X, bottomLeft.Y));
        
        SKPath path = new SKPath();
        path.MoveTo(topRightScreenPosition);
        path.LineTo(topRightScreenPosition.X, bottomLeftScreenPosition.Y);
        path.LineTo(bottomLeftScreenPosition);
        path.LineTo(bottomLeftScreenPosition.X, topRightScreenPosition.Y);
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
        //SKPath path = new SKPath();
        SKPoint vertex = camera.ConvertToScreenCoordinates(points[0]);
        canvas.DrawCircle(vertex, 5, paint);
    }
    
    public static void GS_DrawPath(
        this SKCanvas canvas,
        Camera camera,
        IList<Vector2> points,
        SKPaint paint
    )
    {
        SKPath path = new SKPath();
        path.MoveTo(camera.ConvertToScreenCoordinates(points[0]));
        for (int i = 1; i < points.Count; i++)
            path.LineTo(camera.ConvertToScreenCoordinates(points[i]));
        paint.Style = SKPaintStyle.Stroke;
        canvas.DrawPath(path, paint);
    }

    public static void GS_DrawLine(this SKCanvas canvas, Camera camera, Vector2 start, Vector2 end, SKPaint paint)
    {
        SKPoint screenStart = camera.ConvertToScreenCoordinates(start);
        SKPoint screenEnd = camera.ConvertToScreenCoordinates(end);
        canvas.DrawLine(screenStart, screenEnd, paint);
    }
    
    public static void GS_DrawLineR(this SKCanvas canvas, Camera camera, Vector2 start, Vector2 offset, SKPaint paint)
    {
        canvas.GS_DrawLine(camera, start, start + offset, paint);
    }
    
    public static void GS_DrawPoint(this SKCanvas canvas, Camera camera, Vector2 point, SKPaint paint)
    {
        SKPoint screenPoint = camera.ConvertToScreenCoordinates(point);
        canvas.DrawCircle(screenPoint, 5, paint);
    }
}