using System.Collections;
using System.Globalization;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame.Profiling;

var value1 = new BigIntScientificDecimal(100, 24);
var value2 = new BigIntScientificDecimal(100, 0);
Console.WriteLine($"{value1.ToString("G")}, {value2.ToString("G")}");
Console.WriteLine(BigIntScientificDecimal.Add(value1, value2));

var summary = BenchmarkRunner.Run<Benchmarker>();

public class Benchmarker()
{
    private static Random _rnd = new Random();
    private BigIntScientificDecimal _left1 = new BigIntScientificDecimal(_rnd.NextInt64(), _rnd.Next(-5, 5));
    private ScientificDecimal _left2 = new ScientificDecimal(_rnd.NextDouble(), _rnd.Next(-5, 5));
    private BigIntScientificDecimal _right1 = new BigIntScientificDecimal(_rnd.NextInt64(), _rnd.Next(-5, 5));
    private ScientificDecimal _right2 = new ScientificDecimal(_rnd.NextDouble(), _rnd.Next(-5, 5));
    
    
    [Benchmark]
    public BigIntScientificDecimal Add1()
        => _left1 + _right1;

    [Benchmark]
    public ScientificDecimal Add2()
        => _left2 + _right2;
}