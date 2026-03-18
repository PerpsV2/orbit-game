using System.Collections;
using System.Globalization;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame.Profiling;

Random rnd = new();
for (int i = 0; i < 1000000; ++i)
{
    //BigIntScientificDecimal left1 = new(rnd.NextInt64(1, 30), rnd.Next(-5, 5));
    BigIntScientificDecimal right1 = new(rnd.NextInt64(0, 30), rnd.Next(-5, 5));
    BigIntScientificDecimal.Sqrt(right1);
    //left1 /= right1;
    //Console.WriteLine($"sqrt({right1:N}) = {BigIntScientificDecimal.Sqrt(right1):N}");
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
        => BigIntScientificDecimal.Sqrt(_left1);

    [Benchmark]
    public ScientificDecimal Operation2()
        => ScientificDecimal.Sqrt(_left2);
}