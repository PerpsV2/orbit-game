using System.Globalization;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame.Profiling;

var value1 = new ScientificDecimal(3.14159265358979323, 310410000);