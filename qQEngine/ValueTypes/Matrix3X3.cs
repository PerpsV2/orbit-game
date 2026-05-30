namespace qQEngine;

/// <summary>
/// Value type for a 3x3 matrix.
/// </summary>
public readonly record struct Matrix3X3 : IFormattable
{
    // Matrix information is organized from left to right then top to bottom
    public readonly SDecimal[] Data = new SDecimal[9];

    public Matrix3X3(SDecimal[] data)
    {
        if (data.Length != 9) 
            throw new ArgumentException("Matrix3x3 data must be of length 9");

        Data = data;
    }
    
    private static Matrix3X3 Add(Matrix3X3 a, Matrix3X3 b)
    {
        SDecimal[] resultData = new SDecimal[9];
        for (int i = 0; i < 9; ++i)
            resultData[i] = a.Data[i] + b.Data[i];
        return new Matrix3X3(resultData);
    }

    public static Matrix3X3 operator +(Matrix3X3 value) 
        => value;
    public static Matrix3X3 operator -(Matrix3X3 value) 
        => new (value.Data.Select(x => -x).ToArray());
    public static Matrix3X3 operator +(Matrix3X3 left, Matrix3X3 right) 
        => Add(left, right);
    public static Matrix3X3 operator -(Matrix3X3 left, Matrix3X3 right) 
        => Add(left, -right);
    public static Matrix3X3 operator *(Matrix3X3 left, Matrix3X3 right)
    {
        SDecimal[] resultData = new SDecimal[9];
        for (int i = 0; i < 3; ++i)
        for (int j = 0; j < 3; ++j)
            resultData[i * 3 + j] =
                left.Data[i * 3] * right.Data[j] + 
                left.Data[i * 3 + 1] * right.Data[j + 3] + 
                left.Data[i * 3 + 2] * right.Data[j + 6];
        return new Matrix3X3(resultData);
    }
    
    public static Vec2 operator *(Matrix3X3 matrix, Vec2 vector)
    {
        if (matrix.Data[6] != SDecimal.Zero || matrix.Data[7] != SDecimal.Zero || matrix.Data[8] != SDecimal.One)
            throw new ArithmeticException("Matrix3x3 must have identity Z-axis values when multiplying with Vector2");
        return new Vec2(
            matrix.Data[0] * vector.X + matrix.Data[1] * vector.Y + matrix.Data[2],
            matrix.Data[3] * vector.X + matrix.Data[4] * vector.Y + matrix.Data[5]);
    }
    
    public static Vec2Double operator *(Matrix3X3 matrix, Vec2Double vector)
    {
        if (matrix.Data[6] != SDecimal.Zero || matrix.Data[7] != SDecimal.Zero || matrix.Data[8] != SDecimal.One)
            throw new ArithmeticException("Matrix3x3 must have identity Z-axis values when multiplying with Vector2");
        return new Vec2Double(
            (double)matrix.Data[0] * vector.X + (double)matrix.Data[1] * vector.Y + (double)matrix.Data[2],
            (double)matrix.Data[3] * vector.X + (double)matrix.Data[4] * vector.Y + (double)matrix.Data[5]);
    }

    public static Vec3 operator *(Matrix3X3 matrix, Vec3 vector)
    {
        return new Vec3(
            matrix.Data[0] * vector.X + matrix.Data[1] * vector.Y + matrix.Data[2] * vector.Z,
            matrix.Data[3] * vector.X + matrix.Data[4] * vector.Y + matrix.Data[5] * vector.Z,
            matrix.Data[6] * vector.X + matrix.Data[7] * vector.Y + matrix.Data[8] * vector.Z
            );
    }

    public static Matrix3X3 operator *(Matrix3X3 matrix, SDecimal scalar)
    {
        return new Matrix3X3(matrix.Data.Select(x => x * scalar).ToArray());
    }
    
    public static Matrix3X3 operator /(Matrix3X3 matrix, SDecimal scalar)
    {
        return new Matrix3X3(matrix.Data.Select(x => x / scalar).ToArray());
    }
    
    public static Matrix3X3 Identity()
    {
        return new Matrix3X3([
            SDecimal.One, SDecimal.Zero, SDecimal.Zero,
            SDecimal.Zero, SDecimal.One, SDecimal.Zero,
            SDecimal.Zero, SDecimal.Zero, SDecimal.One
        ]);
    }

    public static Matrix3X3 Translation(SDecimal x, SDecimal y)
    {
        return new Matrix3X3([
            SDecimal.One, SDecimal.Zero, x,
            SDecimal.Zero, SDecimal.One, y,
            SDecimal.Zero, SDecimal.Zero, SDecimal.One
        ]);
    }

    public static Matrix3X3 Translation(Vec2 vector)
        => Translation(vector.X, vector.Y);
    
    public static Matrix3X3 Rotation(double angle)
    {
        return new Matrix3X3([
            SDecimal.Cos(angle), -SDecimal.Sin(angle), SDecimal.Zero,
            SDecimal.Sin(angle), SDecimal.Cos(angle), SDecimal.Zero,
            SDecimal.Zero, SDecimal.Zero, SDecimal.One
        ]);
    }
    
    public static Matrix3X3 Scale(SDecimal x, SDecimal y)
    {
        return new Matrix3X3([
            x, SDecimal.Zero, SDecimal.Zero,
            SDecimal.Zero, y, SDecimal.Zero,
            SDecimal.Zero, SDecimal.Zero, SDecimal.One
        ]);
    }
    
    public static Matrix3X3 Scale(Vec2 scale)
        => Scale(scale.X, scale.Y);
    
    public static Matrix3X3 Scale(SDecimal scale)
        => Scale(scale, scale);
    
    public bool Equals(Matrix3X3 other)
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