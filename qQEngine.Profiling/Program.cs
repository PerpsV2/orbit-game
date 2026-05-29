using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using qQEngine;

var summary = BenchmarkRunner.Run<Benchmarker>();

public class Benchmarker
{
    private static Random _rnd = new Random();

    private SDecimal _sDecimal1;
    private SDecimal _sDecimal2;

    public Benchmarker()
    {
        _sDecimal1 = new SDecimal(_rnd.NextInt64(), 0);
        _sDecimal2 = new SDecimal(_rnd.NextInt64(), 0);
    }

    [Benchmark]
    public SDecimal Operation1()
    {
        return SDecimal.Zero;
    }
}