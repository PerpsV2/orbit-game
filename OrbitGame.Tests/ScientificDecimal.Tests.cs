namespace OrbitGame.Tests;
using OrbitGame;

public class ScientificDecimalTests
{
    [Fact]
    public void Test_SqrtMethod()
    {
        ScientificDecimal posRadicand = new ScientificDecimal(4, 0);
        ScientificDecimal negRadicand = new ScientificDecimal(-4, 0);
        ScientificDecimal posInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal negInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(2, posRadicand.Sqrt());
        Assert.Equal(posInfinity, posInfinity.Sqrt());
        Assert.Throws<ArithmeticException>(() => negRadicand.Sqrt());
        Assert.Throws<ArithmeticException>(() => negInfinity.Sqrt());
    }

    [Fact]
    public void Test_AbsMethod()
    {
        ScientificDecimal posArgument = new ScientificDecimal(4, 0);
        ScientificDecimal negArgument = new ScientificDecimal(-4, 0);
        ScientificDecimal posInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal negInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(4, posArgument.Abs());
        Assert.Equal(4, negArgument.Abs());
        Assert.Equal(posInfinity, posInfinity.Abs());
        Assert.Equal(posInfinity, negInfinity.Abs());
    }

    [Fact]
    public void Test_MinMethod()
    {
        ScientificDecimal posNum = new ScientificDecimal(2, 0);
        ScientificDecimal negNum = new ScientificDecimal(-4, 0);
        ScientificDecimal posInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal negInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(-4, ScientificDecimal.Min(negNum, posNum));
        Assert.Equal(negInfinity, ScientificDecimal.Min(posInfinity, negInfinity));
        Assert.Equal(negInfinity, ScientificDecimal.Min(negInfinity, negNum));
    }
    
    [Fact]
    public void Test_MaxMethod()
    {
        
        ScientificDecimal posNum = new ScientificDecimal(4, 0);
        ScientificDecimal negNum = new ScientificDecimal(-8, 0);
        ScientificDecimal posInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal negInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(-8, ScientificDecimal.Max(negInfinity, negNum));
        Assert.Equal(4, ScientificDecimal.Max(negNum, posNum));
        Assert.Equal(posInfinity, ScientificDecimal.Max(posInfinity, negInfinity));
    }

    [Fact]
    public void Test_ClampMethod()
    {
        ScientificDecimal posNum = new ScientificDecimal(2, 0);
        ScientificDecimal negNum = new ScientificDecimal(-4, 0);
        ScientificDecimal argument = new ScientificDecimal(-8, 0);
        ScientificDecimal posInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal negInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(-4, argument.Clamp(negNum, posNum));
        Assert.Equal(-4, negInfinity.Clamp(negNum, posNum));
        Assert.Equal(2, posInfinity.Clamp(negNum, posNum));
        Assert.Equal(posInfinity, posInfinity.Clamp(negNum, posInfinity));
        Assert.Throws<ArithmeticException>(() => argument.Clamp(posNum, negNum));
    }
    
    #region Operators
    
    [Fact]
    public void Test_NegativeOperator()
    {
        ScientificDecimal argument = new ScientificDecimal(2, 0);
        ScientificDecimal posInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal negInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(-2, -argument);
        Assert.Equal(negInfinity, -posInfinity);
    }
    
    [Fact]
    public void Test_AdditionOperator()
    {
        ScientificDecimal argument1 = new ScientificDecimal(2, 0);
        ScientificDecimal argument2 = new ScientificDecimal(-4, 0);
        ScientificDecimal posInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal negInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(-2, argument1 + argument2);
        Assert.Equal(posInfinity, argument1 + posInfinity);
        Assert.Equal(posInfinity, posInfinity + posInfinity);
        Assert.Throws<ArithmeticException>(() => posInfinity + negInfinity);
    }

    [Fact]
    public void Test_SubtractionOperator()
    {
        ScientificDecimal argument1 = new ScientificDecimal(2, 0);
        ScientificDecimal argument2 = new ScientificDecimal(-4, 0);
        ScientificDecimal posInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal negInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(6, argument1 - argument2);
        Assert.Equal(posInfinity, argument1 - negInfinity);
        Assert.Throws<ArithmeticException>(() => posInfinity - posInfinity);
    }

    [Fact]
    public void Test_MultiplicationOperator()
    {
        ScientificDecimal zero = new ScientificDecimal(0, 0);
        ScientificDecimal argument1 = new ScientificDecimal(2, 0);
        ScientificDecimal argument2 = new ScientificDecimal(-4, 0);
        ScientificDecimal posInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal negInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(-8, argument1 * argument2);
        Assert.Equal(0, zero * posInfinity);
        Assert.Equal(negInfinity, argument2 * posInfinity);
        Assert.Equal(negInfinity, posInfinity * negInfinity);
        Assert.Equal(posInfinity, negInfinity * negInfinity);
    }

    [Fact]
    public void Test_DivisionOperator()
    {
        ScientificDecimal zero = new ScientificDecimal(0, 0);
        ScientificDecimal argument1 = new ScientificDecimal(2, 0);
        ScientificDecimal argument2 = new ScientificDecimal(-4, 0);
        ScientificDecimal posInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal negInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(-0.5, argument1 / argument2);
        Assert.Equal(0, argument1 / posInfinity);
        Assert.Equal(negInfinity, posInfinity / argument2);
        Assert.Throws<ArithmeticException>(() => argument1 / zero);
        Assert.Throws<ArithmeticException>(() => posInfinity / negInfinity);
    }
    
    [Fact]
    public void Test_EqualsOperator()
    {
        ScientificDecimal argument1 = new ScientificDecimal(1, 0);
        ScientificDecimal argument2 = new ScientificDecimal(-1, 0);
        ScientificDecimal posInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal negInfinity = ScientificDecimal.NegInfinity;
        
        // ReSharper disable once EqualExpressionComparison
        Assert.True(negInfinity == negInfinity);
        Assert.False(argument1 == argument2);
        Assert.False(argument1 == posInfinity);
    }

    [Fact]
    public void Test_GreaterThanOperator()
    {
        ScientificDecimal argument1 = new ScientificDecimal(1, 0);
        ScientificDecimal argument2 = new ScientificDecimal(-1, 0);
        ScientificDecimal posInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal negInfinity = ScientificDecimal.NegInfinity;

        // ReSharper disable once EqualExpressionComparison
        Assert.False(posInfinity > posInfinity);
        Assert.True(posInfinity > negInfinity);
        Assert.True(argument1 > argument2);
    }

    [Fact]
    public void Test_LessThanOperator()
    {
        ScientificDecimal argument1 = new ScientificDecimal(1, 0);
        ScientificDecimal argument2 = new ScientificDecimal(-1, 0);
        ScientificDecimal posInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal negInfinity = ScientificDecimal.NegInfinity;
        
        // ReSharper disable once EqualExpressionComparison
        Assert.False(posInfinity < posInfinity);
        Assert.True(argument2 < argument1);
        Assert.False(posInfinity < argument1);
    }
    
    #endregion Operators
    
    #region Casts

    [Fact]
    public void Test_DoubleCast()
    {
        ScientificDecimal argument = new ScientificDecimal(-1.59m, 1);
        
        Assert.Equal(-15.9d, (double)argument);
    }
    
    [Fact]
    public void Test_IntCast()
    {
        ScientificDecimal posArgument = new ScientificDecimal(1.59m, 1);
        ScientificDecimal negArgument = new ScientificDecimal(-1.59m, 1);
        
        Assert.Equal(15, (int)posArgument);
        Assert.Equal(-15, (int)negArgument);
    }
    
    #endregion Casts
}