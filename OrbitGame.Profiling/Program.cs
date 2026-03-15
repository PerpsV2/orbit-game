using System.Globalization;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame.Profiling;
using OrbitGame;

ScientificDecimal value1 = new ScientificDecimal(100, 5);
Console.WriteLine(value1.ToString("B"));
Console.WriteLine(value1.ToString("N"));