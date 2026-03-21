using System;
using System.Numerics;

namespace OrbitGame;

public interface IArbitraryPlaceDecimal<TSelf> : INumber<TSelf>
    where TSelf : INumber<TSelf>?
{
    public bool Positive { get; }
    public bool Negative { get; }
    public static abstract TSelf PosInfinity { get; }
    public static abstract TSelf NegInfinity { get; }
    
    public static abstract TSelf FromDouble(double value, int exponent = 0);
    public static abstract double ToDouble(TSelf value);
    public static abstract TSelf Sqrt(TSelf value);
    public static abstract double Atan2(TSelf y, TSelf x);
    public TOther Map<TOther>() where TOther : new();
}