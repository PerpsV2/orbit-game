using System;

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

    /// <summary>
    /// Initial time since periapsis
    /// </summary>
    public readonly ScientificDecimal InitialTimeSincePeriapsis;
    
    public KeplerOrbit(Body Body, Body Parent, ScientificDecimal? currentTime = null)
    {
        this.Body = Body;
        this.Parent = Parent;
        
        SD_Vector2 orbitalVelocity = Body.Velocity - Parent.Velocity;
        SD_Vector2 orbitalPosition = Body.Position - Parent.Position;
        
        SD_Vector2 momentum = orbitalVelocity * Body.Mass;
        SD_Vector3 angularMomentum = SD_Vector2.Cross(orbitalPosition, momentum);
        SD_Vector2 orbitalDirectionVector = orbitalPosition.Normalize();
        ScientificDecimal forceStrength = Body.Mass * Parent.Mass * Constants.G;
        SD_Vector2 lrlVector = (SD_Vector2)SD_Vector3.Cross(momentum, angularMomentum) -
                               orbitalDirectionVector * Body.Mass * forceStrength;
        double eccentricity = (double)(lrlVector.Magnitude() / (Body.Mass * forceStrength).Abs());
        ScientificDecimal semiLatusRectum = angularMomentum.Magnitude().Square() / Body.Mass / forceStrength;
        double periapsis = lrlVector != SD_Vector2.Zero ? Utils.WrapAngle(lrlVector.Direction()) : 0;
        
        Eccentricity = eccentricity;
        SemiLatusRectum = semiLatusRectum;
        Periapsis = periapsis;
        
        //if (Eccentricity.Equals(1)) throw new NotImplementedException("Parabolic orbital evaluation is not implemented.");
        
        Equation = angle => semiLatusRectum / (1 + eccentricity * Math.Cos(angle - periapsis));
        
        if (Eccentricity == 0)
        {
            SemiMajorAxis = SemiLatusRectum;
            SemiMinorAxis = SemiLatusRectum;
            SphereOfInfluenceRadius = SemiLatusRectum * Math.Pow((double)(Body.Mass / Parent.Mass), 2f / 5f);
            Period = Math.Tau * (SemiMajorAxis * SemiMajorAxis * SemiMajorAxis / Constants.G / Parent.Mass).Sqrt();
            Center = SD_Vector2.FromPolar(Periapsis, Equation(Periapsis)) + 
                     SD_Vector2.FromPolar(Periapsis, -SemiMajorAxis);
        }
        else if (Eccentricity is > 0 and < 1)
        {
            SemiMajorAxis = (Equation(Periapsis) + Equation(Periapsis + Math.PI)) / 2;
            SemiMinorAxis = (Equation(Periapsis) * Equation(Periapsis + Math.PI)).Sqrt();
            SphereOfInfluenceRadius = SemiMajorAxis * Math.Pow((double)(Body.Mass / Parent.Mass), 2f / 5f);
            Period = Math.Tau * (SemiMajorAxis * SemiMajorAxis * SemiMajorAxis / Constants.G / Parent.Mass).Sqrt();
            Center = SD_Vector2.FromPolar(Periapsis, Equation(Periapsis)) + 
                     SD_Vector2.FromPolar(Periapsis, -SemiMajorAxis);
        }
        else if (Eccentricity >= 1)
        {
            SemiMajorAxis = Equation(Periapsis) / (1 - Eccentricity);
            SemiMinorAxis = SemiLatusRectum / Math.Sqrt(Eccentricity * Eccentricity - 1);
            SphereOfInfluenceRadius = null;
            Period = ScientificDecimal.PosInfinity;
            Center = null;
        }
        
        if (currentTime != null)
        {
            SD_Vector2 relativePosition = Body.Position - Parent.Position;
            double trueAnomaly = SD_Vector2.Direction(Parent.Position, relativePosition) - Periapsis;
            trueAnomaly = Utils.WrapAngle(trueAnomaly);
            InitialTimeSincePeriapsis = CalculateTimeSincePeriapsisFromTrueAnomaly(trueAnomaly);
            InitialTimeSincePeriapsis = Utils.UnsignedMod(InitialTimeSincePeriapsis - currentTime.Value, Period);
        }
        // no given time frame, populate initial time since periapsis with null value
        else InitialTimeSincePeriapsis = 0;
    }
    
    private static double CalculateTrueFromEccentricAnomalyElliptic(double eccentricity, double eccentricAnomaly)
    {
        double a = eccentricity / (1 + Math.Sqrt(1 - eccentricity * eccentricity));
        double trueAnomaly = eccentricAnomaly +
                             2 * Math.Atan(a * Math.Sin(eccentricAnomaly) / (1 - a * Math.Cos(eccentricAnomaly)));
        return Utils.WrapAngle(trueAnomaly);
    }
    
    private static double CalculateTrueFromEccentricAnomalyHyperbolic(double eccentricity, double eccentricAnomaly)
        => 2 * Math.Atan(Math.Tanh(eccentricAnomaly / 2) / Math.Sqrt((eccentricity - 1) / (eccentricity + 1)));

    private static double CalculateEccentricFromTrueAnomalyElliptic(double eccentricity, double trueAnomaly)
    {
        double eccentricAnomaly = Math.Atan2(
            Math.Sqrt(1 - eccentricity * eccentricity) * Math.Sin(trueAnomaly),
            eccentricity + Math.Cos(trueAnomaly)
        );
        return Utils.WrapAngle(eccentricAnomaly);
    }

    private static double CalculateEccentricFromTrueAnomalyHyperbolic(double eccentricity, double trueAnomaly)
        => 2 * Math.Atanh(Math.Sqrt((eccentricity - 1) / (eccentricity + 1)) * Math.Tan(trueAnomaly / 2));

    private static double CalculateEccentricFromMeanAnomalyElliptic(double eccentricity, double meanAnomaly)
    {
        ScientificDecimal epsilon = new ScientificDecimal(1, -10);
        double estimate = eccentricity > 0.8 ? Math.PI : meanAnomaly;
        double finalEccentricAnomaly = estimate;
        int iterations = 0;
        do {
            estimate = finalEccentricAnomaly;
            finalEccentricAnomaly = estimate - (estimate - eccentricity * Math.Sin(estimate) - meanAnomaly) /
                (1 - eccentricity * Math.Cos(estimate));
            iterations++;
        } while (double.Abs(finalEccentricAnomaly - estimate) > epsilon && iterations <= 5);
        return Utils.WrapAngle(finalEccentricAnomaly);
    }

    private static double CalculateEccentricFromMeanAnomalyHyperbolic(double eccentricity, double meanAnomaly)
    {
        ScientificDecimal epsilon = new ScientificDecimal(1, -10);
        double estimate = meanAnomaly;
        double finalEccentricAnomaly = estimate;
        int iterations = 0;
        do
        {
            estimate = finalEccentricAnomaly;
            finalEccentricAnomaly = estimate - (eccentricity * Math.Sinh(estimate) - estimate - meanAnomaly) /
                (eccentricity * Math.Cosh(estimate) - 1);
            iterations++;
        } while (double.Abs(finalEccentricAnomaly - estimate) > epsilon && iterations <= 5);
        return finalEccentricAnomaly;
    }

    private static double CalculateMeanFromEccentricAnomalyElliptic(double eccentricity, double eccentricAnomaly)
        => Utils.WrapAngle(eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly)); 
    
    private static double CalculateMeanFromEccentricAnomalyHyperbolic(double eccentricity, double eccentricAnomaly)
        => eccentricity * Math.Sinh(eccentricAnomaly) - eccentricAnomaly;
    
    private double CalculateMeanAnomalyFromTimeSincePeriapsisElliptic(ScientificDecimal timeSincePeriapsis)
        => (double)(timeSincePeriapsis * Math.Tau / Period);
        
    private double CalculateMeanAnomalyFromTimeSincePeriapsisHyperbolic(ScientificDecimal timeSincePeriapsis)
        => (double)(timeSincePeriapsis / (SemiMajorAxis.Square() * -SemiMajorAxis / (Parent.Mass * Constants.G)).Sqrt());

    private ScientificDecimal CalculateTimeSincePeriapsisFromMeanAnomalyElliptic(double meanAnomaly)
        => meanAnomaly * Period / Math.Tau;
    
    private ScientificDecimal CalculateTimeSincePeriapsisFromMeanAnomalyHyperbolic(double meanAnomaly)
        => (SemiMajorAxis.Square() * -SemiMajorAxis / (Parent.Mass * Constants.G)).Sqrt() * meanAnomaly;

    public ScientificDecimal CalculateTimeSincePeriapsisFromTrueAnomaly(double trueAnomaly)
    {
        if (Eccentricity < 1)
        {
            double eccentricAnomaly = CalculateEccentricFromTrueAnomalyElliptic(Eccentricity, trueAnomaly);
            double meanAnomaly = CalculateMeanFromEccentricAnomalyElliptic(Eccentricity, eccentricAnomaly);
            return CalculateTimeSincePeriapsisFromMeanAnomalyElliptic(meanAnomaly);
        }
        else
        {
            double eccentricAnomaly = CalculateEccentricFromTrueAnomalyHyperbolic(Eccentricity, trueAnomaly);
            double meanAnomaly = CalculateMeanFromEccentricAnomalyHyperbolic(Eccentricity, eccentricAnomaly);
            return CalculateTimeSincePeriapsisFromMeanAnomalyHyperbolic(meanAnomaly);
        }
    }

    public double CalculateTrueAnomalyFromTimeSincePeriapsis(ScientificDecimal timeFromPeriapsis)
    {
        if (Eccentricity < 1)
        {
            double meanAnomaly = CalculateMeanAnomalyFromTimeSincePeriapsisElliptic(timeFromPeriapsis);
            double eccentricAnomaly = CalculateEccentricFromMeanAnomalyElliptic(Eccentricity, meanAnomaly);
            return CalculateTrueFromEccentricAnomalyElliptic(Eccentricity, eccentricAnomaly);
        }
        else
        {
            double meanAnomaly = CalculateMeanAnomalyFromTimeSincePeriapsisHyperbolic(timeFromPeriapsis);
            double eccentricAnomaly = CalculateEccentricFromMeanAnomalyHyperbolic(Eccentricity, meanAnomaly);
            return CalculateTrueFromEccentricAnomalyHyperbolic(Eccentricity, eccentricAnomaly);
        }
    }

    public SpatialInfo GetStateAtTime(ScientificDecimal time)
    {
        if (Eccentricity < 1) time = Utils.UnsignedMod(time + InitialTimeSincePeriapsis, Period);
        SpatialInfo newState = new();
        double trueAnomaly = CalculateTrueAnomalyFromTimeSincePeriapsis(time);
        SD_Vector2 orbitalPosition = SD_Vector2.FromPolar(trueAnomaly + Periapsis, Equation(trueAnomaly + Periapsis));
        newState.Position = Parent.Position + orbitalPosition;
        return newState;
    }
}