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
    
    public static T CalculateTriangleArea<T>(Vec2<T> a, Vec2<T> b, Vec2<T> c)
        where T : IArbitraryPlaceDecimal<T>
    {
        return T.Abs(a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) / T.FromDouble(2);
    }

    public static T CalculateTriangleInertia<T>(Vec2<T> a, Vec2<T> b, Vec2<T> c, T mass)
        where T : IArbitraryPlaceDecimal<T>
    {
        return mass * (
            Vec2<T>.Dot(a, a) + Vec2<T>.Dot(b, b) + Vec2<T>.Dot(c, c) +
            Vec2<T>.Dot(a, b) + Vec2<T>.Dot(b, c) + Vec2<T>.Dot(c, a)
        ) / T.FromDouble(6);
    }
}