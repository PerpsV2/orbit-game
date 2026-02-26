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
        InterpolationHandler<ScientificDecimal>.CreateInterpolation(
            new(() => _independent, val => { _independent = val; }), 
            new(() => _dependent, val => { _dependent = val; }), 
            0, 5, 0, 5);
        _independent = 2.5;
        InterpolationHandler<ScientificDecimal>.UpdateInterpolationValues();
        _output.WriteLine(_dependent.ToString());
    }
}