using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Equation which returns the distance of an object to its parent given a true anomaly.
/// </summary>
public delegate ScientificDecimal OrbitEquation(double angle);

/// <summary>
/// Record struct containing information about a Keplerian orbit as well as methods for converting between certain
/// orbital parameters.
/// </summary>
public readonly record struct KeplerOrbit
{
    public readonly Body Body;
    public readonly Body Parent;
    public readonly double Periapsis;
    public readonly double Eccentricity;
    public readonly ScientificDecimal SemiLatusRectum;
    
    public readonly OrbitEquation Equation;
    public readonly ScientificDecimal SemiMajorAxis;
    public readonly ScientificDecimal SemiMinorAxis;
    public readonly ScientificDecimal Period;
    public readonly SD_Vector2? Center;
    
    public readonly ScientificDecimal? SphereOfInfluenceRadius;

    public readonly ScientificDecimal? InitialTime;
    
    public KeplerOrbit(
        Body Body,
        Body Parent,
        double Eccentricity, 
        double Periapsis,
        ScientificDecimal SemiLatusRectum,
        bool initials = false
        )
    {
        this.Body = Body;
        this.Parent = Parent;
        this.Eccentricity = Eccentricity;
        this.SemiLatusRectum = SemiLatusRectum;
        this.Periapsis = Utils.WrapAngle(Periapsis);
        Equation = angle => SemiLatusRectum / (1 + Eccentricity * Math.Cos(angle - Periapsis));
        
        if (Eccentricity == 0)
        {
            SemiMajorAxis = SemiLatusRectum;
            SemiMinorAxis = SemiLatusRectum;
            SphereOfInfluenceRadius = SemiMajorAxis * Math.Pow((double)(Body.Mass / Parent.Mass), 2f / 5f);
            Period = Math.Tau * (SemiMajorAxis * SemiMajorAxis * SemiMajorAxis / Constants.G / Parent.Mass).Sqrt();
            Center = SD_Vector2.FromPolar(Periapsis, Equation(Periapsis)) + 
                     SD_Vector2.FromPolar(Periapsis, -SemiMajorAxis);
        }
        if (Eccentricity is > 0 and < 1)
        {
            SemiMajorAxis = (Equation(Periapsis) + Equation(Periapsis + Math.PI)) / 2;
            SemiMinorAxis = (Equation(Periapsis) * Equation(Periapsis + Math.PI)).Sqrt();
            SphereOfInfluenceRadius = SemiMajorAxis * Math.Pow((double)(Body.Mass / Parent.Mass), 2f / 5f);
            Period = Math.Tau * (SemiMajorAxis * SemiMajorAxis * SemiMajorAxis / Constants.G / Parent.Mass).Sqrt();
            Center = SD_Vector2.FromPolar(Periapsis, Equation(Periapsis)) + 
                     SD_Vector2.FromPolar(Periapsis, -SemiMajorAxis);
        }
        if (Eccentricity >= 1)
        {
            SemiMajorAxis = (Equation(Periapsis) + Equation(Periapsis + Math.PI)) / 2;
            SemiMinorAxis = ScientificDecimal.PosInfinity;
            SphereOfInfluenceRadius = null;
            Period = ScientificDecimal.PosInfinity;
            Center = null;
        }
        
        if (Eccentricity < 1)
        {
            if (initials)
            {
                SD_Vector2 initialRelativePosition = Body.Position - Parent.Position;
                double initialTrueAnomaly = SD_Vector2.Direction(Parent.Position, initialRelativePosition) - Periapsis;
                initialTrueAnomaly = Utils.WrapAngle(initialTrueAnomaly);
                InitialTime = CalculateTimeSincePeriapsisFromTrueAnomaly(initialTrueAnomaly);
            }
        }
    }
    
    private static double CalculateEccentricFromMeanAnomaly(double eccentricity, double meanAnomaly)
    {
        ScientificDecimal epsilon = new ScientificDecimal(1, -35);
        double eccentricAnomaly = meanAnomaly;
        int iterations = 0;
        while (double.Abs(eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly) - meanAnomaly) > epsilon)
        {
            if (iterations > 100) return eccentricAnomaly;
            eccentricAnomaly -= (eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly) - meanAnomaly) /
                                (1 - eccentricity * Math.Cos(eccentricAnomaly));
            iterations++;
        }
        return eccentricAnomaly;
    }

    private static double CalculateMeanFromEccentricAnomaly(double eccentricity, double eccentricAnomaly)
    {
        return eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly);
    }

    private static double CalculateTrueFromEccentricAnomaly(double eccentricity, double eccentricAnomaly)
    {
        return 2 * Math.Atan2(Math.Sqrt(1 + eccentricity) * Math.Sin(eccentricAnomaly / 2), 
            Math.Sqrt(1 - eccentricity) * Math.Cos(eccentricAnomaly / 2));
    }

    private static double CalculateEccentricFromTrueAnomaly(double eccentricity, double trueAnomaly)
    {
        double eccentricAnomaly = Utils.WrapAngle(Math.Atan2(
            Math.Sqrt(1 - eccentricity * eccentricity) * Math.Sin(trueAnomaly),
            eccentricity + Math.Cos(trueAnomaly)
        ));
        return eccentricAnomaly;
    }

    public ScientificDecimal CalculateTimeSincePeriapsisFromTrueAnomaly(double trueAnomaly)
    {
        double eccentricAnomaly = CalculateEccentricFromTrueAnomaly(Eccentricity, trueAnomaly);
        double meanAnomaly = CalculateMeanFromEccentricAnomaly(Eccentricity, eccentricAnomaly);
        return meanAnomaly * Period / Math.Tau;
    }

    public double CalculateTrueAnomalyFromTimeSincePeriapsis(ScientificDecimal timeFromPeriapsis)
    {
        if (InitialTime == null) return 0;
        
        double meanAnomaly = (double)(Math.Tau / Period * (timeFromPeriapsis + InitialTime)) + Periapsis;
        double eccentricAnomaly = CalculateEccentricFromMeanAnomaly(Eccentricity, meanAnomaly - Periapsis) + Periapsis;
        double trueAnomaly = CalculateTrueFromEccentricAnomaly(Eccentricity, eccentricAnomaly - Periapsis) + Periapsis;
        return trueAnomaly;
    }

    public SpatialInfo GetStateAtTime(ScientificDecimal time)
    {
        if (InitialTime == null) return new();
        time %= Period; 
        
        SpatialInfo newState = new SpatialInfo();
        double trueAnomaly = CalculateTrueAnomalyFromTimeSincePeriapsis(time);
        newState.Position = Parent.Position + SD_Vector2.FromPolar(trueAnomaly, Equation(trueAnomaly));
        return newState;
    }
}