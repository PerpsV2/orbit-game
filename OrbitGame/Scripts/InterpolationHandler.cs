using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace OrbitGame;

public static class InterpolationHandler<T> where T : INumber<T>, IComparable<T>
{
    public delegate void Interpolator(VariableRef independentVariable, VariableRef dependentVariable,
        T startIndependent, T endIndependent, T startDependent, T endDependent);
    
    public class VariableRef(Func<T> getter, Action<T> setter)
    {
        public T Value
        {
            get => getter();
            set => setter(value);
        }
    }

    private class Interpolation(
        VariableRef independentVariable,
        VariableRef dependentVariable,
        Interpolator interpolator,
        T startIndependent,
        T endIndependent,
        T startDependent,
        T endDependent)
    {
        public void UpdateValue()
        {
            interpolator(independentVariable, dependentVariable, 
                startIndependent, endIndependent, startDependent, endDependent);
        }

        public bool IsEnded()
            => independentVariable.Value > endIndependent;
    }

    public static void LinearInterpolate(
        VariableRef independentVariable, VariableRef dependentVariable,
        T startIndependent, T endIndependent, T startDependent, T endDependent
    )
    {
        if (independentVariable.Value < startIndependent) return;
        if (independentVariable.Value > endIndependent) return;
        T progress = (independentVariable.Value - startIndependent) / (endIndependent - startIndependent);
        dependentVariable.Value = startDependent + (endDependent - startDependent) * progress;
    }
    
    private static readonly List<Interpolation> Interpolations = [];
    
    public static void CreateInterpolation(
        VariableRef independentVariable, VariableRef dependentVariable,
        T startIndependent, T endIndependent, T startDependent, T endDependent,
        Interpolator? interpolator = null
    )
    {
        Interpolations.Add(new Interpolation(independentVariable, dependentVariable, 
            interpolator ?? LinearInterpolate,
            startIndependent, endIndependent, startDependent, endDependent));
    }

    public static void UpdateInterpolationValues()
    {
        Interpolations.RemoveAll(x => x.IsEnded());
        foreach (var interpolation in Interpolations)
        {
            interpolation.UpdateValue();
        }
    }
}