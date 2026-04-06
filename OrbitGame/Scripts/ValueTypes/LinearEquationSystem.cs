using System;
using System.Numerics;

namespace OrbitGame;

public class LinearEquationSystem<T>(int numVariables)
    where T : INumber<T>
{
    private readonly int _numVariables = numVariables;
    private readonly T[,] _coefficients = new T[numVariables, numVariables];
    private readonly T[] _constants = new T[numVariables];

    public void SetCoefficient(int index, T value)
    {
        if (index >= _numVariables * _numVariables || index < 0)
            throw new IndexOutOfRangeException("Coefficient index out of the range of system of linear equations");

        var indices = int.DivRem(index, _numVariables);
        _coefficients[indices.Quotient, indices.Remainder] = value;
    }

    public void SetConstant(int index, T value)
    {
        if (index >= _numVariables || index < 0) 
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
            for (int row = highestAvailableRow; row < _numVariables; ++row)
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

            for (int row = highestAvailableRow; row < _numVariables; ++row)
            {
                T coefficient = _coefficients[row, currentCol];
                RowAdd(row, highestAvailableRow - 1, -coefficient);
            }
        }
    }
    
    public T[] Solve()
    {
        ConvertToReducedEchelonForm();
        T[] result = new T[_numVariables];

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