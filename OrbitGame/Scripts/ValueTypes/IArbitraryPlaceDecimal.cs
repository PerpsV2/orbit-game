using System;
using System.Numerics;

namespace OrbitGame;

/// <summary>
/// Number with arbitrary place value.
/// </summary>
/// <typeparam name="TSelf"></typeparam>
public interface IArbitraryPlaceDecimal<TSelf> : INumber<TSelf>
    where TSelf : INumber<TSelf>?
{
    /// <summary>
    /// Whether the number is greater or equal to zero.
    /// </summary>
    public bool Positive { get; }
    
    /// <summary>
    /// Whether the number is less than zero.
    /// </summary>
    public bool Negative { get; }
    
    /// <summary>
    /// Returns the value of positive infinity.
    /// </summary>
    public static abstract TSelf PositiveInfinity { get; }
    
    /// <summary>
    /// Returns the value of negative infinity.
    /// </summary>
    public static abstract TSelf NegativeInfinity { get; }
    
    /// <summary>
    /// Converts a double into an arbitrary place decimal.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <param name="exponent">Power of ten exponent to multiply the value by.</param>
    /// <returns>An arbitrary place decimal with an equivalent value as the double.</returns>
    /// <exception cref="ArgumentException">Attempted to convert NaN into an arbitrary place decimal.</exception>
    public static abstract TSelf FromDouble(double value, int exponent = 0);
    
    /// <summary>
    /// Converts an arbitrary place decimal into a double.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns>A double with an equivalent value as the arbitrary place decimal.</returns>
    /// <exception cref="OverflowException">Arbitrary place decimal is outside the range of a double.</exception>
    public static abstract double ToDouble(TSelf value);
    
    /// <summary>
    /// Converts an arbitrary place decimal into a double while clamping within the double's range to avoid overflow.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns>A double with an equivalent value as the arbitrary place decimal or double.Min/MaxValue.</returns>
    public static abstract double ToDoubleSafe(TSelf value);
    
    /// <summary>
    /// Method to map from one TSelf to another type of arbitrary place decimal
    /// </summary>
    /// <typeparam name="TOther">Type to convert to</typeparam>
    /// <returns>An arbitrary place decimal of type TOther with an equivalent value</returns>
    public TOther Map<TOther>() where TOther : new();

    /// <summary>
    /// Calculates the modulo between two values.
    /// </summary>
    /// <param name="value">Value to modulate.</param>
    /// <param name="mod">Mod.</param>
    /// <returns>The modulus of the value by the mod.</returns>
    /// <exception cref="ArithmeticException">
    /// Attempted to modulate by zero or attempted to modulate infinity or by infinity.
    /// </exception>
    public static abstract TSelf Mod(TSelf value, TSelf mod);
    
    /// <summary>
    /// Calculate the square of an arbitrary place decimal
    /// </summary>
    /// <param name="value">Value to calculate the square of.</param>
    /// <returns>Square of the value.</returns>
    public static abstract TSelf Square(TSelf value);

    /// <summary>
    /// Calculate an arbitrary place value raised to an integer power.
    /// </summary>
    /// <param name="value">Value to exponentiate.</param>
    /// <param name="amount">Integer power.</param>
    /// <returns>The value raised to the integer power.</returns>
    public static abstract TSelf IntPow(TSelf value, int amount);
    
    /// <summary>
    /// Calculate the square root of an arbitrary place decimal.
    /// </summary>
    /// <param name="value">Value to calculate the square root of.</param>
    /// <returns>Square root of the value.</returns>
    public static abstract TSelf Sqrt(TSelf value);
    
    /// <summary>
    /// Calculate the arctangent of two arbitrary place decimals.
    /// </summary>
    /// <param name="y">Y-value for the atan2 function.</param>
    /// <param name="x">X-value for the atan2 function.</param>
    /// <returns>Arctangent angle in radians.</returns>
    public static abstract double Atan2(TSelf y, TSelf x);

    /// <summary>
    /// Returns the cosine of a value.
    /// </summary>
    /// <param name="value">value to take the cosine of.</param>
    /// <returns>Cosine of the value in radians.</returns>
    public static abstract double Cos(TSelf value);
    
    /// <summary>
    /// Returns the sine of a value.
    /// </summary>
    /// <param name="value">value to take the sine of.</param>
    /// <returns>Sine of the value in radians.</returns>
    public static abstract double Sin(TSelf value);
    
    /// <summary>
    /// Returns the tangent of a value.
    /// </summary>
    /// <param name="value">value to take the tangent of.</param>
    /// <returns>Tangent of the value in radians.</returns>
    public static abstract double Tan(TSelf value);

    /// <summary>
    /// Calculate the minimum value in a set of given arbitrary place decimals.
    /// </summary>
    /// <param name="value">First value.</param>
    /// <param name="values">Subsequent values.</param>
    /// <returns>The minimum value in the set of given values</returns>
    public static abstract TSelf Min(TSelf value, params TSelf[] values);
    
    /// <summary>
    /// Calculate the maximum value in a set of given arbitrary place decimals.
    /// </summary>
    /// <param name="value">First value.</param>
    /// <param name="values">Subsequent values.</param>
    /// <returns>The maximum value in the set of given values</returns>
    public static abstract TSelf Max(TSelf value, params TSelf[] values);

    /// <summary>
    /// Rounds a value to the nearest integer.
    /// </summary>
    /// <param name="value">Value to round.</param>
    /// <param name="mode">Rounding mode for midpoint decimals.</param>
    /// <returns>Value rounded to the nearest integer.</returns>
    public static abstract TSelf Round(TSelf value, MidpointRounding mode);

    /// <summary>
    /// Returns the floor of a value.
    /// </summary>
    /// <param name="value">Value to floor.</param>
    /// <returns>Value rounded down to an integer.</returns>
    public static abstract TSelf Floor(TSelf value);
    
    /// <summary>
    /// Returns the ceiling of a value.
    /// </summary>
    /// <param name="value">Value to ceiling.</param>
    /// <returns>Value rounded up to an integer.</returns>
    public static abstract TSelf Ceiling(TSelf value);
}