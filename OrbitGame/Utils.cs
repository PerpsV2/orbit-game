using System.Numerics;
using SkiaSharp;

namespace OrbitGame;

public static class Constants
{
    public static readonly ScientificDecimal G = new(6.6743m, -11);
}

public static class Utils
{
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
        (float x, float y) screenPosition = camera.ConvertToScreenCoordinates(centre);
        float screenRadiusX = (float)(radius / (camera.Right - camera.Left)) * Options.ScreenSize.width;
        float screenRadiusY = (float)(radius / (camera.Top - camera.Bottom)) * Options.ScreenSize.height;
        
        (float x, float y) cameraOriginScreenPosition = camera.ConvertToScreenCoordinates(camera.AbsolutePosition);
        SKMatrix rotation = SKMatrix.CreateRotation((float)camera.Rotation, cameraOriginScreenPosition.x, 
            cameraOriginScreenPosition.y);
        canvas.SetMatrix(rotation);
        
        canvas.DrawOval(screenPosition.x, screenPosition.y, screenRadiusX, screenRadiusY, paint);
        
        canvas.ResetMatrix();
    }

    public static void GS_DrawRect(
        this SKCanvas canvas,
        Camera camera,
        Vector2 topLeft,
        Vector2 bottomRight,
        SKPaint paint)
    {
        (float x, float y) topLeftScreenPosition = camera.ConvertToScreenCoordinates(topLeft);
        (float x, float y) bottomRightScreenPosition = camera.ConvertToScreenCoordinates(bottomRight);
        float w = Math.Abs(bottomRightScreenPosition.x - topLeftScreenPosition.x);
        float h = Math.Abs(bottomRightScreenPosition.y - topLeftScreenPosition.y);
        (float x, float y) cameraOriginScreenPosition = camera.ConvertToScreenCoordinates(camera.AbsolutePosition);
        SKMatrix rotation = SKMatrix.CreateRotation((float)camera.Rotation, cameraOriginScreenPosition.x,
            cameraOriginScreenPosition.y);
        canvas.SetMatrix(rotation);
        
        canvas.DrawRect(topLeftScreenPosition.x, topLeftScreenPosition.y, w, h, paint);
        
        canvas.ResetMatrix();
    }
}