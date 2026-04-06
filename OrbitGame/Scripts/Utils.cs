using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

namespace OrbitGame;

public static class Constants
{
    public static SDecimal G { get; private set; } = new(6.6743, -11);
    public static PDecimal GPrecise { get; private set; }= new(6.6743, -11);

    public static void SetGravitationalConstant<T>(IArbitraryPlaceDecimal<T> value) 
        where T : IArbitraryPlaceDecimal<T>, new()
    {
        G = value.Map<SDecimal>();
        GPrecise = value.Map<PDecimal>();
    }
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
    
    public static T UnsignedMod<T>(T a, T b) where T : IArbitraryPlaceDecimal<T>
        => a - b * T.Floor(a / b);

    public static double WrapAngle(double angle)
        => UnsignedMod(angle, Math.Tau);

    public static bool AngleInRange(double angle, double min, double max)
    {
        min = WrapAngle(min);
        max = WrapAngle(max);
        angle = WrapAngle(angle);
        while (max < min) max += Math.Tau;
        return (angle >= min && angle <= max) || (angle + Math.Tau >= min && angle + Math.Tau <= max);
    }

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

    public static void IterateAngleRange(double start, double end, double step, Action<int, double> action, 
        bool extraIteration = false)
    {
        if (start > end) end += Math.Tau;
        int i = 0;
        for (double angle = start; angle <= end + step * (extraIteration ? 1 : 0); angle += step)
        {
            action(i, WrapAngle(angle));
            ++i;
        }
    }

    public static double GetClockwiseAngle(double left, double right)
    {
        return WrapAngle(WrapAngle(right) - WrapAngle(left)) < Math.PI ? left : right;
    }
    
    public static double GetCounterClockwiseAngle(double left, double right)
    {
        return WrapAngle(WrapAngle(right) - WrapAngle(left)) >= Math.PI ? left : right;
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

    public static SDecimal GetConvexHullArea(Vec2<SDecimal>[] points)
    {
        if (points.Length < 3) return 0;
        var triangles = TriangulateConvex(points);
        return triangles.Aggregate(new SDecimal(0),
            (a, t) => a + CalculateTriangleArea(t.a, t.b, t.c));
    }
    
    public static SDecimal CalculateTriangleArea(Vec2<SDecimal> a, Vec2<SDecimal> b, Vec2<SDecimal> c)
    {
        return SDecimal.Abs(a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) / 2;
    }

    public static SDecimal CalculateTriangleInertia(Vec2<SDecimal> a, Vec2<SDecimal> b, Vec2<SDecimal> c, SDecimal mass)
    {
        return mass * (
            Vec2<SDecimal>.Dot(a, a) + Vec2<SDecimal>.Dot(b, b) + Vec2<SDecimal>.Dot(c, c) +
            Vec2<SDecimal>.Dot(a, b) + Vec2<SDecimal>.Dot(b, c) + Vec2<SDecimal>.Dot(c, a)
        ) / 6;
    }

    public static SDecimal GetConvexHullInertia(Vec2<SDecimal>[] points, SDecimal totalMass)
    {
        var triangles = TriangulateConvex(points);

        SDecimal totalArea = GetConvexHullArea(points);

        SDecimal[] masses = new SDecimal[triangles.Length];
        SDecimal[] inertias = new SDecimal[triangles.Length];
        Vec2<SDecimal>[] centroids = new Vec2<SDecimal>[triangles.Length];

        for (int i = 0; i < triangles.Length; ++i)
        {
            Vec2<SDecimal> a = triangles[i].a;
            Vec2<SDecimal> b = triangles[i].b;
            Vec2<SDecimal> c = triangles[i].c;

            masses[i] = totalMass * CalculateTriangleArea(a, b, c) / totalArea;
            centroids[i] = (a + b + c) / 3;
            inertias[i] = CalculateTriangleInertia(a, b, c, masses[i]);
        }

        Vec2<SDecimal> totalCentroid = new();
        for (int i = 0; i < triangles.Length; ++i)
        {
            totalCentroid += new Vec2<SDecimal>(
                masses[i] * centroids[i].X,
                masses[i] * centroids[i].Y
            );
        }

        totalCentroid /= totalMass;

        SDecimal[] centroidDistancesSquared = new SDecimal[triangles.Length];
        for (int i = 0; i < triangles.Length; ++i)
            centroidDistancesSquared[i] = (totalCentroid - centroids[i]).MagnitudeSquared();

        SDecimal totalInertia = new();
        for (int i = 0; i < triangles.Length; ++i)
            totalInertia += inertias[i] + masses[i] * centroidDistancesSquared[i];

        return totalInertia;
    }
    
    /// <summary>
    /// Decomposes the vertices of a convex polygon into triangles
    /// </summary>
    public static (Vec2<SDecimal> a, Vec2<SDecimal> b, Vec2<SDecimal> c)[] TriangulateConvex(Vec2<SDecimal>[] points)
    {
        if (points.Length < 3) throw new ArgumentException("Convex shape must have at least 3 points.");
        var triangulation = new (Vec2<SDecimal> a, Vec2<SDecimal> b, Vec2<SDecimal> c)[points.Length - 2];
        for (int i = 1; i < points.Length - 1; ++i)
            triangulation[i - 1] = (points[0], points[i], points[i + 1]);

        return triangulation;
    }
    
    /// <summary>
    /// Returns the center of mass from the vertices of a convex polygon
    /// </summary>
    public static Vec2<SDecimal> CenterOfMassConvex(Vec2<SDecimal>[] points)
    {
        var triangles = TriangulateConvex(points);
        Vec2<SDecimal> centerOfMass = Vec2<SDecimal>.Zero;
        foreach (var triangle in triangles)
        {
            Vec2<SDecimal> centroid = (triangle.a + triangle.b + triangle.c) / 3;
            centerOfMass += centroid;
        }

        centerOfMass /= triangles.Length;

        return centerOfMass;
    }

    /// <summary>
    /// Re-centers a convex polygon at its center of mass
    /// </summary>
    public static Vec2<SDecimal>[] CenterConvex(Vec2<SDecimal>[] points)
        => points.Select(v => v - CenterOfMassConvex(points)).ToArray();

    /// <summary>
    /// Returns whether a set of three points is ordered clockwise or counter-clockwise
    /// </summary>
    public static RotationDirection TripletRotationDirection(Vec2<SDecimal>[] triplet)
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
    public static LinkedList<int> GetConvexHullIndices(Vec2<SDecimal>[] points)
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
}