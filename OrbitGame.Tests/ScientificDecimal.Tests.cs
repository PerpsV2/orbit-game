namespace OrbitGame.Tests;

public class ScientificDecimalTests
{
    private ScientificDecimal _posNum = new ScientificDecimal(4, 0);
    private ScientificDecimal _negNum = new ScientificDecimal(-4, 0);
    private ScientificDecimal _zero = new ScientificDecimal(0, 0);
    private ScientificDecimal _posInfinity = ScientificDecimal.PosInfinity;
    private ScientificDecimal _negInfinity = ScientificDecimal.NegInfinity;
    
    [Fact]
    public void Test_SqrtMethod()
    {
        Assert.Equal(2, _posNum.Sqrt());
        Assert.Equal(_posInfinity, _posInfinity.Sqrt());
        Assert.Throws<ArithmeticException>(() => _negNum.Sqrt());
        Assert.Throws<ArithmeticException>(() => _negInfinity.Sqrt());
    }

    [Fact]
    public void Test_AbsMethod()
    {
        Assert.Equal(4, _posNum.Abs());
        Assert.Equal(4, _negNum.Abs());
        Assert.Equal(_posInfinity, _posInfinity.Abs());
        Assert.Equal(_posInfinity, _negInfinity.Abs());
    }

    [Fact]
    public void Test_MinMethod()
    {
        Assert.Equal(-4, ScientificDecimal.Min(_negNum, _posNum));
        Assert.Equal(_negInfinity, ScientificDecimal.Min(_posInfinity, _negInfinity));
        Assert.Equal(_negInfinity, ScientificDecimal.Min(_negInfinity, _negNum));
    }
    
    [Fact]
    public void Test_MaxMethod()
    {
        Assert.Equal(-4, ScientificDecimal.Max(_negInfinity, _negNum));
        Assert.Equal(4, ScientificDecimal.Max(_negNum, _posNum));
        Assert.Equal(_posInfinity, ScientificDecimal.Max(_posInfinity, _negInfinity));
    }

    [Fact]
    public void Test_ClampMethod()
    {
        ScientificDecimal argument = new ScientificDecimal(-8, 0);
        
        Assert.Equal(-4, argument.Clamp(_negNum, _posNum));
        Assert.Equal(-4, _negInfinity.Clamp(_negNum, _posNum));
        Assert.Equal(4, _posInfinity.Clamp(_negNum, _posNum));
        Assert.Equal(_posInfinity, _posInfinity.Clamp(_negNum, _posInfinity));
        Assert.Throws<ArithmeticException>(() => argument.Clamp(_posNum, _negNum));
    }
    
    #region Operators
    
    [Fact]
    public void Test_NegativeOperator()
    {
        Assert.Equal(-4, -_posNum);
        Assert.Equal(_negInfinity, -_posInfinity);
    }
    
    [Fact]
    public void Test_AdditionOperator()
    {
        Assert.Equal(0, _posNum + _negNum);
        Assert.Equal(_posInfinity, _posNum + _posInfinity);
        Assert.Equal(_posInfinity, _posInfinity + _posInfinity);
        Assert.Throws<ArithmeticException>(() => _posInfinity + _negInfinity);
    }

    [Fact]
    public void Test_SubtractionOperator()
    {
        Assert.Equal(8, _posNum - _negNum);
        Assert.Equal(_posInfinity, _posNum - _negInfinity);
        Assert.Throws<ArithmeticException>(() => _posInfinity - _posInfinity);
    }

    [Fact]
    public void Test_MultiplicationOperator()
    {
        Assert.Equal(-16, _posNum * _negNum);
        Assert.Equal(0, _zero * _posInfinity);
        Assert.Equal(_negInfinity, _negNum * _posInfinity);
        Assert.Equal(_negInfinity, _posInfinity * _negInfinity);
        Assert.Equal(_posInfinity, _negInfinity * _negInfinity);
    }

    [Fact]
    public void Test_DivisionOperator()
    {
        Assert.Equal(-1, _posNum / _negNum);
        Assert.Equal(0, _posNum / _posInfinity);
        Assert.Equal(_negInfinity, _posInfinity / _negNum);
        Assert.Equal(_negInfinity, _negNum / _zero);
        Assert.Throws<ArithmeticException>(() => _zero / _zero);
        Assert.Throws<ArithmeticException>(() => _posInfinity / _negInfinity);
    }
    
    [Fact]
    public void Test_EqualsOperator()
    {
        // ReSharper disable once EqualExpressionComparison
        Assert.True(_negInfinity == _negInfinity);
        Assert.False(_posNum == _negNum);
        Assert.False(_posNum == _posInfinity);
    }

    [Fact]
    public void Test_GreaterThanOperator()
    {
        // ReSharper disable once EqualExpressionComparison
        Assert.False(_posInfinity > _posInfinity);
        Assert.True(_posInfinity > _negInfinity);
        Assert.True(_posNum > _negNum);
    }

    [Fact]
    public void Test_LessThanOperator()
    {
        // ReSharper disable once EqualExpressionComparison
        Assert.False(_posInfinity < _posInfinity);
        Assert.True(_negNum < _posNum);
        Assert.False(_posInfinity < _posNum);
    }
    
    #endregion Operators
    
    #region Casts

    [Fact]
    public void Test_DoubleCast()
    {
        ScientificDecimal argument = new ScientificDecimal(-1.59, 1);
        
        Assert.Equal(-15.9d, (double)argument);
    }
    
    [Fact]
    public void Test_IntCast()
    {
        ScientificDecimal posArgument = new ScientificDecimal(1.59, 1);
        ScientificDecimal negArgument = new ScientificDecimal(-1.59, 1);
        
        Assert.Equal(15, (int)posArgument);
        Assert.Equal(-15, (int)negArgument);
    }
    
    #endregion Casts
}