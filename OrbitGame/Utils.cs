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

    public static bool IntervalIntersects<T>(T value, T lowerBound, T upperBound) where T : IComparable<T>
    {
        return value.CompareTo(lowerBound) >= 0 && value.CompareTo(upperBound) <= 0;
    }
    
    public static ScientificDecimal IntervalPenetrationDistance(ScientificDecimal l1, ScientificDecimal u1,
        ScientificDecimal l2, ScientificDecimal u2)
        => !IntervalIntersects(l1, u1, l2, u2) ? 0 : u2 - l1 > u1 - l2 ? -u1 + l2 : u2 - l1;

    public static (ScientificDecimal start, ScientificDecimal end)? GetIntervalIntersection
        (ScientificDecimal s1, ScientificDecimal e1, ScientificDecimal s2, ScientificDecimal e2)
    {
        if (s2 > e1 || s1 > e2) return null;
        ScientificDecimal start = ScientificDecimal.Max(s1, s2);
        ScientificDecimal end = ScientificDecimal.Min(e1, e2);
        return (start, end);
    }

    public static bool IntervalIntersects<T>(T l1, T u1, T l2, T u2) where T : IComparable<T>
    {
        return IntervalIntersects(l1, l2, u2) || 
               IntervalIntersects(u1, l2, u2) || 
               (l1.CompareTo(l2) <= 0 && u1.CompareTo(u2) >= 0);
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