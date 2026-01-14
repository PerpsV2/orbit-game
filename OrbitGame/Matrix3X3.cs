namespace OrbitGame;
using MatrixData = ScientificDecimal[];

public class Matrix3X3 : IEquatable<Matrix3X3>, IFormattable
{
    // Matrix information is organized from left to right then top to bottom
    public readonly MatrixData Data = new ScientificDecimal[9];

    public Matrix3X3(ScientificDecimal[] data)
    {
        if (data.Length != 9) 
            throw new ArgumentException("Matrix3x3 data must be of length 9");

        Data = data;
    }

    public Matrix3X3(Vector3 basisX, Vector3 basisY, Vector3 basisZ)
    {
        Data[0] = basisX.X;
        Data[3] = basisX.Y;
        Data[6] = basisX.Z;

        Data[1] = basisY.X;
        Data[4] = basisY.Y;
        Data[7] = basisY.Z;

        Data[2] = basisZ.X;
        Data[5] = basisZ.Y;
        Data[8] = basisZ.Z;
    }

    private static Matrix3X3 Add(Matrix3X3 a, Matrix3X3 b)
    {
        MatrixData resultData = new ScientificDecimal[9];
        for (int i = 0; i < 9; ++i)
            resultData[i] = a.Data[i] + b.Data[i];
        return new Matrix3X3(resultData);
    }

    public static Matrix3X3 operator +(Matrix3X3 m) => m;
    public static Matrix3X3 operator -(Matrix3X3 m) 
        => new (m.Data.Select(x => -x).ToArray());
    public static Matrix3X3 operator +(Matrix3X3 a, Matrix3X3 b) => Add(a, b);
    public static Matrix3X3 operator -(Matrix3X3 a, Matrix3X3 b) => Add(a, -b);
    public static Matrix3X3 operator *(Matrix3X3 a, Matrix3X3 b)
    {
        MatrixData resultData = new ScientificDecimal[9];
        for (int i = 0; i < 3; ++i)
        for (int j = 0; j < 3; ++j)
            resultData[i * 3 + j] =
                a.Data[i * 3] * b.Data[j] + a.Data[i * 3 + 1] * b.Data[j + 3] + a.Data[i * 3 + 2] * b.Data[j + 6];
        return new Matrix3X3(resultData);
    }
    
    public static Vector2 operator *(Matrix3X3 matrix, Vector2 vector)
    {
        return new Vector2(
            matrix.Data[0] * vector.X + matrix.Data[1] * vector.Y + matrix.Data[2],
            matrix.Data[3] * vector.X + matrix.Data[4] * vector.Y + matrix.Data[5]);
    }

    public static Vector3 operator *(Matrix3X3 matrix, Vector3 vector)
    {
        
    }
    
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

    public bool Equals(Matrix3X3? other)
    {
        if (other == null) return false;
        if (Data == other.Data) return true;
        return false;
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
}