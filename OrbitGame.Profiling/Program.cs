using System.Globalization;
using OrbitGame.Profiling;

/*var value = new BinaryScientificDecimal(-0b101101L, 0, 10);
Console.WriteLine(value.ToString("B"));
Console.WriteLine(value.ToString("N"));
value.IncreaseExponent(5);
Console.WriteLine(value.ToString("B"));
Console.WriteLine(value.ToString("N"));*/

var value1 = new BinaryScientificDecimal(0b101101L, -312234, 62);
var value2 = new BinaryScientificDecimal(0b101101L, 416551, 62);

Console.WriteLine(value1.ToString("B"));
Console.WriteLine(value2.ToString("B"));
Console.WriteLine(BinaryScientificDecimal.Multiply(value1, value2).ToString("B"));

Console.WriteLine(value1.ToString("N"));
Console.WriteLine(value2.ToString("N"));
Console.WriteLine(BinaryScientificDecimal.Multiply(value1, value2).ToString("N"));