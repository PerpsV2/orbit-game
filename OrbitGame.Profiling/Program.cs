using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame;
using OrbitGame.Profiling;

OrbitGame.Profiling.LinearEquationSystem<SDecimal> system = new OrbitGame.Profiling.LinearEquationSystem<SDecimal>(5, 7);

system.SetCoefficient(0, 1);
system.SetCoefficient(1, 4);
system.SetCoefficient(2, 16);
system.SetCoefficient(3, 1);
system.SetCoefficient(4, 4);
system.SetConstant(0, 1);

system.SetCoefficient(5, 0);
system.SetCoefficient(6, 0);
system.SetCoefficient(7, 4);
system.SetCoefficient(8, 0);
system.SetCoefficient(9, -2);
system.SetConstant(1, 1);

system.SetCoefficient(10, 9);
system.SetCoefficient(11, -6);
system.SetCoefficient(12, 4);
system.SetCoefficient(13, -3);
system.SetCoefficient(14, 2);
system.SetConstant(2, 1);

system.SetCoefficient(15, 16);
system.SetCoefficient(16, -8);
system.SetCoefficient(17, 4);
system.SetCoefficient(18, 4);
system.SetCoefficient(19, -2);
system.SetConstant(3, 1);

system.SetCoefficient(20, 36);
system.SetCoefficient(21, 12);
system.SetCoefficient(22, 4);
system.SetCoefficient(23, 6);
system.SetCoefficient(24, 2);
system.SetConstant(4, 1);

system.SetCoefficient(25, 36);
system.SetCoefficient(26, 12);
system.SetCoefficient(27, 4);
system.SetCoefficient(28, 6);
system.SetCoefficient(29, 2);
system.SetConstant(5, 1);

system.SetCoefficient(30, 36);
system.SetCoefficient(31, 12);
system.SetCoefficient(32, 4);
system.SetCoefficient(33, 6);
system.SetCoefficient(34, 2);
system.SetConstant(6, 1);

Console.WriteLine(system);

system.ConvertToReducedEchelonForm();

Console.WriteLine(system);

SDecimal[] solution = system.Solve();

foreach (var value in solution)
{
    Console.WriteLine($"{value.ToString()} ");
}

/*var summary = BenchmarkRunner.Run<Benchmarker>();

public class Benchmarker
{
    private static readonly Random Rnd = new();
    private readonly SDecimal _value = new(Rnd.NextDouble(), Rnd.Next(-300, 300));
    
    
    [Benchmark]
    public double Operation1()
        => SDecimal.ConvertToDoubleSaturating(_value);
}*/