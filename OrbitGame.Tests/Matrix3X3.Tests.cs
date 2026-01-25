namespace OrbitGame.Tests;

public class Matrix3X3_Tests
{
    #region Operators
    
    [Fact]
    public void Matrix3X3_NegativeOperator()
    {
        Matrix3X3 argument = new Matrix3X3([0, 1, -2, 3, -4, 5, -6, 7, -8]);
        Assert.Equal(new Matrix3X3([0, -1, 2, -3, 4, -5, 6, -7, 8]), -argument);
    }
    
    [Fact]
    public void Matrix3X3_AdditionOperator()
    {
        Matrix3X3 argument1 = new Matrix3X3([0, 1, 2, 3, 4, 5, 6, 7, 8]);
        Matrix3X3 argument2 = new Matrix3X3([8, 7, 6, 5, 4, 3, 2, 1, 0]);
        
        Assert.Equal(new Matrix3X3([8, 8, 8, 8, 8, 8, 8, 8, 8]), argument1 + argument2);
    }

    [Fact]
    public void Matrix3X3_SubtractionOperator()
    {
        Matrix3X3 argument1 = new Matrix3X3([8, 8, 8, 8, 8, 8, 8, 8, 8]);
        Matrix3X3 argument2 = new Matrix3X3([0, 1, 2, 3, 4, 5, 6, 7, 8]);
        
        Assert.Equal(new Matrix3X3([8, 7, 6, 5, 4, 3, 2, 1, 0]), argument1 - argument2);
    }
    
    [Fact]
    public void Matrix3X3_MatrixMultiplicationOperator()
    {
        Matrix3X3 argument1 = new Matrix3X3([0, 1, 2, 3, 4, 5, 6, 7, 8]);
        Matrix3X3 argument2 = new Matrix3X3([8, 7, 6, 5, 4, 3, 2, 1, 0]);
        
        Assert.Equal(new Matrix3X3([9, 6, 3, 54, 42, 30, 99, 78, 57]), argument1 * argument2);
        Assert.Equal(new Matrix3X3([57, 78, 99, 30, 42, 54, 3, 6, 9]), argument2 * argument1);
    }

    [Fact]
    public void Matrix3X3_Vector2MultiplicationOperator()
    {
        Matrix3X3 matrix1 = new Matrix3X3([0, 1, 2, 3, 4, 5, 0, 0, 1]);
        Matrix3X3 matrix2 = new Matrix3X3([0, 1, 2, 3, 4, 5, 6, 7, 8]);
        Vector2 vector = new Vector2(1, -1);
        
        Assert.Equal(new Vector2(1, 4), matrix1 * vector);
        Assert.Throws<ArithmeticException>(() => matrix2 * vector);
    }

    [Fact]
    public void Matrix3X3_Vector3MultiplicationOperator()
    {
        Matrix3X3 matrix = new Matrix3X3([0, 1, 2, 3, 4, 5, 6, 7, 8]);
        Vector3 vector = new Vector3(1, -1, 2);
        
        Assert.Equal(new Vector3(3, 9, 15), matrix * vector);
    }

    [Fact]
    public void Matrix3X3_ScalarMultiplicationOperator()
    {
        Matrix3X3 matrix = new Matrix3X3([1, 0, 0, 0, 1, 0, 0, 0, 1]);
        ScientificDecimal scalar = -5;
        
        Assert.Equal(new Matrix3X3([-5, 0, 0, 0, -5, 0, 0, 0, -5]), matrix * scalar);
    }
    
    [Fact]
    public void Matrix3X3_ScalarDivisionOperator()
    {
        Matrix3X3 matrix = new Matrix3X3([5, 0, 0, 0, 5, 0, 0, 0, 5]);
        ScientificDecimal scalar = 5;
        
        Assert.Equal(new Matrix3X3([1, 0, 0, 0, 1, 0, 0, 0, 1]), matrix / scalar);
    }
    
    #endregion
    
    #region Transformations

    [Fact]
    public void Matrix3X3_TranslationMethod()
    {
        Vector2 translation = new Vector2(2, -2);
        Assert.Equal(new Matrix3X3([1, 0, 2, 0, 1, -2, 0, 0, 1]), Matrix3X3.Translation(translation));
    }

    [Fact]
    public void Matrix3X3_RotationMethod()
    {
        double angle = Math.PI / 2;
        AssertExtensions.Matrix3X3Equals(new Matrix3X3([0, -1, 0, 1, 0, 0, 0, 0, 1]), Matrix3X3.Rotation(angle));
    }

    [Fact]
    public void Matrix3X3_ScaleMethod()
    {
        Vector2 scale = new Vector2(2, -2);
        AssertExtensions.Matrix3X3Equals(new Matrix3X3([2, 0, 0, 0, -2, 0, 0, 0, 1]), Matrix3X3.Scale(scale));
    }
    
    #endregion
}