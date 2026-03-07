using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace OrbitGame;

public static class InterpolationHandler<T> where T : INumber<T>, IComparable<T>
{
    public delegate void InterpolatorMethod(InterpolatorRef variableReferences,
        T startIndependent, T endIndependent, T startDependent, T endDependent);
    
    public class InterpolatorRef(Func<T> independentGetter, Action<T> dependentSetter)
    {
        public T Independent => independentGetter();

        public T Dependent {
            set => dependentSetter(value);
        }
    }

    public class Interpolation(
        InterpolatorRef variableReferences,
        InterpolatorMethod interpolator,
        T startIndependent,
        T endIndependent,
        T startDependent,
        T endDependent)
    {
        public delegate void InterpolationEvent(object? sender, EventArgs e);

        public event InterpolationEvent? InterpolationStart;
        public event InterpolationEvent? InterpolationEnd;

        public InterpolatorMethod Interpolator = interpolator;
        
        private T _lastIndependentValue = variableReferences.Independent;
        
        public void UpdateValue()
        {
            T currentIndependentValue = variableReferences.Independent;
            if (_lastIndependentValue < startIndependent && currentIndependentValue >= startIndependent) 
                OnInterpolationStart();
            else if (_lastIndependentValue < endIndependent && currentIndependentValue >= endIndependent)
                OnInterpolationEnd();
            Interpolator(variableReferences, startIndependent, endIndependent, startDependent, endDependent);
            _lastIndependentValue = currentIndependentValue;
        }

        private void OnInterpolationStart()
        {
            InterpolationStart?.Invoke(this, EventArgs.Empty);
            variableReferences.Dependent = startDependent;
        }

        private void OnInterpolationEnd()
        {
            InterpolationEnd?.Invoke(this, EventArgs.Empty);
            variableReferences.Dependent = endDependent;
        }

        public bool IsActive()
            => variableReferences.Independent >= startIndependent && variableReferences.Independent <= endIndependent;

        public bool IsEnded()
            => variableReferences.Independent > endIndependent;

        ~Interpolation()
        {
            if (IsActive()) InterpolationEnd?.Invoke(this, EventArgs.Empty);
        }
    }

    public static void LinearInterpolate(
        InterpolatorRef variableReferences,
        T startIndependent, T endIndependent, T startDependent, T endDependent
    )
    {
        if (variableReferences.Independent < startIndependent) return;
        if (variableReferences.Independent > endIndependent) return;
        T progress = (variableReferences.Independent - startIndependent) / (endIndependent - startIndependent);
        variableReferences.Dependent = startDependent + (endDependent - startDependent) * progress;
    }
    
    private static readonly List<Interpolation> Interpolations = [];
    
    public static Interpolation CreateInterpolation(
        InterpolatorRef variableReferences,
        T startIndependent, T endIndependent, T startDependent, T endDependent,
        InterpolatorMethod? interpolator = null
    )
    {
        if (endIndependent < startIndependent) throw new ArgumentOutOfRangeException(nameof(endIndependent));
        Interpolations.Add(new Interpolation(variableReferences, interpolator ?? LinearInterpolate,
            startIndependent, endIndependent, startDependent, endDependent));
        return Interpolations.Last();
    }

    public static void UpdateInterpolationValues()
    {
        foreach (var interpolation in Interpolations)
            interpolation.UpdateValue();
        Interpolations.RemoveAll(x => x.IsEnded());
    }
}