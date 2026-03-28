using System;

namespace OrbitGame;

/// <summary>
/// Represents a point on a Keplerian Orbit
/// </summary>
public readonly record struct KeplerOrbitPoint(ConicPath ConicPath, double TrueAnomaly)
{
    public SDecimal GetNextTime(SDecimal currentTime)
    {
        if (ConicPath.Orbit == null) throw new NullReferenceException("KeplerOrbitPoint has no orbit");
        KeplerOrbit orbit = ConicPath.Orbit.Value;
        SDecimal pointTimeSincePeriapsis = orbit.GetTimeSincePeriapsisFromTrueAnomaly(TrueAnomaly);
        SDecimal lastPeriapsisTime = orbit.Period * (currentTime / orbit.Period).Floor();
        SDecimal currentTimeSincePeriapsis = currentTime + orbit.InitialTimeSincePeriapsis - lastPeriapsisTime;
        if (orbit.Eccentricity < 1)
        {
            // make sure the resultant time is within one period of the current time
            if (pointTimeSincePeriapsis < currentTimeSincePeriapsis) pointTimeSincePeriapsis += orbit.Period;
            if (pointTimeSincePeriapsis - currentTimeSincePeriapsis > orbit.Period) pointTimeSincePeriapsis -= orbit.Period;
        }
        return currentTime + pointTimeSincePeriapsis - currentTimeSincePeriapsis;
    }
}