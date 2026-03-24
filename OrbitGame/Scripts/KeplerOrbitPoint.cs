using System;

namespace OrbitGame;

/// <summary>
/// Represents a point on a Keplerian Orbit
/// </summary>
public readonly record struct KeplerOrbitPoint(KeplerOrbitPath Path, double TrueAnomaly)
{
    public SDecimal GetTimeAtPoint(SDecimal currentTime)
    {
        if (Path.Orbit == null) throw new NullReferenceException("KeplerOrbitPathPoint has no orbit");
        KeplerOrbit orbit = Path.Orbit.Value;
        SDecimal timeSincePeriapsis = orbit.InitialTimeSincePeriapsis;
        SDecimal selectTimeFromPeriapsis = orbit.CalculateTimeSincePeriapsisFromTrueAnomaly(TrueAnomaly);
        SDecimal timeUntilPoint = selectTimeFromPeriapsis - timeSincePeriapsis - currentTime;
        if (!orbit.Prograde) timeUntilPoint = -timeUntilPoint;
        if (orbit.Eccentricity >= 1) return currentTime + timeUntilPoint;
        while (timeUntilPoint < 0) timeUntilPoint += orbit.Period;
        while (timeUntilPoint > orbit.Period) timeUntilPoint -= orbit.Period;
        return currentTime + timeUntilPoint;
    }
}