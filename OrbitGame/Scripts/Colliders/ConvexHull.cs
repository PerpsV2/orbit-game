using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

public class ConvexHull<T> where T : IArbitraryPlaceDecimal<T>, new()
{
    public Vec2<T>[] Points { get; private set; }
    public (Vec2<T> a, Vec2<T> b, Vec2<T> c)[] Triangulation { get; private set; }
    
    public ConvexHull(Vec2<T>[] points)
    {
        SetHull(points);
        Triangulate();
        Center();
        
        Points ??= [];
        Triangulation ??= [];
    }
    
    private void Center()
        => Points = Points.Select(v => v - CalculateCenterOfMass()).ToArray();
    
    private void Triangulate()
    {
        if (Points.Length < 3) throw new ArgumentException("Convex shape must have at least 3 points.");
        var triangulation = new (Vec2<T> a, Vec2<T> b, Vec2<T> c)[Points.Length - 2];
        for (int i = 1; i < Points.Length - 1; ++i)
            triangulation[i - 1] = (Points[0], Points[i], Points[i + 1]);
        Triangulation = triangulation;
    }

    private void SetHull(Vec2<T>[] points)
    {
        LinkedList<int> convexHullIndices = GetHullIndices(points);
        Points = convexHullIndices.Select(x => points[x]).ToArray();
    }
    
    private static RotationDirection GetTripletRotationDirection(Vec2<T>[] triplet)
    {
        if (triplet.Length != 3) throw new ArgumentException("Vector2 triplet must have exactly 3 values");
        T edgeSlope1 = (triplet[1].Y - triplet[0].Y) * (triplet[2].X - triplet[0].X);
        T edgeSlope2 = (triplet[2].Y - triplet[0].Y) * (triplet[1].X - triplet[0].X);
        return edgeSlope1 > edgeSlope2 ? RotationDirection.Clockwise :
            edgeSlope1 < edgeSlope2 ? RotationDirection.Counterclockwise : RotationDirection.None;
    }
    
    private static LinkedList<int> GetHullIndices(Vec2<T>[] points)
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
    
    public Vec2<T> CalculateCenterOfMass()
    {
        Vec2<T> centerOfMass = Vec2<T>.Zero;
        foreach (var triangle in Triangulation)
        {
            Vec2<T> centroid = (triangle.a + triangle.b + triangle.c) / T.FromDouble(3);
            centerOfMass += centroid;
        }

        centerOfMass /= T.FromDouble(Triangulation.Length);

        return centerOfMass;
    }
    
    public T CalculateArea()
    {
        if (Points.Length < 3) return T.Zero;
        return Triangulation.Aggregate(T.Zero, (a, t) => a + Utils.CalculateTriangleArea(t.a, t.b, t.c));
    }
    
    public T CalculateInertia(T mass)
    {
        T totalArea = CalculateArea();

        T[] masses = new T[Triangulation.Length];
        T[] inertias = new T[Triangulation.Length];
        Vec2<T>[] centroids = new Vec2<T>[Triangulation.Length];

        for (int i = 0; i < Triangulation.Length; ++i)
        {
            Vec2<T> a = Triangulation[i].a;
            Vec2<T> b = Triangulation[i].b;
            Vec2<T> c = Triangulation[i].c;

            masses[i] = mass * Utils.CalculateTriangleArea(a, b, c) / totalArea;
            centroids[i] = (a + b + c) / T.FromDouble(3);
            inertias[i] = Utils.CalculateTriangleInertia(a, b, c, masses[i]);
        }

        Vec2<T> totalCentroid = new();
        for (int i = 0; i < Triangulation.Length; ++i)
        {
            totalCentroid += new Vec2<T>(
                masses[i] * centroids[i].X,
                masses[i] * centroids[i].Y
            );
        }

        totalCentroid /= mass;

        T[] centroidDistancesSquared = new T[Triangulation.Length];
        for (int i = 0; i < Triangulation.Length; ++i)
            centroidDistancesSquared[i] = (totalCentroid - centroids[i]).MagnitudeSquared();

        T totalInertia = new();
        for (int i = 0; i < Triangulation.Length; ++i)
            totalInertia += inertias[i] + masses[i] * centroidDistancesSquared[i];

        return totalInertia;
    }
}