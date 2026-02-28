using Xunit.Abstractions;

namespace OrbitGame.Tests;

public class InterpolationHandler_Tests
{
    private readonly ITestOutputHelper _output;
    
    private ScientificDecimal _independent = new();
    private ScientificDecimal _dependent = new();

    public InterpolationHandler_Tests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void Test_InterpolationHandler()
    {
        _independent = 2.5;
        InterpolationHandler<ScientificDecimal>.UpdateInterpolationValues();
        _output.WriteLine(_dependent.ToString());
    }
}