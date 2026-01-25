namespace OrbitGame.Tests;
using OrbitGame;

public class ScientificDecimalTests
{
    [Fact]
    public void Test_SqrtMethod()
    {
        ScientificDecimal num1 = new ScientificDecimal(4, 0);
        ScientificDecimal num2 = new ScientificDecimal(-4, 0);
        ScientificDecimal numPosInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal numNegInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(2, num1.Sqrt());
        Assert.Equal(numPosInfinity, numPosInfinity.Sqrt());
        Assert.Throws<ArithmeticException>(() => num2.Sqrt());
        Assert.Throws<ArithmeticException>(() => numNegInfinity.Sqrt());
    }

    [Fact]
    public void Test_AbsMethod()
    {
        ScientificDecimal num1 = new ScientificDecimal(4, 0);
        ScientificDecimal num2 = new ScientificDecimal(-4, 0);
        ScientificDecimal numPosInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal numNegInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(4, num1.Abs());
        Assert.Equal(4, num2.Abs());
        Assert.Equal(numPosInfinity, numPosInfinity.Abs());
        Assert.Equal(numPosInfinity, numNegInfinity.Abs());
    }

    [Fact]
    public void Test_MinMethod()
    {
        ScientificDecimal num1 = new ScientificDecimal(2, 0);
        ScientificDecimal num2 = new ScientificDecimal(-4, 0);
        ScientificDecimal numPosInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal numNegInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(numNegInfinity, ScientificDecimal.Min(numNegInfinity, numPosInfinity, num1, num2));
        Assert.Equal(2, ScientificDecimal.Min(numPosInfinity, num1));
        Assert.Equal(-4, ScientificDecimal.Min(num1, num2));
        Assert.Equal(numNegInfinity, ScientificDecimal.Min(numPosInfinity, numNegInfinity));
    }
    
    [Fact]
    public void Test_MaxMethod()
    {
        ScientificDecimal num1 = new ScientificDecimal(-8, 0);
        ScientificDecimal num2 = new ScientificDecimal(4, 0);
        ScientificDecimal numPosInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal numNegInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(numPosInfinity, ScientificDecimal.Max(numNegInfinity, numPosInfinity, num1, num2));
        Assert.Equal(-8, ScientificDecimal.Max(numNegInfinity, num1));
        Assert.Equal(4, ScientificDecimal.Max(num1, num2));
        Assert.Equal(numPosInfinity, ScientificDecimal.Max(numPosInfinity, numNegInfinity));
    }

    [Fact]
    public void Test_ClampMethod()
    {
        ScientificDecimal num1 = new ScientificDecimal(2, 0);
        ScientificDecimal num2 = new ScientificDecimal(-4, 0);
        ScientificDecimal num3 = new ScientificDecimal(-8, 0);
        ScientificDecimal numPosInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal numNegInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(-4, num3.Clamp(num2, num1));
        Assert.Equal(-4, numNegInfinity.Clamp(num2, num1));
        Assert.Equal(2, numPosInfinity.Clamp(num2, num1));
        Assert.Equal(numPosInfinity, numPosInfinity.Clamp(num2, numPosInfinity));
        Assert.Throws<ArithmeticException>(() => num3.Clamp(num1, num2));
    }
    
    #region Operators
    
    [Fact]
    public void Test_NegativeOperator()
    {
        ScientificDecimal num1 = new ScientificDecimal(2, 0);
        ScientificDecimal num2 = new ScientificDecimal(-4, 0);
        ScientificDecimal numPosInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal numNegInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(-2, -num1);
        Assert.Equal(4, -num2);
        Assert.Equal(numNegInfinity, -numPosInfinity);
        Assert.Equal(numPosInfinity, -numNegInfinity);
    }
    
    [Fact]
    public void Test_AdditionOperator()
    {
        ScientificDecimal num1 = new ScientificDecimal(2, 0);
        ScientificDecimal num2 = new ScientificDecimal(-4, 0);
        ScientificDecimal numPosInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal numNegInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(-2, num1 + num2);
        Assert.Equal(numPosInfinity, num1 + numPosInfinity);
        Assert.Equal(numPosInfinity, num2 + numPosInfinity);
        Assert.Equal(numNegInfinity, num1 + numNegInfinity);
        Assert.Equal(numPosInfinity, numPosInfinity + numPosInfinity);
        Assert.Equal(numNegInfinity, numNegInfinity + numNegInfinity);
        Assert.Throws<ArithmeticException>(() => numPosInfinity + numNegInfinity);
    }

