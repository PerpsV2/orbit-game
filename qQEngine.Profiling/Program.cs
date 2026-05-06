using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using qQEngine;

var summary = BenchmarkRunner.Run<Benchmarker>();

public class Benchmarker
{
    private static Random _rnd = new Random();

    private double _rndValue;
    private SDecimal _sDecimal;
    private ScientificDecimal _scientificDecimal;
    
    public Benchmarker()
    {
        _rndValue = _rnd.NextDouble();
    }

    [Benchmark]
    public void Operation1()
    {
        _sDecimal = new SDecimal(_rndValue, 0);
    }

    [Benchmark]
    public void Operation2()
    {
        _scientificDecimal = new ScientificDecimal(_rndValue, 0);
    }
}