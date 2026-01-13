using System.Reflection.Metadata;

namespace OrbitGame;

public class Matrix3x3
{
    // Matrix information is organized from left to right then top to bottom
    public ScientificDecimal[] Data = new ScientificDecimal[9];

    public Matrix3x3(ScientificDecimal[] data)
    {
        if (data.Length != 9) 
            throw new ArgumentException("Matrix3x3 data must be of length 9");

        Data = data;
    }

    public Matrix3x3(Vector3 basisX, Vector3 basisY, Vector3 basisZ)
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

    static Matrix3x3 Identity()
    {
        return new Matrix3x3([
            1, 0, 0,
            0, 1, 0,
            0, 0, 1
        ]);
    }

    static Matrix3x3 Translation(ScientificDecimal x, ScientificDecimal y)
    {
        return new Matrix3x3([
            1, 0, x,
            0, 1, y,
            0, 0, 1
        ]);
    }
    
    static Matrix3x3 Rotation(double angle)
    {
        return new Matrix3x3([
            Math.Cos(angle), -Math.Sin(angle), 0,
            Math.Sin(angle), Math.Cos(angle), 0,
            0, 0, 1
        ]);
    }
    
    static Matrix3x3 Scale(ScientificDecimal x, ScientificDecimal y)
    {
        return new Matrix3x3([
            x, 0, 0,
            0, y, 0,
            0, 0, 1
        ]);
    }
}