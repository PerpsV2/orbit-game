namespace OrbitGame.Tests;

public class ScientificDecimalTests
{
    private ScientificDecimal _testPosScientificDecimal = new(4, 0);
    private ScientificDecimal _testNegScientificDecimal = new(-4, 0);
    private readonly ScientificDecimal _testZeroScientificDecimal = new(0, 0);
    private ScientificDecimal _testPosInfinity = ScientificDecimal.PosInfinity;
    private ScientificDecimal _testNegInfinity = ScientificDecimal.NegInfinity;
    
    [Fact]
    public void Test_SqrtMethod()
    {
        Assert.Equal(2, _testPosScientificDecimal.Sqrt());
        Assert.Equal(_testPosInfinity, _testPosInfinity.Sqrt());
        Assert.Throws<ArithmeticException>(() => _testNegScientificDecimal.Sqrt());
        Assert.Throws<ArithmeticException>(() => _testNegInfinity.Sqrt());
    }

    [Fact]
    public void Test_AbsMethod()
    {
        Assert.Equal(4, _testPosScientificDecimal.Abs());
        Assert.Equal(4, _testNegScientificDecimal.Abs());
        Assert.Equal(_testPosInfinity, _testPosInfinity.Abs());
        Assert.Equal(_testPosInfinity, _testNegInfinity.Abs());
    }

    [Fact]
    public void Test_MinMethod()
    {
        Assert.Equal(-4, ScientificDecimal.Min(_testNegScientificDecimal, _testPosScientificDecimal));
        Assert.Equal(_testNegInfinity, ScientificDecimal.Min(_testPosInfinity, _testNegInfinity));
        Assert.Equal(_testNegInfinity, ScientificDecimal.Min(_testNegInfinity, _testNegScientificDecimal));
    }
    
    [Fact]
    public void Test_MaxMethod()
    {
        Assert.Equal(-4, ScientificDecimal.Max(_testNegInfinity, _testNegScientificDecimal));
        Assert.Equal(4, ScientificDecimal.Max(_testNegScientificDecimal, _testPosScientificDecimal));
        Assert.Equal(_testPosInfinity, ScientificDecimal.Max(_testPosInfinity, _testNegInfinity));
    }

    [Fact]
    public void Test_ClampMethod()
    {
        ScientificDecimal argument = new ScientificDecimal(-8, 0);
        
        Assert.Equal(-4, argument.Clamp(_testNegScientificDecimal, _testPosScientificDecimal));
        Assert.Equal(-4, _testNegInfinity.Clamp(_testNegScientificDecimal, _testPosScientificDecimal));
        Assert.Equal(4, _testPosInfinity.Clamp(_testNegScientificDecimal, _testPosScientificDecimal));
        Assert.Equal(_testPosInfinity, _testPosInfinity.Clamp(_testNegScientificDecimal, _testPosInfinity));
        Assert.Throws<ArithmeticException>(() => argument.Clamp(_testPosScientificDecimal, _testNegScientificDecimal));
    }

    [Fact]
    public void Test_RoundMethod()
    {
        ScientificDecimal argument = new(0.00001, 0);
        Assert.Equal(0, argument.Round());
        argument = new(0.9, 0);
        Assert.Equal(1, argument.Round());
        argument = new(1.1, 0);
        Assert.Equal(1, argument.Round());
        argument = new(10000.9, 0);
        Assert.Equal(10001, argument.Round(), Assert.Epsilon);
    }
    
    #region Operators
    
    [Fact]
    public void Test_NegativeOperator()
    {
        Assert.Equal(-4, -_testPosScientificDecimal);
        Assert.Equal(_testNegInfinity, -_testPosInfinity);
    }
    
    [Fact]
    public void Test_AdditionOperator()
    {
        Assert.Equal(0, _testPosScientificDecimal + _testNegScientificDecimal);
        Assert.Equal(_testPosInfinity, _testPosScientificDecimal + _testPosInfinity);
        Assert.Equal(_testPosInfinity, _testPosInfinity + _testPosInfinity);
        Assert.Throws<ArithmeticException>(() => _testPosInfinity + _testNegInfinity);
    }

    [Fact]
    public void Test_SubtractionOperator()
    {
        Assert.Equal(8, _testPosScientificDecimal - _testNegScientificDecimal);
        Assert.Equal(_testPosInfinity, _testPosScientificDecimal - _testNegInfinity);
        Assert.Throws<ArithmeticException>(() => _testPosInfinity - _testPosInfinity);
    }

    [Fact]
    public void Test_MultiplicationOperator()
    {
        Assert.Equal(-16, _testPosScientificDecimal * _testNegScientificDecimal);
        Assert.Equal(0, _testZeroScientificDecimal * _testPosInfinity);
        Assert.Equal(_testNegInfinity, _testNegScientificDecimal * _testPosInfinity);
        Assert.Equal(_testNegInfinity, _testPosInfinity * _testNegInfinity);
        Assert.Equal(_testPosInfinity, _testNegInfinity * _testNegInfinity);
    }

    [Fact]
    public void Test_DivisionOperator()
    {
        Assert.Equal(-1, _testPosScientificDecimal / _testNegScientificDecimal);
        Assert.Equal(0, _testPosScientificDecimal / _testPosInfinity);
        Assert.Equal(_testNegInfinity, _testPosInfinity / _testNegScientificDecimal);
        Assert.Equal(_testNegInfinity, _testNegScientificDecimal / _testZeroScientificDecimal);
        Assert.Throws<ArithmeticException>(() => _testZeroScientificDecimal / _testZeroScientificDecimal);
        Assert.Throws<ArithmeticException>(() => _testPosInfinity / _testNegInfinity);
    }

    [Fact]
    public void Test_ModuloOperator()
    {
        Assert.Equal(2, new ScientificDecimal(2, 0) % new ScientificDecimal(4, 0));
        Assert.Equal(0, new ScientificDecimal(4, 0) % new ScientificDecimal(2, 0));
        Assert.Throws<ArithmeticException>(() => _testPosInfinity % _testPosScientificDecimal);
        Assert.Throws<ArithmeticException>(() => _testPosScientificDecimal % _testPosInfinity);
        Assert.Throws<ArithmeticException>(() => _testPosScientificDecimal % _testZeroScientificDecimal);
        Assert.Equal(1, new ScientificDecimal(10) % 3, 0.00001);
    }
    
    [Fact]
    public void Test_EqualsOperator()
    {
        // ReSharper disable once EqualExpressionComparison
        Assert.True(_testNegInfinity == _testNegInfinity);
        Assert.False(_testPosScientificDecimal == _testNegScientificDecimal);
        Assert.False(_testPosScientificDecimal == _testPosInfinity);
    }

    [Fact]
    public void Test_GreaterThanOperator()
    {
        // ReSharper disable once EqualExpressionComparison
        Assert.False(_testPosInfinity > _testPosInfinity);
        Assert.True(_testPosInfinity > _testNegInfinity);
        Assert.True(_testPosScientificDecimal > _testNegScientificDecimal);
    }

    [Fact]
    public void Test_LessThanOperator()
    {
        // ReSharper disable once EqualExpressionComparison
        Assert.False(_testPosInfinity < _testPosInfinity);
        Assert.True(_testNegScientificDecimal < _testPosScientificDecimal);
        Assert.False(_testPosInfinity < _testPosScientificDecimal);
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