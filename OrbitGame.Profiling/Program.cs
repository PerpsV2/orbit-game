using System.Collections;
using System.Globalization;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame.Profiling;

/*BigIntScientificDecimal value1 = new(144, 30);
BigIntScientificDecimal value2 = Math.Tau;
Console.WriteLine($"{value1.ToString("N")}, {value2.ToString("N")}");
Console.WriteLine(BigIntScientificDecimal.Sqrt(value1).ToString("G"));*/

Random rnd = new();
for (int i = 0; i < 1000000; ++i)
{
    BigIntScientificDecimal left1 = new(rnd.NextInt64(1, 30), rnd.Next(-5, 5));
    BigIntScientificDecimal right1 = new(rnd.NextInt64(0, 30), rnd.Next(-5, 5));
    Console.WriteLine($"{left1:G} / {right1:G} = {left1/right1:N}");
}

//var summary = BenchmarkRunner.Run<Benchmarker>();

public class Benchmarker
{
    private static readonly Random Rnd = new();
    private readonly BigIntScientificDecimal _left1 = new (Rnd.NextInt64(0, 30), Rnd.Next(-5, 5));
    private readonly ScientificDecimal _left2 = new (Rnd.NextDouble() * Rnd.NextInt64(0, 30), Rnd.Next(-5, 5));
    private readonly BigIntScientificDecimal _right1 = new (Rnd.NextInt64(0, 30), Rnd.Next(-5, 5));
    private readonly ScientificDecimal _right2 = new (Rnd.NextDouble() * Rnd.NextInt64(0, 30), Rnd.Next(-5, 5));
    
    
    [Benchmark]
    public BigIntScientificDecimal Operation1()
        => _left1 / _right1;

    [Benchmark]
    public ScientificDecimal Operation2()
        => _left2 / _right2;
}