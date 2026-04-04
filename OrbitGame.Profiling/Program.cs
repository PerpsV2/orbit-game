using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame;

var summary = BenchmarkRunner.Run<Benchmarker>();

public class Benchmarker
{
    private static readonly Random Rnd = new();
    private readonly SDecimal _value = new(Rnd.NextDouble(), Rnd.Next(-300, 300));
    
    
    [Benchmark]
    public double Operation1()
        => SDecimal.ConvertToDoubleSaturating(_value);
}