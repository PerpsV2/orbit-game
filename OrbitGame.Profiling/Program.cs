using System.Globalization;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame.Profiling;

var value1 = new BinaryScientificDecimal(0b1L, 0, 62);
var value2 = new BinaryScientificDecimal(0b1L, 5, 62);
var result = value1 * value2;
var format = "N";
Console.WriteLine(value1.ToString(format));
Console.WriteLine(value2.ToString(format));
Console.WriteLine(result.ToString(format));

var summary = BenchmarkRunner.Run<Profiler>();

public class Profiler
{
    private static readonly Random Rnd = new();
    
    private readonly BinaryScientificDecimal _left = new(Rnd.NextInt64(long.MinValue, long.MaxValue), Rnd.Next(-3000, 3000), 62);
    private readonly BinaryScientificDecimal _right = new(Rnd.NextInt64(long.MinValue, long.MaxValue), Rnd.Next(-3000, 3000), 62);

    private readonly DecimallyScientificDecimal _sdLeft = new((decimal)Rnd.NextDouble(), Rnd.Next(-3000, 3000));
    private readonly DecimallyScientificDecimal _sdRight = new((decimal)Rnd.NextDouble(), Rnd.Next(-3000, 3000));
    
    //[Benchmark]
    public BinaryScientificDecimal BSD_Add()
        => _left + _right;
    
    //[Benchmark]
    public BinaryScientificDecimal BSD_Subtract()
        => _left - _right;
    
    [Benchmark]
    public BinaryScientificDecimal BSD_Multiply()
        => _left * _right;
    
    //[Benchmark]
    public BinaryScientificDecimal BSD_Divide()
        => _left / _right;
    
    //[Benchmark]
    public DecimallyScientificDecimal SD_Add()
        => _sdLeft + _sdRight;
    
    //[Benchmark]
    public DecimallyScientificDecimal SD_Subtract()
        => _sdLeft - _sdRight;
    
    [Benchmark]
    public DecimallyScientificDecimal SD_Multiply()
        => _sdLeft * _sdRight;

    //[Benchmark]
    public DecimallyScientificDecimal SD_Divide()
        => _sdLeft / _sdRight;
}