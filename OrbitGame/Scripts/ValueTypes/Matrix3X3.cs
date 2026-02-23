using System;
using System.Linq;

namespace OrbitGame;
using MatrixData = ScientificDecimal[];

/// <summary>
/// Value type for a 3x3 matrix.
/// </summary>
public readonly record struct Matrix3X3 : IFormattable
{
    // Matrix information is organized from left to right then top to bottom
    public readonly MatrixData Data = new ScientificDecimal[9];

    public Matrix3X3(ScientificDecimal[] data)
    {
        if (data.Length != 9) 
            throw new ArgumentException("Matrix3x3 data must be of length 9");

        Data = data;
    }
    
    private static Matrix3X3 Add(Matrix3X3 a, Matrix3X3 b)
    {
        MatrixData resultData = new ScientificDecimal[9];
        for (int i = 0; i < 9; ++i)
            resultData[i] = a.Data[i] + b.Data[i];
        return new Matrix3X3(resultData);
    }
    
    #region Operators

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
        MatrixData resultData = new ScientificDecimal[9];
        for (int i = 0; i < 3; ++i)
        for (int j = 0; j < 3; ++j)
            resultData[i * 3 + j] =
                left.Data[i * 3] * right.Data[j] + 
                left.Data[i * 3 + 1] * right.Data[j + 3] + 
                left.Data[i * 3 + 2] * right.Data[j + 6];
        return new Matrix3X3(resultData);
    }
    
    public static SD_Vector2 operator *(Matrix3X3 matrix, SD_Vector2 vector)
    {
        if (matrix.Data[6] != 0 || matrix.Data[7] != 0 || matrix.Data[8] != 1)
            throw new ArithmeticException("Matrix3x3 must have identity Z-axis values when multiplying with Vector2");
        return new SD_Vector2(
            matrix.Data[0] * vector.X + matrix.Data[1] * vector.Y + matrix.Data[2],
            matrix.Data[3] * vector.X + matrix.Data[4] * vector.Y + matrix.Data[5]);
    }

    public static SD_Vector3 operator *(Matrix3X3 matrix, SD_Vector3 vector)
    {
        return new SD_Vector3(
            matrix.Data[0] * vector.X + matrix.Data[1] * vector.Y + matrix.Data[2] * vector.Z,
            matrix.Data[3] * vector.X + matrix.Data[4] * vector.Y + matrix.Data[5] * vector.Z,
            matrix.Data[6] * vector.X + matrix.Data[7] * vector.Y + matrix.Data[8] * vector.Z
            );
    }

    public static Matrix3X3 operator *(Matrix3X3 matrix, ScientificDecimal scalar)
    {
        return new Matrix3X3(matrix.Data.Select(x => x * scalar).ToArray());
    }
    
    public static Matrix3X3 operator /(Matrix3X3 matrix, ScientificDecimal scalar)
    {
        return new Matrix3X3(matrix.Data.Select(x => x / scalar).ToArray());
    }
    
    #endregion
    
    #region Transformations
    
    public static Matrix3X3 Identity()
    {
        return new Matrix3X3([
            1, 0, 0,
            0, 1, 0,
            0, 0, 1
        ]);
    }

    public static Matrix3X3 Translation(ScientificDecimal x, ScientificDecimal y)
    {
        return new Matrix3X3([
            1, 0, x,
            0, 1, y,
            0, 0, 1
        ]);
    }

    public static Matrix3X3 Translation(SD_Vector2 vector)
        => Translation(vector.X, vector.Y);
    
    public static Matrix3X3 Rotation(double angle)
    {
        return new Matrix3X3([
            Math.Cos(angle), -Math.Sin(angle), 0,
            Math.Sin(angle), Math.Cos(angle), 0,
            0, 0, 1
        ]);
    }
    
    public static Matrix3X3 Scale(ScientificDecimal x, ScientificDecimal y)
    {
        return new Matrix3X3([
            x, 0, 0,
            0, y, 0,
            0, 0, 1
        ]);
    }
    
    public static Matrix3X3 Scale(SD_Vector2 scale)
        => Scale(scale.X, scale.Y);
    
    public static Matrix3X3 Scale(ScientificDecimal scale)
        => Scale(scale, scale);
    
    #endregion
    
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