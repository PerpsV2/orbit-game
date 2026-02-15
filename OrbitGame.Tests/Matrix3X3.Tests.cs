namespace OrbitGame.Tests;

public class Matrix3X3_Tests
{
    private readonly Matrix3X3 _matrix1 = new Matrix3X3([0, 1, 2, 3, 4, 5, 6, 7, 8]);
    private readonly Matrix3X3 _matrix2 = new Matrix3X3([8, 7, 6, 5, 4, 3, 2, 1, 0]);
    private readonly Matrix3X3 _matrix3 = new Matrix3X3([0, 1, 2, 3, 4, 5, 0, 0, 1]);
    private readonly SD_Vector2 _vector2 = new SD_Vector2(1, -1);
    private readonly SD_Vector3 _vector3 = new SD_Vector3(1, -1, 2);
    private readonly ScientificDecimal _scalar1 = 2;
    
    #region Operators
    
    [Fact]
    public void Matrix3X3_NegativeOperator()
    {
        Assert.Equal(new Matrix3X3([0, -1, -2, -3, -4, -5, -6, -7, -8]), -_matrix1);
    }
    
    [Fact]
    public void Matrix3X3_AdditionOperator()
    {
        Assert.Equal(new Matrix3X3([8, 8, 8, 8, 8, 8, 8, 8, 8]), _matrix1 + _matrix2);
    }

    [Fact]
    public void Matrix3X3_SubtractionOperator()
    {
        Assert.Equal(new Matrix3X3([-8, -6, -4, -2, 0, 2, 4, 6, 8]), _matrix1 - _matrix2);
    }
    
    [Fact]
    public void Matrix3X3_MatrixMultiplicationOperator()
    {
        Assert.Equal(new Matrix3X3([9, 6, 3, 54, 42, 30, 99, 78, 57]), _matrix1 * _matrix2);
        Assert.Equal(new Matrix3X3([57, 78, 99, 30, 42, 54, 3, 6, 9]), _matrix2 * _matrix1);
    }

    [Fact]
    public void Matrix3X3_Vector2MultiplicationOperator()
    {
        Assert.Equal(new SD_Vector2(1, 4), _matrix3 * _vector2);
        Assert.Throws<ArithmeticException>(() => _matrix1 * _vector2);
    }

    [Fact]
    public void Matrix3X3_Vector3MultiplicationOperator()
    {
        Assert.Equal(new SD_Vector3(3, 9, 15), _matrix1 * _vector3);
    }

    [Fact]
    public void Matrix3X3_ScalarMultiplicationOperator()
    {
        Assert.Equal(new Matrix3X3([0, 2, 4, 6, 8, 10, 12, 14, 16]), _matrix1 * _scalar1);
    }
    
    [Fact]
    public void Matrix3X3_ScalarDivisionOperator()
    {
        Assert.Equal(new Matrix3X3([0, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4]), _matrix1 / _scalar1);
    }
    
    #endregion
    
    #region Transformations

    [Fact]
    public void Matrix3X3_TranslationMethod()
    {
        SD_Vector2 translation = new SD_Vector2(2, -2);
        Assert.Equal(new Matrix3X3([1, 0, 2, 0, 1, -2, 0, 0, 1]), Matrix3X3.Translation(translation));
    }

    [Fact]
    public void Matrix3X3_RotationMethod()
    {
        double angle = Math.PI / 2;
        Assert.Equal(new Matrix3X3([0, -1, 0, 1, 0, 0, 0, 0, 1]), Matrix3X3.Rotation(angle));
    }

    [Fact]
    public void Matrix3X3_ScaleMethod()
    {
        SD_Vector2 scale = new SD_Vector2(2, -2);
        Assert.Equal(new Matrix3X3([2, 0, 0, 0, -2, 0, 0, 0, 1]), Matrix3X3.Scale(scale));
    }
    
    #endregion
}