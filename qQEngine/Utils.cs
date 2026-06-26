namespace qQEngine;

public static class Options
{
    public static (int width, int height) ScreenSize = (400, 400);
}

public static class Constants
{
    public static SDecimal G { get; private set; } = new(6.6743, -11);
    public static SDecimal Ke { get; private set; } = new(8.988, 9);
}

public enum NumericalIntegrator
{
    ExplicitEuler,
    ImplicitEuler,
    VelocityVerlet,
    RungeKutta4
}

public static class Utils
{
    public static double UnsignedMod(double a, double b)
        => a - b * Math.Floor(a / b);
    
    public static double WrapAngle(double angle)
        => UnsignedMod(angle, Math.Tau);
}