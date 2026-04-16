using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

/// <summary>
/// Represents a hull bounded by a convex set of points ordered counterclockwise starting with the leftmost point.
/// </summary>
public struct ConvexHull
{
    /// <summary>
    /// Points which define the bounds of the convex hull.
    /// Colinear points are not included.
    /// </summary>
    public List<Vec2Double> Points { get; private set; } = [];

    /// <summary>
    /// Fan triangulation of the convex hull.
    /// </summary>
    public (Vec2Double a, Vec2Double b, Vec2Double c)[] Triangulation { get; private set; } = [];
    
    /// <summary>
    /// Generate the convex hull including all of the input points centered at its center of mass.
    /// </summary>
    /// <param name="points">Points to form a convex hull out of.</param>
    public ConvexHull(Vec2Double[] points)
    {
        LinkedList<int> convexHullIndices = GetHullIndices(points);
        convexHullIndices.RemoveLast();
        Points = convexHullIndices.Select(x => points[x]).ToList();
        RemoveColinearPoints();
        Triangulate();
        Center();
    }

    /// <summary>
    /// Set the origin of the convex hull to be the center of mass.
    /// </summary>
    private void Center()
    {
        Vec2Double centerOfMass = CalculateCenterOfMass();
        Points = Points.Select(v => v - centerOfMass).ToList();
        Triangulation = Triangulation.Select(x => (x.a - centerOfMass, x.b - centerOfMass, x.c - centerOfMass)).ToArray();
    }

    /// <summary>
    /// Creates and sets the fan triangulation of the convex hull.
    /// </summary>
    private void Triangulate()
    {
        if (Points.Count < 3) Triangulation = [];
        var triangulation = new (Vec2Double a, Vec2Double b, Vec2Double c)[Points.Count - 2];
        for (int i = 1; i < Points.Count - 1; ++i)
            triangulation[i - 1] = (Points[0], Points[i], Points[i + 1]);
        Triangulation = triangulation;
    }

    /// <summary>
    /// Remove colinear points from the boundary of the convex hull.
    /// </summary>
    private void RemoveColinearPoints()
    {
        for (int i = 0; i < Points.Count; ++i)
        {
            Vec2Double prevPoint = Points[Utils.UnsignedMod(i - 1, Points.Count)];
            Vec2Double currentPoint = Points[i];
            Vec2Double nextPoint = Points[Utils.UnsignedMod(i + 1, Points.Count)];
            double prevSlope = (currentPoint.Y - prevPoint.Y) / (currentPoint.X - prevPoint.X);
            double nextSlope = (nextPoint.Y - currentPoint.X) / (nextPoint.X - currentPoint.X);
            if (Math.Abs(nextSlope - prevSlope) < 1e-10)
            {
                Points.RemoveAt(i);
                i--;
            }
        }
    }
    
    /// <summary>
    /// Returns the rotational order of points of a set of three points.
    /// </summary>
    /// <param name="triplet">Set of three points.</param>
    /// <returns>
    /// Clockwise or Counterclockwise depending on the order of the points or None if the
    /// triangle formed by the triplet is degenerate.
    /// </returns>
    /// <exception cref="ArgumentException">Given number of points is not equal to three.</exception>
    private static RotationDirection GetTripletRotationDirection(Vec2Double[] triplet)
    {
        if (triplet.Length != 3) throw new ArgumentException("Vector2 triplet must have exactly 3 values");
        double edgeSlope1 = (triplet[1].Y - triplet[0].Y) * (triplet[2].X - triplet[0].X);
        double edgeSlope2 = (triplet[2].Y - triplet[0].Y) * (triplet[1].X - triplet[0].X);
        return edgeSlope1 > edgeSlope2 ? RotationDirection.Clockwise :
            edgeSlope1 < edgeSlope2 ? RotationDirection.Counterclockwise : RotationDirection.None;
    }
    
    /// <summary>
    /// Returns the indices of the convex hull which bounds a set of points.
    /// </summary>
    /// <param name="points">Set of points to construct the convex hull out of.</param>
    /// <returns>A linked list containing the indices of the convex hull in counterclockwise order.</returns>
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
    
    /// <summary>
    /// Calculates the center of mass of the convex hull.
    /// </summary>
    /// <returns>The center of mass of the convex hull.</returns>
    private readonly Vec2Double CalculateCenterOfMass()
    {
        Vec2Double centerOfMass = Vec2Double.Zero;;
        foreach (var triangle in Triangulation)
        {
            Vec2Double centroid = (triangle.a + triangle.b + triangle.c) / 3;
            centerOfMass += centroid;
        }
        centerOfMass /= Triangulation.Length;

        return centerOfMass;
    }
    
    /// <summary>
    /// Calculates the area of the convex hull.
    /// </summary>
    /// <returns>The area of the convex hull.</returns>
    public readonly double CalculateArea()
    {
        if (Points.Count < 3) return 0;
        return Triangulation.Aggregate(0d, (a, t) => a + Utils.CalculateTriangleArea(t.a, t.b, t.c));
    }
    
    /// <summary>
    /// Calculates the moment of inertia for the convex hull given a mass.
    /// </summary>
    /// <param name="mass">Mass of the convex hull.</param>
    /// <returns>The moment of inertia around the z-axis given the mass.</returns>
    public readonly SDecimal CalculateInertia(SDecimal mass)
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