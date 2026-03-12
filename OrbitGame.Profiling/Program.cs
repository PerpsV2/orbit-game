using System.Globalization;
using OrbitGame.Profiling;

var value1 = new BinaryScientificDecimal(0b10L, 0, 7);
var value2 = new BinaryScientificDecimal(0b1011L, 0, 7);
Console.WriteLine(value1.ToString("B"));
Console.WriteLine(value2.ToString("B"));
Console.WriteLine(BinaryScientificDecimal.Multiply(value1, value2).ToString("B"));
Console.WriteLine(value1.ToString("N"));
Console.WriteLine(value2.ToString("N"));
Console.WriteLine(BinaryScientificDecimal.Multiply(value1, value2).ToString("N"));
//Console.WriteLine(BinaryScientificDecimal.Multiply(value1, value2).ToString("B", CultureInfo.InvariantCulture));
//Console.WriteLine(BinaryScientificDecimal.Multiply(value1, value2).ToString("G", CultureInfo.InvariantCulture));