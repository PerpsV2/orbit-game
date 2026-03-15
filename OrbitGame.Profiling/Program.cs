using System.Globalization;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame.Profiling;
using OrbitGame;

var value1 = 3.52525f;
Console.WriteLine(((ScientificDecimal)(value1)).ToString("N"));