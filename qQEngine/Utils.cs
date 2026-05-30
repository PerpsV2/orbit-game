namespace qQEngine;

public static class Utils
{
    public static double UnsignedMod(double a, double b)
        => a - b * Math.Floor(a / b);
    
    public static double WrapAngle(double angle)
        => UnsignedMod(angle, Math.Tau);
}