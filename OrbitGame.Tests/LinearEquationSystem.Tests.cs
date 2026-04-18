using Xunit;

namespace OrbitGame.Tests;

public class LinearEquationSystem_Tests
{
    private LinearEquationSystem<double> _consistentSystem;
    private LinearEquationSystem<double> _underdeterminedSystem;
    private LinearEquationSystem<double> _overdeterminedSystem;
    
    public LinearEquationSystem_Tests()
    {
        _consistentSystem = new LinearEquationSystem<double>(3, 3);
        _consistentSystem.SetCoefficient(0, 0);
        _consistentSystem.SetCoefficient(1, 1);
        _consistentSystem.SetCoefficient(2, 1);
        _consistentSystem.SetConstant(0, 2);
        
        _consistentSystem.SetCoefficient(3, 1);
        _consistentSystem.SetCoefficient(4, 0);
        _consistentSystem.SetCoefficient(5, 1);
        _consistentSystem.SetConstant(1, 2);
        
        _consistentSystem.SetCoefficient(6, 1);
        _consistentSystem.SetCoefficient(7, 0);
        _consistentSystem.SetCoefficient(8, 0);
        _consistentSystem.SetConstant(2, 2);
        
        _overdeterminedSystem = new LinearEquationSystem<double>(3, 4);
        _overdeterminedSystem.SetCoefficient(0, 0);
        _overdeterminedSystem.SetCoefficient(1, 1);
        _overdeterminedSystem.SetCoefficient(2, 1);
        _overdeterminedSystem.SetConstant(0, 2);
        
        _overdeterminedSystem.SetCoefficient(3, 1);
        _overdeterminedSystem.SetCoefficient(4, 0);
        _overdeterminedSystem.SetCoefficient(5, 1);
        _overdeterminedSystem.SetConstant(1, 2);
        
        _overdeterminedSystem.SetCoefficient(6, 1);
        _overdeterminedSystem.SetCoefficient(7, 0);
        _overdeterminedSystem.SetCoefficient(8, 0);
        _overdeterminedSystem.SetConstant(2, 2);
        
        _overdeterminedSystem.SetCoefficient(9, 1);
        _overdeterminedSystem.SetCoefficient(10, 1);
        _overdeterminedSystem.SetCoefficient(11, 0);
        _overdeterminedSystem.SetConstant(3, 2);
        
        _underdeterminedSystem = new LinearEquationSystem<double>(3, 2);
        _overdeterminedSystem.SetCoefficient(0, 0);
        _overdeterminedSystem.SetCoefficient(1, 1);
        _overdeterminedSystem.SetCoefficient(2, 1);
        _overdeterminedSystem.SetConstant(0, 2);
        
        _overdeterminedSystem.SetCoefficient(3, 1);
        _overdeterminedSystem.SetCoefficient(4, 0);
        _overdeterminedSystem.SetCoefficient(5, 1);
        _overdeterminedSystem.SetConstant(1, 2);
    }

    [Fact]
    public void LinearEquationSystem_SolveMethod()
    {
        double[]? solution = _consistentSystem.Solve();
        Assert.NotNull(solution);
        if (solution is not null)
        {
            Assert.Equal(2, solution[0]);
            Assert.Equal(2, solution[1]);
            Assert.Equal(0, solution[2]);
        }

        solution = _overdeterminedSystem.Solve(true);
        Assert.NotNull(solution);
        if (solution is not null) {
            Assert.Equal(2, solution[0]);
            Assert.Equal(2, solution[1]);
            Assert.Equal(0, solution[2]);
        }
        
        solution = _underdeterminedSystem.Solve();
        Assert.Null(solution);
    }
}