using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using qQEngine;

var summary = BenchmarkRunner.Run<Benchmarker>();

public class Benchmarker
{
    private static Random _rnd = new Random();

    private SDecimal _sDecimal1;
    private SDecimal _sDecimal2;
    private ScientificDecimal _scientificDecimal1;
    private ScientificDecimal _scientificDecimal2;

    public Benchmarker()
    {
        _sDecimal1 = new SDecimal(_rnd.NextInt64(), 0);
        _sDecimal2 = new SDecimal(_rnd.NextInt64(), 0);

        _scientificDecimal1 = new ScientificDecimal(_rnd.NextInt64(), 0);
        _scientificDecimal2 = new ScientificDecimal(_rnd.NextInt64(), 0);
    }

    [Benchmark]
    public SDecimal Operation1()
    {
        return _sDecimal1 + _sDecimal2;
    }
    
    [Benchmark]
    public ScientificDecimal Operation2()
    {
        return _scientificDecimal1 + _scientificDecimal2;
    }
}