    [Fact]
    public void Test_SubtractionOperator()
    {
        ScientificDecimal num1 = new ScientificDecimal(2, 0);
        ScientificDecimal num2 = new ScientificDecimal(-4, 0);
        ScientificDecimal numPosInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal numNegInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(6, num1 - num2);
        Assert.Equal(-6, num2 - num1);
        Assert.Equal(numPosInfinity, num1 - numNegInfinity);
        Assert.Equal(numNegInfinity, numNegInfinity - num1);
        Assert.Throws<ArithmeticException>(() => numPosInfinity - numPosInfinity);
        Assert.Throws<ArithmeticException>(() => numNegInfinity - numNegInfinity);
    }

    [Fact]
    public void Test_MultiplicationOperator()
    {
        ScientificDecimal num1 = new ScientificDecimal(0, 0);
        ScientificDecimal num2 = new ScientificDecimal(2, 0);
        ScientificDecimal num3 = new ScientificDecimal(-4, 0);
        ScientificDecimal numPosInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal numNegInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(-8, num2 * num3);
        Assert.Equal(0, num1 * num3);
        Assert.Equal(0, num1 * numPosInfinity);
        Assert.Equal(numPosInfinity, num2 * numPosInfinity);
        Assert.Equal(numNegInfinity, num3 * numPosInfinity);
        Assert.Equal(numPosInfinity, numPosInfinity * numPosInfinity);
        Assert.Equal(numNegInfinity, numPosInfinity * numNegInfinity);
        Assert.Equal(numPosInfinity, numNegInfinity * numNegInfinity);
    }

    [Fact]
    public void Test_DivisionOperator()
    {
        ScientificDecimal num1 = new ScientificDecimal(0, 0);
        ScientificDecimal num2 = new ScientificDecimal(2, 0);
        ScientificDecimal num3 = new ScientificDecimal(-4, 0);
        ScientificDecimal numPosInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal numNegInfinity = ScientificDecimal.NegInfinity;
        
        Assert.Equal(0, num1 / num3);
        Assert.Equal(-2, num3 / num2);
        Assert.Equal(-0.5, num2 / num3);
        Assert.Equal(0, num2 / numPosInfinity);
        Assert.Equal(numNegInfinity, numPosInfinity / num3);
        Assert.Throws<ArithmeticException>(() => num2 / num1);
        Assert.Throws<ArithmeticException>(() => numPosInfinity / numNegInfinity);
    }
    
    [Fact]
    public void Test_EqualsOperator()
    {
        ScientificDecimal num2 = new ScientificDecimal(1, 0);
        ScientificDecimal num3 = new ScientificDecimal(-1, 0);
        ScientificDecimal numPosInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal numNegInfinity = ScientificDecimal.NegInfinity;
        
        // ReSharper disable twice EqualExpressionComparison
        Assert.True(numPosInfinity == numPosInfinity);
        Assert.True(numNegInfinity == numNegInfinity);
        Assert.False(num2 == num3);
        Assert.False(num2 == numPosInfinity);
        Assert.False(numPosInfinity == numNegInfinity);
    }

    [Fact]
    public void Test_GreaterThanOperator()
    {
        ScientificDecimal num2 = new ScientificDecimal(1, 0);
        ScientificDecimal num3 = new ScientificDecimal(-1, 0);
        ScientificDecimal numPosInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal numNegInfinity = ScientificDecimal.NegInfinity;
        
        Assert.True(numPosInfinity > num2);
        Assert.True(num3 > numNegInfinity);
        Assert.True(num2 > num3);
        Assert.False(num3 > num2);
        Assert.False(numPosInfinity > numPosInfinity);
    }

    [Fact]
    public void Test_LessThanOperator()
    {
        ScientificDecimal num2 = new ScientificDecimal(1, 0);
        ScientificDecimal num3 = new ScientificDecimal(-1, 0);
        ScientificDecimal numPosInfinity = ScientificDecimal.PosInfinity;
        ScientificDecimal numNegInfinity = ScientificDecimal.NegInfinity;
        
        Assert.True(num3 < num2);
        Assert.False(numPosInfinity < num2);
        Assert.False(num3 < numNegInfinity);
        Assert.False(num2 < num3);
        Assert.False(numPosInfinity < numPosInfinity);
    }
    
    #endregion Operators
    
    #region Casts

    [Fact]
    public void Test_DoubleCast()
    {
        ScientificDecimal num1 = new ScientificDecimal(-1.59m, 1);
        ScientificDecimal num2 = new ScientificDecimal(1.59m, 1);
        
        Assert.Equal(-15.9d, (double)num1);
        Assert.Equal(15.9d, (double)num2);
    }
    
    [Fact]
    public void Test_IntCast()
    {
        ScientificDecimal num1 = new ScientificDecimal(-1.59m, 1);
        ScientificDecimal num2 = new ScientificDecimal(1.59m, 1);
        
        Assert.Equal(-15, (int)num1);
        Assert.Equal(15, (int)num2);
    }
    
    #endregion Casts
}