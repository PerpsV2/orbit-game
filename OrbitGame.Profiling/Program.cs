using BenchmarkDotNet.Attributes;
using OrbitGame;

var pDecimal = new PDecimal(5, 100);
var sDecimal = new SDecimal(4, 100);
Console.WriteLine(pDecimal.Map<SDecimal>());
Console.WriteLine(sDecimal.Map<PDecimal>());

Console.WriteLine(pDecimal);
Console.WriteLine(sDecimal);

//var summary = BenchmarkRunner.Run<Benchmarker>();

/*public class Benchmarker
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
}*/