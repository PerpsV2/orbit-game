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
        ScientificDecimal epsilon = new ScientificDecimal(1, -10);
        double estimate = eccentricity > 0.8 ? Math.PI : meanAnomaly;
        double finalEccentricAnomaly;
        int iterations = 0;
        do {
            finalEccentricAnomaly = estimate - (estimate - eccentricity * Math.Sin(estimate) - meanAnomaly) /
                (1 - eccentricity * Math.Cos(estimate));
            iterations++;
        } while (double.Abs(finalEccentricAnomaly - estimate) > epsilon && iterations <= 100);
        return finalEccentricAnomaly;
    }

    private static double CalculateMeanFromEccentricAnomaly(double eccentricity, double eccentricAnomaly)
    {
        return eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly);
    }

    private static double CalculateTrueFromEccentricAnomaly(double eccentricity, double eccentricAnomaly)
    {
        double a = eccentricity / (1 + Math.Sqrt(1 - eccentricity * eccentricity));
        return eccentricAnomaly + 2 * Math.Atan(a * Math.Sin(eccentricAnomaly) / (1 - a * Math.Cos(eccentricAnomaly)));
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
        
        double meanAnomaly = (double)(Math.Tau / Period * timeFromPeriapsis);
        double eccentricAnomaly = CalculateEccentricFromMeanAnomaly(Eccentricity, meanAnomaly);
        return CalculateTrueFromEccentricAnomaly(Eccentricity, eccentricAnomaly);
    }

    public SpatialInfo GetStateAtTime(ScientificDecimal time)
    {
        if (InitialTime == null) return new();
        time = (time - InitialTime.Value) % Period; 
        
        SpatialInfo newState = new SpatialInfo();
        double trueAnomaly = CalculateTrueAnomalyFromTimeSincePeriapsis(time);
        SD_Vector2 orbitalPosition = SD_Vector2.FromPolar(trueAnomaly, Equation(trueAnomaly));
        newState.Position = Parent.Position + orbitalPosition;
        return newState;
    }
}