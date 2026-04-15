using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame;
using OrbitGame.Profiling;

Console.WriteLine(Utils.PerlinNoise1D(1, 2, 1, 1));

/*var summary = BenchmarkRunner.Run<Benchmarker>();

public class Benchmarker
{
    private static readonly Random Rnd = new();
    private readonly SDecimal _value = new(Rnd.NextDouble(), Rnd.Next(-300, 300));


    [Benchmark]
    public double Operation1()
        => SDecimal.ConvertToDoubleSaturating(_value);
}*/