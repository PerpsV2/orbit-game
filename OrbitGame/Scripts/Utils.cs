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
    VelocityVerlet,
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

    public static double WrapAngle(double angle)
        => UnsignedMod(angle, Math.Tau);

    public static void GetMinAngleRange(out double min, out double max, double angle, params double[] angles)
    {
        double[] sortedAngles = angles.Append(angle).OrderBy(WrapAngle).ToArray();
        double minRange = Math.Tau;
        min = angle;
        max = angle;
        for (int i = 0; i < sortedAngles.Length; i++)
        {
            double arcStartAngle = sortedAngles[i];
            double arcEndAngle = sortedAngles[(int)UnsignedMod(i - 1, sortedAngles.Length)];
            if (arcEndAngle < arcStartAngle) arcEndAngle += Math.Tau;
            double arcRange = arcEndAngle - arcStartAngle;
            if (arcRange < minRange)
            {
                minRange = arcRange;
                min = WrapAngle(arcStartAngle);
                max = WrapAngle(arcEndAngle);
            }
        }
    }

    public static void IterateAngleRange(double start, double end, double step, Action<double> action)
    {
        if (start > end) end += Math.Tau;
        for (double angle = start; angle <= end; angle += step)
            action(WrapAngle(angle));
    }
    
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
        var triangles = TriangulateConvex(points);
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
            
            masses[i] = mass / totalArea * CalculateTriangleArea(a, b, c);
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
    
    /// <summary>
    /// Decomposes the vertices of a convex polygon into triangles
    /// </summary>
    public static (SD_Vector2 a, SD_Vector2 b, SD_Vector2 c)[] TriangulateConvex(SD_Vector2[] points)
    {
        if (points.Length < 3) throw new ArgumentException("Convex shape must have at least 3 points.");
        var triangulation = new (SD_Vector2 a, SD_Vector2 b, SD_Vector2 c)[points.Length - 2];
        for (int i = 1; i < points.Length - 1; ++i)
            triangulation[i - 1] = (points[0], points[i], points[i + 1]);

        return triangulation;
    }
    
    /// <summary>
    /// Returns the center of mass from the vertices of a convex polygon
    /// </summary>
    public static SD_Vector2 CenterOfMassConvex(SD_Vector2[] points)
    {
        var triangles = TriangulateConvex(points);
        SD_Vector2 centerOfMass = SD_Vector2.Zero;
        foreach (var triangle in triangles)
        {
            SD_Vector2 centroid = (triangle.a + triangle.b + triangle.c) / 3;
            centerOfMass += centroid;
        }

        centerOfMass /= triangles.Length;

        return centerOfMass;
    }

    /// <summary>
    /// Re-centers a convex polygon at its center of mass
    /// </summary>
    public static SD_Vector2[] CenterConvex(SD_Vector2[] points)
        => points.Select(v => v - CenterOfMassConvex(points)).ToArray();

    /// <summary>
    /// Returns whether a set of three points is ordered clockwise or counter-clockwise
    /// </summary>
    public static RotationDirection TripletRotationDirection(SD_Vector2[] triplet)
    {
        if (triplet.Length != 3) throw new ArgumentException("Vector2 triplet must have exactly 3 values");
        double edgeSlope1 = (double)((triplet[1].Y - triplet[0].Y) * (triplet[2].X - triplet[0].X));
        double edgeSlope2 = (double)((triplet[2].Y - triplet[0].Y) * (triplet[1].X - triplet[0].X));
        return edgeSlope1 > edgeSlope2 ? RotationDirection.Clockwise :
            edgeSlope1 < edgeSlope2 ? RotationDirection.Counterclockwise : RotationDirection.None;
    }
    
    /// <summary>
    /// Returns the vertex order of the convex hull for the set of points given as a linked list
    /// </summary>
    public static LinkedList<int> GetConvexHullIndices(SD_Vector2[] points)
    {
        // get leftmost point to start
        int leftmostIndex = 0;
        for (int i = 0; i < points.Length; ++i)
            if (points[i].X < points[leftmostIndex].X) leftmostIndex = i;
        LinkedList<int> convexHull = new();
        convexHull.AddFirst(leftmostIndex);

        int currentIndex = leftmostIndex;
        do {
            int nextIndex = (currentIndex + 1) % points.Length;
            for (int i = 0; i < points.Length; ++i)
            {
                if (i == currentIndex || i == nextIndex) continue;
                if (TripletRotationDirection([points[currentIndex], points[i], points[nextIndex]]) ==
                    RotationDirection.Counterclockwise)
                    nextIndex = i;
            }
            convexHull.AddLast(nextIndex);
            currentIndex = nextIndex;
        } while (currentIndex != leftmostIndex);

        return convexHull;
    }
    
    public static void DrawPoly(GraphicsDevice graphicsDevice, List<Vector2> points, Color colour)
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

        Effect effect = Effects.DefaultEffect ?? throw new NullReferenceException("Effect not initialized yet");
        effect.Parameters["World"].SetValue(Matrix.Identity);
        effect.Parameters["Colour"].SetValue(colour.ToVector4());
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3
            );
        }
    }

    public static void DrawLine(this GraphicsDevice graphicsDevice, Vector2 start, Vector2 end, Color colour)
    {
        VertexPositionColor[] vertices = [new(new Vector3(start.X, start.Y, 0), Color.White), new(new Vector3(end.X, end.Y, 0), Color.White)];
        int[] indices = [0, 1];
        
        Effect effect = Effects.DefaultEffect ?? throw new NullReferenceException("Effect not initialized yet");
        effect.Parameters["World"].SetValue(Matrix.Identity);
        effect.Parameters["Colour"].SetValue(colour.ToVector4());
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.LineList, vertices, 0, vertices.Length, indices, 0, 1
            );
        }
    }

    public static void GS_DrawLine(this GraphicsDevice graphicsDevice, Camera camera, SD_Vector2 start, SD_Vector2 end, Color colour)
    {
        Vector2 screenStart = camera.ConvertToScreenCoordinates(start);
        Vector2 screenEnd = camera.ConvertToScreenCoordinates(end);
        DrawLine(graphicsDevice, screenStart, screenEnd, colour);
    }

    public static void GS_DrawLineR(this GraphicsDevice graphicsDevice, Camera camera, SD_Vector2 start,
        SD_Vector2 displacement, Color colour)
    {
        GS_DrawLine(graphicsDevice, camera, start, start + displacement, colour);
    }
    
    public static void GS_DrawPoint(this GraphicsDevice graphicsDevice, Camera camera, SD_Vector2 position, Color colour)
    {
        Vector2 screenPosition = camera.ConvertToScreenCoordinates(position);
        VertexPositionColor[] vertices = [
            new(new Vector3(0, 5, 0), colour), 
            new(new Vector3(5, 0, 0), colour),
            new(new Vector3(0, -5, 0), colour), 
            new(new Vector3(-5, 0, 0), colour)];
        int[] indices = [0, 1, 3, 3, 1, 2];
        
        Effect effect = Effects.DefaultEffect ?? throw new NullReferenceException("Effect not initialized yet");;
        effect.Parameters["World"].SetValue(Matrix.CreateScale(new Vector3(1, 1, 1)) * 
                                            Matrix.CreateTranslation(new Vector3(screenPosition.X, screenPosition.Y, 0)));
        effect.Parameters["Colour"].SetValue(colour.ToVector4());
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, 2
            );
        }
    }
}