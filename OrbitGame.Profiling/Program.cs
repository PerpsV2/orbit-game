using System.Globalization;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame.Profiling;

var value1 = new BinaryScientificDecimal(0b1101100101110101, -1);
var value2 = new BinaryScientificDecimal(0b111, -2);
//var result = value1 % value2;
var format = "N";
Console.WriteLine(value1.ToString(format));
//Console.WriteLine(value2.ToString(format));
//Console.WriteLine(result.ToString(format));
Console.WriteLine(BinaryScientificDecimal.Sqrt(value1).ToString(format));

//var summary = BenchmarkRunner.Run<Profiler>();

public class Profiler
{
    private static readonly Random Rnd = new();
    
    private readonly BinaryScientificDecimal _left = new(Rnd.NextInt64(long.MinValue, long.MaxValue), Rnd.Next(-3000, 3000));
    private readonly BinaryScientificDecimal _right = new(Rnd.NextInt64(long.MinValue, long.MaxValue), Rnd.Next(-3000, 3000));

    private readonly ScientificDecimal _sdLeft = new(Rnd.NextDouble(), Rnd.Next(-3000, 3000));
    private readonly ScientificDecimal _sdRight = new(Rnd.NextDouble(), Rnd.Next(-3000, 3000));
    
    [Benchmark]
    public BinaryScientificDecimal BSD_Mod()
        => _left % _right;
    
    [Benchmark]
    public ScientificDecimal SD_Mod()
        => _sdLeft % _sdRight;
}