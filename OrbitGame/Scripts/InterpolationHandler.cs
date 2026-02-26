using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace OrbitGame;

public delegate void Interpolator<T>(in T independentVariable, out T dependentVariable,
    T startIndependent, T endIndependent, T startDependent, T endDependent);

public static class InterpolationHandler<T> where T : INumber<T>, IComparable<T>
{
    public static void AdjustInterpolationValue(ref T independentVariable, ref T dependentVariable,
        T startIndependent, T endIndependent, T startDependent, T endDependent)
    {
        T progress;
        if (independentVariable <= startIndependent) progress = T.Zero;
        else if (independentVariable >= endIndependent) progress = T.One;
        else progress = (independentVariable - startIndependent) / (endIndependent - startIndependent);
        dependentVariable = startDependent + (endDependent - startDependent) * progress;
    }
}