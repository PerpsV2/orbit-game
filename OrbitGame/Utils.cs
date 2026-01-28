using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

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

    public static ScientificDecimal CalculateTriangleArea(SD_Vector2 a, SD_Vector2 b, SD_Vector2 c)
    {
        return (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)).Abs()/ 2;
    }

    public readonly static int CircleVertices = 400;
    private static VertexBuffer _circleVertexBuffer;
    private static IndexBuffer _circleIndexBuffer;
    
    public static void GenerateCircleBuffers(GraphicsDevice graphicsDevice)
    {
        var vertices = new VertexPositionColor[CircleVertices + 1];
        for (int i = 0; i < CircleVertices; i++)
        {
            double angle = i * Math.Tau / CircleVertices;
            vertices[i] = new VertexPositionColor(new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0), Color.White);
        }
        vertices[^1] = new VertexPositionColor(Vector3.Zero, Color.White);

        var indices = new int[CircleVertices * 3];
        for (int i = 0; i < CircleVertices; i++)
        {
            indices[i * 3] = CircleVertices;
            indices[i * 3 + 1] = i;
            indices[i * 3 + 2] = (i + 1) % CircleVertices;
        }
         
        _circleVertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.None);
        _circleIndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
         
        _circleVertexBuffer.SetData(vertices);
        _circleIndexBuffer.SetData(indices);
    }
    
    public static void GS_DrawCircle(
        this SpriteBatch spriteBatch, 
        Camera camera, 
        SD_Vector2 center, 
        ScientificDecimal radius, 
        Color colour)
    {
        Vector2 screenCenter = camera.ConvertToScreenCoordinates(center);
        float screenRadius = camera.ConvertToScreenDistance(radius);
        
        spriteBatch.GraphicsDevice.SetVertexBuffer(_circleVertexBuffer);
        spriteBatch.GraphicsDevice.Indices = _circleIndexBuffer;

        Effect effect = OrbitGame.CurrentEffect;
        effect.Parameters["projection"].SetValue(Matrix.CreateOrthographicOffCenter(
            0, Options.ScreenSize.width, Options.ScreenSize.height, 0, 
            0, 100));
        effect.Parameters["world"].SetValue(Matrix.CreateScale(new Vector3(screenRadius, screenRadius, 1)) * 
                       Matrix.CreateTranslation(new Vector3(screenCenter.X, screenCenter.Y, 0)));
        effect.Parameters["colour"].SetValue(colour.ToVector4());
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            spriteBatch.GraphicsDevice.DrawInstancedPrimitives(
                PrimitiveType.TriangleList, 0, 0, CircleVertices, _circleVertexBuffer.VertexCount
                );
        }
    }
    
    private static VertexBuffer _polyVertexBuffer;
    private static IndexBuffer _polyIndexBuffer;
    
    public static void DrawPoly(this SpriteBatch spriteBatch, Camera camera, List<Vector2> points, Color colour)
    {
        if (points.Count == 0) return;
        
        var vertices = new VertexPositionColor[points.Count];
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = new VertexPositionColor(new Vector3(points[i].X, points[i].Y, 0), Color.White);
        
        var indices = new int[(vertices.Length - 2) * 3];
        for (int i = 0; i < vertices.Length - 2; i++)
        {
            indices[i * 3] = 0;
            indices[i * 3 + 1] = i + 1;
            indices[i * 3 + 2] = (i + 2) % vertices.Length;
        }

        Effect effect = OrbitGame.CurrentEffect;
        effect.Parameters["projection"].SetValue(Matrix.CreateOrthographicOffCenter(
            0, Options.ScreenSize.width, Options.ScreenSize.height, 0, 
            0, 100));
        effect.Parameters["world"].SetValue(Matrix.CreateScale(new Vector3(1, 1, 1)) * 
                                            Matrix.CreateTranslation(new Vector3(0, 0, 0)));
        effect.Parameters["colour"].SetValue(colour.ToVector4());
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            spriteBatch.GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3
            );
        }
    }

    public static void GS_DrawPoly(this SpriteBatch spriteBatch, Camera camera, SD_Vector2[] points, Color colour)
    {
        DrawPoly(spriteBatch, camera, points.Select(camera.ConvertToScreenCoordinates).ToList(), colour);
    }
    
    /*
    public static void GS_DrawEllipseOrbit(
        this SpriteBatch canvas, 
        Camera camera, 
        SD_Vector2 centre, 
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
        this SpriteBatch canvas,
        Camera camera,
        SD_Vector2 topRight,
        SD_Vector2 bottomLeft,
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
        this SpriteBatch canvas,
        Camera camera,
        IList<SD_Vector2> points,
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
        this SpriteBatch canvas,
        Camera camera,
        IList<SD_Vector2> points,
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

    public static void GS_DrawLine(this SpriteBatch canvas, Camera camera, SD_Vector2 start, SD_Vector2 end, SKPaint paint)
    {
        SKPoint screenStart = camera.ConvertToScreenCoordinates(start);
        SKPoint screenEnd = camera.ConvertToScreenCoordinates(end);
        canvas.DrawLine(screenStart, screenEnd, paint);
    }
    
    public static void GS_DrawLineR(this SpriteBatch canvas, Camera camera, SD_Vector2 start, SD_Vector2 offset, SKPaint paint)
    {
        canvas.GS_DrawLine(camera, start, start + offset, paint);
    }
    
    public static void GS_DrawPoint(this SpriteBatch canvas, Camera camera, SD_Vector2 point, SKPaint paint)
    {
        SKPoint screenPoint = camera.ConvertToScreenCoordinates(point);
        canvas.DrawCircle(screenPoint, 5, paint);
    }*/
}