using System;

namespace OrbitGame;

public delegate ScientificDecimal OrbitEquation(double angle);

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
        this.Periapsis = Utils.UnsignedMod(Periapsis, Math.Tau);
        
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
                SD_Vector2 initialVelocity = Body.Velocity - Parent.Velocity;
                double initialTrueAnomaly = Math.Acos((double)(
                    SD_Vector2.Dot(SD_Vector2.FromPolar(Periapsis, Eccentricity), initialPosition) /
                    (Eccentricity * initialPosition.Magnitude()))
                    );
                
                if (SD_Vector2.Dot(initialPosition, initialVelocity) < 0)
                    initialTrueAnomaly = Math.Tau - initialTrueAnomaly;

                if (double.IsNaN(initialTrueAnomaly)) return;

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
}