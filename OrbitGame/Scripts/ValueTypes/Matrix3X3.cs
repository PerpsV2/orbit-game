using System;
using System.Linq;

namespace OrbitGame;

/// <summary>
/// Value type for a 3x3 matrix.
/// </summary>
public readonly record struct Matrix3X3<T> : IFormattable where T : IArbitraryPlaceDecimal<T>
{
    // Matrix information is organized from left to right then top to bottom
    public readonly T[] Data = new T[9];

    public Matrix3X3(T[] data)
    {
        if (data.Length != 9) 
            throw new ArgumentException("Matrix3x3 data must be of length 9");

        Data = data;
    }
    
    private static Matrix3X3<T> Add(Matrix3X3<T> a, Matrix3X3<T> b)
    {
        T[] resultData = new T[9];
        for (int i = 0; i < 9; ++i)
            resultData[i] = a.Data[i] + b.Data[i];
        return new Matrix3X3<T>(resultData);
    }
    
    #region Operators

    public static Matrix3X3<T> operator +(Matrix3X3<T> value) 
        => value;
    public static Matrix3X3<T> operator -(Matrix3X3<T> value) 
        => new (value.Data.Select(x => -x).ToArray());
    public static Matrix3X3<T> operator +(Matrix3X3<T> left, Matrix3X3<T> right) 
        => Add(left, right);
    public static Matrix3X3<T> operator -(Matrix3X3<T> left, Matrix3X3<T> right) 
        => Add(left, -right);
    public static Matrix3X3<T> operator *(Matrix3X3<T> left, Matrix3X3<T> right)
    {
        T[] resultData = new T[9];
        for (int i = 0; i < 3; ++i)
        for (int j = 0; j < 3; ++j)
            resultData[i * 3 + j] =
                left.Data[i * 3] * right.Data[j] + 
                left.Data[i * 3 + 1] * right.Data[j + 3] + 
                left.Data[i * 3 + 2] * right.Data[j + 6];
        return new Matrix3X3<T>(resultData);
    }
    
    public static Vec2<T> operator *(Matrix3X3<T> matrix, Vec2<T> vector)
    {
        if (matrix.Data[6] != T.Zero || matrix.Data[7] != T.Zero || matrix.Data[8] != T.One)
            throw new ArithmeticException("Matrix3x3 must have identity Z-axis values when multiplying with Vector2");
        return new Vec2<T>(
            matrix.Data[0] * vector.X + matrix.Data[1] * vector.Y + matrix.Data[2],
            matrix.Data[3] * vector.X + matrix.Data[4] * vector.Y + matrix.Data[5]);
    }

    public static Vec3<T> operator *(Matrix3X3<T> matrix, Vec3<T> vector)
    {
        return new Vec3<T>(
            matrix.Data[0] * vector.X + matrix.Data[1] * vector.Y + matrix.Data[2] * vector.Z,
            matrix.Data[3] * vector.X + matrix.Data[4] * vector.Y + matrix.Data[5] * vector.Z,
            matrix.Data[6] * vector.X + matrix.Data[7] * vector.Y + matrix.Data[8] * vector.Z
            );
    }

    public static Matrix3X3<T> operator *(Matrix3X3<T> matrix, T scalar)
    {
        return new Matrix3X3<T>(matrix.Data.Select(x => x * scalar).ToArray());
    }
    
    public static Matrix3X3<T> operator /(Matrix3X3<T> matrix, T scalar)
    {
        return new Matrix3X3<T>(matrix.Data.Select(x => x / scalar).ToArray());
    }
    
    #endregion
    
    #region Transformations
    
    public static Matrix3X3<T> Identity()
    {
        return new Matrix3X3<T>([
            T.One, T.Zero, T.Zero,
            T.Zero, T.One, T.Zero,
            T.Zero, T.Zero, T.One
        ]);
    }

    public static Matrix3X3<T> Translation(T x, T y)
    {
        return new Matrix3X3<T>([
            T.One, T.Zero, x,
            T.Zero, T.One, y,
            T.Zero, T.Zero, T.One
        ]);
    }

    public static Matrix3X3<T> Translation(Vec2<T> vector)
        => Translation(vector.X, vector.Y);
    
    public static Matrix3X3<T> Rotation(double angle)
    {
        return new Matrix3X3<T>([
            T.FromDouble(Math.Cos(angle)), T.FromDouble(-Math.Sin(angle)), T.Zero,
            T.FromDouble(Math.Sin(angle)), T.FromDouble(Math.Cos(angle)), T.Zero,
            T.Zero, T.Zero, T.One
        ]);
    }
    
    public static Matrix3X3<T> Scale(T x, T y)
    {
        return new Matrix3X3<T>([
            x, T.Zero, T.Zero,
            T.Zero, y, T.Zero,
            T.Zero, T.Zero, T.One
        ]);
    }
    
    public static Matrix3X3<T> Scale(Vec2<T> scale)
        => Scale(scale.X, scale.Y);
    
    public static Matrix3X3<T> Scale(T scale)
        => Scale(scale, scale);
    
    #endregion
    
    public bool Equals(Matrix3X3<T> other)
    {
        for (int i = 0; i < 9; ++i)
            if (!Data[i].Equals(other.Data[i])) return false;

        return true;
    }
    
    public override string ToString()
    {
        string result = "";
        for (int i = 0; i < 3; ++i)
            result += $"{Data[i * 3]}, {Data[i * 3 + 1]}, {Data[i * 3 + 2]}\n";
        return result;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
        => ToString();
    
    public override int GetHashCode()
    {
        return Data.GetHashCode();
    }
}