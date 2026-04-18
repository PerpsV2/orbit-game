using System;
using System.Numerics;

namespace OrbitGame;

public class LinearEquationSystem<T>(int numVariables, int numEquations)
    where T : INumber<T>
{
    private int _numEquations = numEquations;
    private readonly int _numVariables = numVariables;
    private T[,] _coefficients = new T[numEquations, numVariables];
    private T[] _constants = new T[numEquations];

    public void SetCoefficient(int index, T value)
    {
        if (index >= _numVariables * _numEquations || index < 0)
            throw new IndexOutOfRangeException("Coefficient index out of the range of system of linear equations");

        var indices = int.DivRem(index, _numVariables);
        _coefficients[indices.Quotient, indices.Remainder] = value;
    }

    public void SetConstant(int index, T value)
    {
        if (index >= _numEquations || index < 0) 
            throw new IndexOutOfRangeException("Constant index out of the range of system of linear equations");

        _constants[index] = value;
    }

    private void RowInterchange(int leftRowIndex, int rightRowIndex)
    {
        for (int i = 0; i < _numVariables; ++i)
        {
            (_coefficients[leftRowIndex, i], _coefficients[rightRowIndex, i]) = 
                (_coefficients[rightRowIndex, i], _coefficients[leftRowIndex, i]);
        }
        
        (_constants[leftRowIndex], _constants[rightRowIndex]) =
            (_constants[rightRowIndex], _constants[leftRowIndex]);
    }

    private void RowScale(int rowIndex, T scale)
    {
        for (int i = 0; i < _numVariables; ++i)
            _coefficients[rowIndex, i] *= scale;
        _constants[rowIndex] *= scale;
    }
    
    private void RowAdd(int modifyingRowIndex, int scaleRowIndex, T scale)
    {
        for (int i = 0; i < _numVariables; ++i)
            _coefficients[modifyingRowIndex, i] += scale * _coefficients[scaleRowIndex, i];
        _constants[modifyingRowIndex] += scale * _constants[scaleRowIndex];
    }

    private void ConvertToReducedEchelonForm()
    {
        int highestAvailableRow = 0;
        for (int currentCol = 0; currentCol < _numVariables; ++currentCol)
        {
            bool nonZeroContainingColumn = false;
            for (int row = highestAvailableRow; row < _numEquations; ++row)
            {
                T coefficient = _coefficients[row, currentCol];
                if (coefficient != T.Zero)
                {
                    RowScale(row, T.One / coefficient);
                    RowInterchange(row, highestAvailableRow);
                    highestAvailableRow++;
                    nonZeroContainingColumn = true;
                    break;
                }
            }

            if (!nonZeroContainingColumn) continue;

            for (int row = highestAvailableRow; row < _numEquations; ++row)
            {
                T coefficient = _coefficients[row, currentCol];
                if (currentCol == _numVariables - 1) RowAdd(row, highestAvailableRow - 1, -coefficient + T.One);
                else RowAdd(row, highestAvailableRow - 1, -coefficient);
            }
        }
    }

    private T[,] GetTranspose()
    {
        T[,] transpose = new T[_coefficients.GetLength(1), _coefficients.GetLength(0)];
        for (int i = 0; i < _coefficients.GetLength(0); ++i)
            for (int j = 0; j < _coefficients.GetLength(1); ++j)
                transpose[j, i] = _coefficients[i, j];

        return transpose;
    }

    private void Normalize()
    {
        T[,] transpose = GetTranspose();
        
        T[,] resultCoefficients = new T[_numVariables, _numVariables];
        for (int i = 0; i < _numVariables; ++i)
            for (int j = 0; j < _numVariables; ++j)
                for (int k = 0; k < _numEquations; ++k)
                    resultCoefficients[i, j] += transpose[i, k] * _coefficients[k, j];

        T[] resultConstants = new T[_numVariables];
        for (int i = 0; i < _numVariables; ++i)
            for (int j = 0; j < _numEquations; ++j)
                resultConstants[i] += transpose[i, j] * _constants[j];
        
        _coefficients = resultCoefficients;
        _constants = resultConstants;
        
        _numEquations = _numVariables;
    }
    
    public T[]? Solve(bool normalize = false)
    {
        if (normalize) Normalize();
        ConvertToReducedEchelonForm();
        T[] result = new T[_numVariables];

        if (_numEquations < _numVariables) return null;

        for (int i = _numVariables - 1; i >= 0; --i)
        {
            result[i] = _constants[i];
            for (int j = _numVariables - 1; j > i; --j)
                result[i] -= result[j] * _coefficients[i, j];
        }

        return result;
    }
    
    public override string ToString()
    {
        string result = "";
        for (int r = 0; r < _coefficients.GetLength(0); ++r)
        {
            for (int c = 0; c < _coefficients.GetLength(1); ++c)
            {
                result += _coefficients[r, c] + " ";
            }
            result += _constants[r] + " ";
            result += "\n";
        }

        return result;
    }
}