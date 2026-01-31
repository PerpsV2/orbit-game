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
    
    public readonly static int EmptyCircleVertices = 400;
    private static VertexBuffer _emptyCircleVertexBuffer;
    private static IndexBuffer _emptyCircleIndexBuffer;
    
    public static void GenerateEmptyCircleBuffers(GraphicsDevice graphicsDevice)
    {
        var vertices = new VertexPositionColor[EmptyCircleVertices + 1];
        for (int i = 0; i < EmptyCircleVertices; i++)
        {
            double angle = i * Math.Tau / EmptyCircleVertices;
            vertices[i] = new VertexPositionColor(new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0), Color.White);
        }
        vertices[^1] = vertices[0];

        var indices = new int[vertices.Length];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;
         
        _emptyCircleVertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.None);
        _emptyCircleIndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
         
        _emptyCircleVertexBuffer.SetData(vertices);
        _emptyCircleIndexBuffer.SetData(indices);
    }

    public static void GS_DrawEllipseOrbit(
        this SpriteBatch spriteBatch, 
        Camera camera, 
        SD_Vector2 center, 
        ScientificDecimal semiMajorAxis, 
        ScientificDecimal semiMinorAxis, 
        double periapsisArgument,
        Color colour
        )
    {
        Vector2 screenPosition = camera.ConvertToScreenCoordinates(center);
        float screenMajorRadius = camera.ConvertToScreenDistance(semiMajorAxis);
        float screenMinorRadius = camera.ConvertToScreenDistance(semiMinorAxis);
        
        spriteBatch.GraphicsDevice.SetVertexBuffer(_emptyCircleVertexBuffer);
        spriteBatch.GraphicsDevice.Indices = _emptyCircleIndexBuffer;

        Effect effect = OrbitGame.CurrentEffect;
        effect.Parameters["projection"].SetValue(Matrix.CreateOrthographicOffCenter(
            0, Options.ScreenSize.width, Options.ScreenSize.height, 0, 
            0, 100));
        effect.Parameters["world"].SetValue(Matrix.CreateScale(new Vector3(screenMajorRadius, screenMinorRadius, 1)) * 
                                            Matrix.CreateRotationZ(-(float)periapsisArgument) * 
                                            Matrix.CreateTranslation(new Vector3(screenPosition.X, screenPosition.Y, 0)));
        effect.Parameters["colour"].SetValue(colour.ToVector4());
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            spriteBatch.GraphicsDevice.DrawInstancedPrimitives(
                PrimitiveType.LineStrip, 0, 0, EmptyCircleVertices, _emptyCircleVertexBuffer.VertexCount
            );
        }
    }
}