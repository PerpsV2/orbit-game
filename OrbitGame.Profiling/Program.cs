using BenchmarkDotNet.Attributes;
using OrbitGame.Profiling;

Random rnd = new();
for (int i = 0; i < 1000000; ++i)
{
    BigIntScientificDecimal right1 = new(rnd.NextInt64(-30, 30), rnd.Next(-1, -1));
    Console.WriteLine($"{right1.ToString("N")} = {BigIntScientificDecimal.Round(right1).ToString("N")}");
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