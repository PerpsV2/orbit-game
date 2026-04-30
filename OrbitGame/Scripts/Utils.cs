using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

namespace OrbitGame;

public static class Constants
{
    public static double ComparisonTolerance { get; } = 1e-32;
    public static double Epsilon { get; } = 1e-10;
    public static SDecimal G { get; private set; } = new(6.6743, -11);

    public static void SetGravitationalConstant<T>(IArbitraryPlaceDecimal<T> value) 
        where T : IArbitraryPlaceDecimal<T>, new()
    {
        G = value.Map<SDecimal>();
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
    
    public static double CalculateTriangleArea(Vec2Double a, Vec2Double b, Vec2Double c)
    {
        return double.Abs(a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) / 2;
    }

    public static SDecimal CalculateTriangleInertia(Vec2Double a, Vec2Double b, Vec2Double c, SDecimal mass)
    {
        return mass * (
            Vec2Double.Dot(a, a) + Vec2Double.Dot(b, b) + Vec2Double.Dot(c, c) +
            Vec2Double.Dot(a, b) + Vec2Double.Dot(b, c) + Vec2Double.Dot(c, a)
        ) / 6;
    }

    public static double SmoothStep(double value)
    {
        if (value >= 1) return 1;
        if (value <= 0) return 0;
        return 3 * value * value - 2 * value * value * value;
    }

    public static T Lerp<T>(T a, T b, T t) where T : INumber<T>
        => a + t * (b - a);

    public static double PerlinNoise1D(int seed, double position, double amplitude, double frequency)
    {
        if (frequency <= 0) throw new ArgumentOutOfRangeException(nameof(frequency));
        position /= frequency;
        double leftSlope = new Random(HashCode.Combine(seed, Math.Floor(position))).NextDouble() * 2 - 1;
        double rightSlope = new Random(HashCode.Combine(seed, Math.Ceiling(position))).NextDouble() * 2 - 1;
        double midValue = position - Math.Floor(position);
        double leftValue = leftSlope * midValue;
        double rightValue = rightSlope * (midValue - 1);
        return amplitude * Lerp(leftValue, rightValue, SmoothStep(midValue));
    }
}