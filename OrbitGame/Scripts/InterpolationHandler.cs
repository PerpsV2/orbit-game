using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace OrbitGame;

public static class InterpolationHandler<T> where T : INumber<T>, IComparable<T>
{
    public delegate void Interpolator(InterpolatorRef variableReferences,
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
        Interpolator interpolator,
        T startIndependent,
        T endIndependent,
        T startDependent,
        T endDependent)
    {
        public delegate void InterpolationEvent(object? sender, EventArgs e);

        private bool _lastActiveState;

        public event InterpolationEvent? InterpolationStart;
        public event InterpolationEvent? InterpolationEnd;
        
        public void UpdateValue()
        {
            bool currentActiveState = IsActive();
            if (currentActiveState != _lastActiveState)
            {
                if (currentActiveState) OnInterpolationStart();
                else OnInterpolationEnd();
            }
            interpolator(variableReferences, startIndependent, endIndependent, startDependent, endDependent);
            _lastActiveState = currentActiveState;
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
    
    public static void CubicInterpolate(
        InterpolatorRef variableReferences,
        T startIndependent, T endIndependent, T startDependent, T endDependent
    )
    {
        if (variableReferences.Independent < startIndependent) return;
        if (variableReferences.Independent > endIndependent) return;
        T progress = (variableReferences.Independent - startIndependent) / (endIndependent - startIndependent);
        double progressDouble = double.CreateSaturating(progress);
        progressDouble = 3 * Math.Pow(progressDouble, 2) - 2 * Math.Pow(progressDouble, 3);
        variableReferences.Dependent = startDependent + (endDependent - startDependent) * T.CreateSaturating(progressDouble);
    }
    
    private static readonly List<Interpolation> Interpolations = [];
    
    public static Interpolation CreateInterpolation(
        InterpolatorRef variableReferences,
        T startIndependent, T endIndependent, T startDependent, T endDependent,
        Interpolator? interpolator = null
    )
    {
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