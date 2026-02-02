using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

namespace OrbitGame;

public static class Constants
{
    public static readonly ScientificDecimal G = new(6.6743, -11);
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
    
    public static double DecimalSqrt(double x, double epsilon = 0.0)
    {
        if (x < 0) throw new ArithmeticException("Cannot calculate square root from a negative number");

        double current = Math.Sqrt(x), previous;
        do
        {
            previous = current;
            if (previous == 0.0) return 0;
            current = (previous + x / previous) / 2;
        }
        while (Math.Abs(previous - current) > epsilon);
        return current;
    }

    public static ScientificDecimal CalculateTriangleArea(SD_Vector2 a, SD_Vector2 b, SD_Vector2 c)
    {
        return (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)).Abs()/ 2;
    }
    
    public static ScientificDecimal CalculateConvexInertia(SD_Vector2[] points, ScientificDecimal mass)
    {
        var triangles = SD_Vector2.TriangulateConvex(points);
        ScientificDecimal totalArea = triangles.Aggregate(new ScientificDecimal(0),
            (a, t) => a + CalculateTriangleArea(t.a, t.b, t.c));
        
        ScientificDecimal[] masses = new ScientificDecimal[triangles.Length];
        SD_Vector2[] centroids = new SD_Vector2[triangles.Length];
        ScientificDecimal[] inertias = new ScientificDecimal[triangles.Length];
        for (int i = 0; i < triangles.Length; ++i)
        {
            SD_Vector2 a = triangles[i].a;
            SD_Vector2 b = triangles[i].b;
            SD_Vector2 c = triangles[i].c;
            
            masses[i] = mass / totalArea * Utils.CalculateTriangleArea(a, b, c);
            centroids[i] = (a + b + c) / 3;
            inertias[i] = masses[i] * (SD_Vector2.Dot(a, a) + SD_Vector2.Dot(b, b) + SD_Vector2.Dot(c, c) +
                                       SD_Vector2.Dot(c, c) + SD_Vector2.Dot(a, b) + SD_Vector2.Dot(b, c) + 
                                       SD_Vector2.Dot(c, a))/ 6;
        }

        ScientificDecimal totalInertia = 0;
        for (int i = 0; i < triangles.Length; ++i)
            totalInertia += inertias[i] + masses[i] * (centroids[i].X.Square() + centroids[i].Y.Square());
        
        return totalInertia;
    }
    
    public static void DrawPoly(GraphicsDevice graphicsDevice, Camera camera, Effect effect, List<Vector2> points, 
        Color colour)
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

        effect.Parameters["projection"].SetValue(Matrix.CreateOrthographicOffCenter(
            0, Options.ScreenSize.width, Options.ScreenSize.height, 0, 
            0, 100));
        effect.Parameters["world"].SetValue(Matrix.CreateScale(new Vector3(1, 1, 1)) * 
                                            Matrix.CreateTranslation(new Vector3(0, 0, 0)));
        effect.Parameters["colour"].SetValue(colour.ToVector4());
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3
            );
        }
    }

    public static void GS_DrawPath(GraphicsDevice graphicsDevice, Camera camera, Effect effect, List<SD_Vector2> points, 
        Color colour, bool closed = false)
    {
        List<Vector2> screenPoints = points.Select(camera.ConvertToScreenCoordinates).ToList();
        
        var vertices = new VertexPositionColor[screenPoints.Count];
        for (int i = 0; i < vertices.Length - 1; ++i)
            vertices[i] = new VertexPositionColor(new Vector3(screenPoints[i].X, screenPoints[i].Y, 0), Color.White);
        
        var indices = new int[vertices.Length + (closed ? 1 : 0)];
        for (int i = 0; i < indices.Length - 1; ++i)
            indices[i] = i;
        if (closed) indices[^1] = 0;
        
        effect.Parameters["projection"].SetValue(Matrix.CreateOrthographicOffCenter(
            0, Options.ScreenSize.width, Options.ScreenSize.height, 0, 
            0, 100));
        effect.Parameters["world"].SetValue(Matrix.CreateScale(new Vector3(1, 1, 1)) * 
                                            Matrix.CreateTranslation(new Vector3(0, 0, 0)));
        effect.Parameters["colour"].SetValue(colour.ToVector4());
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.LineStrip, vertices, 0, vertices.Length, indices, 0, indices.Length - 2
            );
        }
    }

    public static void DrawLine(this GraphicsDevice graphicsDevice, Vector2 start, Vector2 end, Color colour)
    {
        VertexPositionColor[] vertices = [new(new Vector3(start.X, start.Y, 0), Color.White), new(new Vector3(end.X, end.Y, 0), Color.White)];
        int[] indices = [0, 1];
        
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
            graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.LineList, vertices, 0, vertices.Length, indices, 0, 1
            );
        }
    }
}