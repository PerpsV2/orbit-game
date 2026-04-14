using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame;
using OrbitGame.Profiling;

ConvexHull hull = new ConvexHull([
    new DoubleVec2(0, 0),
    new DoubleVec2(-3, 1),
    new DoubleVec2(4, 2),
    new DoubleVec2(5, 4),
    new DoubleVec2(-6, -4),
    new DoubleVec2(-4, 0),
]);

Utils.LogEnumerable(hull.Points);

/*var summary = BenchmarkRunner.Run<Benchmarker>();

public class Benchmarker
{
    private static readonly Random Rnd = new();
    private readonly SDecimal _value = new(Rnd.NextDouble(), Rnd.Next(-300, 300));


    [Benchmark]
    public double Operation1()
        => SDecimal.ConvertToDoubleSaturating(_value);
}*/