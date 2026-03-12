using System.Globalization;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame.Profiling;

var value1 = new BinaryScientificDecimal(0b0101L, 300, 62);
var value2 = new BinaryScientificDecimal(0b1L, 300, 62);
var result = value1 / value2;
Console.WriteLine(value1.ToString("N"));
Console.WriteLine(value2.ToString("N"));
Console.WriteLine(result.ToString("N"));

var summary = BenchmarkRunner.Run<Profiler>();

public class Profiler
{
    private static readonly Random Rnd = new();
    
    private readonly BinaryScientificDecimal _left = new(Rnd.NextInt64(long.MinValue, long.MaxValue), Rnd.Next(-3000, 3000), 62);
    private readonly BinaryScientificDecimal _right = new(Rnd.NextInt64(long.MinValue, long.MaxValue), Rnd.Next(-3000, 3000), 62);

    private readonly DecimallyScientificDecimal _SD_left = new((decimal)Rnd.NextDouble(), Rnd.Next(-3000, 3000));
    private readonly DecimallyScientificDecimal _SD_right = new((decimal)Rnd.NextDouble(), Rnd.Next(-3000, 3000));
    
    [Benchmark]
    public BinaryScientificDecimal BSD_Method()
        => _left / _right;

    [Benchmark]
    public DecimallyScientificDecimal SD_Method()
        => _SD_left / _SD_right;
}