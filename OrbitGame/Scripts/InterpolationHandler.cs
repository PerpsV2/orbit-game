using System.Numerics;

namespace OrbitGame;

public ref struct Interpolation<T> where T : INumber<T>
{
    private readonly ref ScientificDecimal _time;
    private readonly ScientificDecimal _startTime;
    private readonly ScientificDecimal _endTime;
    private readonly T _startValue;
    private readonly T _endValue;
    
    public Interpolation(ref ScientificDecimal time, ScientificDecimal startTime, ScientificDecimal endTime, 
        T startValue, T endValue)
    {
        _time = time;
        _startTime = startTime;
        _endTime = endTime;
        _startValue = startValue;
        _endValue = endValue;
    }
}

public static class InterpolationHandler
{
    public static void CreateInterpolation<T>(ref ScientificDecimal time, ScientificDecimal start, ScientificDecimal end, 
        T startValue, T endValue) where T : INumber<T>
    {
    }
}