using System;
using System.Collections.Generic;
using System.Numerics;

namespace OrbitGame;

public static class InterpolationHandler<T> where T : INumber<T>, IComparable<T>
{
    public readonly struct Interpolation(T startIndependent, T endIndependent, T startDependent, T endDependent)
    {
        public bool IsEnded(ref readonly T independent) => independent > endIndependent;
    
        public T Query(ref readonly T independent)
        {
            T progress = independent >= endIndependent ? endIndependent :
                independent <= startIndependent ? startIndependent : 
                (independent - startIndependent) / (endIndependent - startIndependent);
            return startDependent + (endDependent - startDependent) * progress;
        }
    }
    
    private static readonly Dictionary<string, Interpolation> Interpolations = new();

    public static Interpolation CreateInterpolation(
        string key, ref readonly T independent, 
        T startIndependent, T endIndependent, T startDependent, T endDependent)
    {
        Interpolation interpolation = new(startIndependent, endIndependent, startDependent, endDependent);
        bool attempt = Interpolations.TryAdd(key, interpolation);
        if (attempt) return interpolation;
        if (!Interpolations[key].IsEnded(in independent)) return interpolation;
        Interpolations.Remove(key);
        Interpolations.Add(key, interpolation);
        return interpolation;
    }

    public static T Query(string key, ref readonly T independent)
    {
        Interpolations.TryGetValue(key, out var interpolation);
        return interpolation.Query(in independent);
    }
}