using System;
using System.Globalization;
using OrbitGame.Profiling;

var value = BinaryScientificDecimal.FromDecimal(1300, 5, 5);
Console.WriteLine(value.ToString("B", CultureInfo.InvariantCulture));
Console.WriteLine(value.ToString("G", CultureInfo.InvariantCulture));