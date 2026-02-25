using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OrbitGame;

public enum InterpolationKey
{
    FastForwardTime
}

public static class InterpolationHandler<T> where T : INumber<T>, IComparable<T>
{
    public readonly struct Interpolation(T startIndependent, T endIndependent, T startDependent, T endDependent)
    {
        private readonly T _startIndependent = startIndependent;
        private readonly T _endIndependent = endIndependent;
        private readonly T _startDependent = startDependent;
        private readonly T _endDependent = endDependent;

        public bool Intersects(Interpolation other)
            => other._startIndependent < _endIndependent && _startIndependent < other._endIndependent;
        
        public bool Intersects(T independent)
            => independent > _startIndependent && independent < _endIndependent;
        
        public bool IsEnded(T independent) => independent > _endIndependent;
    
        public T Query(T independent)
        {
            T progress = independent >= _endIndependent ? _endIndependent :
                independent <= _startIndependent ? _startIndependent : 
                (independent - _startIndependent) / (_endIndependent - _startIndependent);
            return _startDependent + (_endDependent - _startDependent) * progress;
        }
    }
    
    private static readonly Dictionary<InterpolationKey, List<Interpolation>> Interpolations = new();

    public static Interpolation? CreateInterpolation(
        InterpolationKey key, T independent, 
        T startIndependent, T endIndependent, T startDependent, T endDependent)
    {
        Interpolation interpolation = new(startIndependent, endIndependent, startDependent, endDependent);
        if (Interpolations.TryGetValue(key, out var keyInterpolations))
        {
            keyInterpolations.RemoveAll(x => x.IsEnded(independent));
            if (keyInterpolations.Any(x => x.Intersects(interpolation))) return null;
            keyInterpolations.Add(interpolation);
            return interpolation;
        }
        Interpolations.Add(key, [interpolation]);
        return interpolation;
    }

    public static T? Query(InterpolationKey key, T independent, T? defaultValue = default)
    {
        Interpolations.TryGetValue(key, out var keyInterpolations);
        if (keyInterpolations == null) return defaultValue;
        keyInterpolations.RemoveAll(x => x.IsEnded(independent));
        foreach (var interpolation in keyInterpolations)
        {
            if (interpolation.Intersects(independent)) return interpolation.Query(independent);
        }
        return defaultValue;
    }
}