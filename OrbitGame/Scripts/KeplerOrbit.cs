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
        Equation = angle => SemiLatusRectum / (1 + Eccentricity * Math.Cos(angle - Periapsis));
        this.Periapsis = Utils.WrapAngle(Periapsis);
        
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
                SD_Vector2 initialPosition = Body.Position - Parent.Position;
                double initialTrueAnomaly = SD_Vector2.Direction(Parent.Position, initialPosition) - Periapsis;
                if (SD_Vector2.Dot(Body.Velocity, Body.Position) > 0) initialTrueAnomaly = Math.Tau - initialTrueAnomaly;

                double initialEccentricAnomaly = Math.Atan2(
                    Math.Sqrt(1 - Eccentricity * Eccentricity) * Math.Sin(initialTrueAnomaly),
                    Eccentricity + Math.Cos(initialTrueAnomaly)
                ) % Math.Tau;

                double initialMeanAnomaly = (initialEccentricAnomaly - Eccentricity * (
                    Math.Sqrt(1 - Eccentricity * Eccentricity) * Math.Sin(initialTrueAnomaly) /
                    1 + Eccentricity * Math.Cos(initialTrueAnomaly)
                )) % Math.Tau;

                InitialTime = initialMeanAnomaly / (Math.Tau / Period);
            }
        }
    }
    
    private double CalculateEccentricAnomaly(double meanAnomaly)
    {
        ScientificDecimal epsilon = new ScientificDecimal(1, -35);
        double eccentricAnomaly = meanAnomaly;
        int iterations = 0;
        while (double.Abs(eccentricAnomaly - Eccentricity * Math.Sin(eccentricAnomaly) - meanAnomaly) > epsilon)
        {
            if (iterations > 100) return eccentricAnomaly;
            eccentricAnomaly -= (eccentricAnomaly - Eccentricity * Math.Sin(eccentricAnomaly) - meanAnomaly) /
                                (1 - Eccentricity * Math.Cos(eccentricAnomaly));
            iterations++;
        }
        return eccentricAnomaly;
    }

    private double CalculateTrueAnomaly(double eccentricAnomaly)
    {
        return 2 * Math.Atan2(Math.Sqrt(1 + Eccentricity) * Math.Sin(eccentricAnomaly / 2), 
            Math.Sqrt(1 - Eccentricity) * Math.Cos(eccentricAnomaly / 2));
    }

    public SpatialInfo GetStateAtTime(ScientificDecimal time)
    {
        if (InitialTime == null) return new();
        time %= Period; 
        
        SpatialInfo newState = new SpatialInfo();
        double meanAnomaly = (double)(Math.Tau / Period * (time + InitialTime)) + Periapsis;
        double eccentricAnomaly = CalculateEccentricAnomaly(meanAnomaly - Periapsis) + Periapsis;
        double trueAnomaly = CalculateTrueAnomaly(eccentricAnomaly - Periapsis) + Periapsis;
        newState.Position = Parent.Position + SD_Vector2.FromPolar(trueAnomaly, Equation(trueAnomaly));
        return newState;
    }
}