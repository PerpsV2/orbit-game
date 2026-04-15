using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

/// <summary>
/// Represents a convex set of points.
/// </summary>
public struct ConvexHull
{
    /// <summary>
    /// Convex hull points.
    /// </summary>
    public Vec2Double[] Points { get; private set; }
    
    /// <summary>
    /// Fan triangulation of the convex hull.
    /// </summary>
    public (Vec2Double a, Vec2Double b, Vec2Double c)[] Triangulation { get; private set; }
    
    /// <summary>
    /// Create the minimum convex hull which includes all of the input points.
    /// </summary>
    /// <param name="points">Points to form a convex hull out of.</param>
    public ConvexHull(Vec2Double[] points)
    {
        SetHull(points);
        Triangulate();
        Center();
        
        Points ??= [];
        Triangulation ??= [];
    }

    /// <summary>
    /// Set the origin of the convex hull to be the center of mass.
    /// </summary>
    private void Center()
    {
        Vec2Double centerOfMass = CalculateCenterOfMass();
        Points = Points.Select(v => v - centerOfMass).ToArray();
    }

    /// <summary>
    /// Sets the fan triangulation of the convex hull.
    /// </summary>
    private void Triangulate()
    {
        if (Points.Length < 3) Triangulation = [];
        var triangulation = new (Vec2Double a, Vec2Double b, Vec2Double c)[Points.Length - 2];
        for (int i = 1; i < Points.Length - 1; ++i)
            triangulation[i - 1] = (Points[0], Points[i], Points[i + 1]);
        Triangulation = triangulation;
    }

    private void SetHull(Vec2Double[] points)
    {
        LinkedList<int> convexHullIndices = GetHullIndices(points);
        convexHullIndices.RemoveLast();
        Points = convexHullIndices.Select(x => points[x]).ToArray();
    }
    
    private static RotationDirection GetTripletRotationDirection(Vec2Double[] triplet)
    {
        if (triplet.Length != 3) throw new ArgumentException("Vector2 triplet must have exactly 3 values");
        double edgeSlope1 = (triplet[1].Y - triplet[0].Y) * (triplet[2].X - triplet[0].X);
        double edgeSlope2 = (triplet[2].Y - triplet[0].Y) * (triplet[1].X - triplet[0].X);
        return edgeSlope1 > edgeSlope2 ? RotationDirection.Clockwise :
            edgeSlope1 < edgeSlope2 ? RotationDirection.Counterclockwise : RotationDirection.None;
    }
    
    private static LinkedList<int> GetHullIndices(Vec2Double[] points)
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
                if (GetTripletRotationDirection([points[currentIndex], points[i], points[nextIndex]]) ==
                    RotationDirection.Counterclockwise)
                    nextIndex = i;
            }
            convexHull.AddLast(nextIndex);
            currentIndex = nextIndex;
        } while (currentIndex != leftmostIndex);

        return convexHull;
    }
    
    public Vec2Double CalculateCenterOfMass()
    {
        Vec2Double centerOfMass = Vec2Double.Zero;
        foreach (var triangle in Triangulation)
        {
            Vec2Double centroid = (triangle.a + triangle.b + triangle.c) / 3;
            centerOfMass += centroid;
        }

        centerOfMass /= Triangulation.Length;

        return centerOfMass;
    }
    
    public double CalculateArea()
    {
        if (Points.Length < 3) return 0;
        return Triangulation.Aggregate(0d, (a, t) => a + Utils.CalculateTriangleArea(t.a, t.b, t.c));
    }
    
    public SDecimal CalculateInertia(SDecimal mass)
    {
        double totalArea = CalculateArea();

        double[] areas = new double[Triangulation.Length];
        SDecimal[] masses = new SDecimal[Triangulation.Length];
        SDecimal[] inertias = new SDecimal[Triangulation.Length];
        Vec2Double[] centroids = new Vec2Double[Triangulation.Length];

        for (int i = 0; i < Triangulation.Length; ++i)
        {
            Vec2Double a = Triangulation[i].a;
            Vec2Double b = Triangulation[i].b;
            Vec2Double c = Triangulation[i].c;

            areas[i] = Utils.CalculateTriangleArea(a, b, c);
            masses[i] = mass * areas[i] / totalArea;
            centroids[i] = (a + b + c) / 3;
            inertias[i] = Utils.CalculateTriangleInertia(a, b, c, masses[i]);
        }

        Vec2Double totalCentroid = new();
        for (int i = 0; i < Triangulation.Length; ++i)
        {
            totalCentroid += new Vec2Double(
                areas[i] * centroids[i].X,
                areas[i] * centroids[i].Y
            );
        }

        totalCentroid /= totalArea;

        SDecimal[] centroidDistancesSquared = new SDecimal[Triangulation.Length];
        for (int i = 0; i < Triangulation.Length; ++i)
            centroidDistancesSquared[i] = (totalCentroid - centroids[i]).MagnitudeSquared();

        SDecimal totalInertia = new();
        for (int i = 0; i < Triangulation.Length; ++i)
            totalInertia += inertias[i] + masses[i] * centroidDistancesSquared[i];

        return totalInertia;
    }
}