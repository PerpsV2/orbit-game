using System;
using System.Globalization;
using BenchmarkDotNet.Loggers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;

namespace OrbitGame;

/// <summary>
/// Record struct containing information about a Keplerian orbit as well as methods for converting between certain
/// orbital parameters.
/// </summary>
public readonly record struct KeplerOrbit
{
    /// <summary>
    /// Equation which returns the distance of an object to its parent given a true anomaly.
    /// </summary>
    public delegate SDecimal OrbitEquation(double angle);
    
    public readonly Body Body;
    public readonly Body Parent;
    
    // ----- Initials ----- 
    /// <summary>
    /// Time at which the orbit was calculated.
    /// </summary>
    private readonly SDecimal _initialCalculationTime;
    private readonly SpatialInfo _initialParentSpatialInfo;
    private readonly SpatialInfo _initialOrbitalSpatialInfo;
    
    private SDecimal PBodyMass => (SDecimal)Body.Mass;
    private SDecimal PParentMass => (SDecimal) Parent.Mass;
    
    // ----- Orbital Parameters ----- 
    public readonly Vec2<SDecimal> LRLVector;
    public readonly SDecimal SemiLatusRectum;
    public readonly bool Prograde;
    
    private readonly Lazy<double> _lazyPeriapsis;
    public double Periapsis => _lazyPeriapsis.Value;
    
    private readonly Lazy<double> _lazyEccentricity;
    public double Eccentricity => _lazyEccentricity.Value;
    
    private readonly Lazy<OrbitEquation> _lazyEquation;
    private OrbitEquation Equation => _lazyEquation.Value;
    
    /// <summary>
    /// Semi-major axis of the orbital conic section. Is always negative for hyperbolas.
    /// </summary>
    private readonly Lazy<SDecimal> _lazySemiMajorAxis;
    public SDecimal SemiMajorAxis => _lazySemiMajorAxis.Value;
    
    private readonly Lazy<SDecimal> _lazySemiMinorAxis;
    public SDecimal SemiMinorAxis => _lazySemiMinorAxis.Value;
    
    private readonly Lazy<SDecimal> _lazyPeriod;
    public SDecimal Period => _lazyPeriod.Value;
    
    private readonly Lazy<SDecimal> _lazySphereOfInfluenceRadius;
    public SDecimal SphereOfInfluenceRadius => _lazySphereOfInfluenceRadius.Value;
    
    private readonly Lazy<Vec2<SDecimal>> _lazyCenter;
    public Vec2<SDecimal> Center => _lazyCenter.Value;
    
    private readonly Lazy<SDecimal> _lazyInitialTimeSincePeriapsis;
    public SDecimal InitialTimeSincePeriapsis => _lazyInitialTimeSincePeriapsis.Value;

    public KeplerOrbit(
        Body body, 
        Body parent, 
        SpatialInfo bodySpatialInfo, 
        SpatialInfo parentSpatialInfo, 
        SDecimal initialCalculationTime)
    {
        Body = body;
        Parent = parent;
        _initialCalculationTime = initialCalculationTime;
        
        _initialParentSpatialInfo = parentSpatialInfo;
        _initialOrbitalSpatialInfo = new SpatialInfo(
            bodySpatialInfo.Position - parentSpatialInfo.Position,
            bodySpatialInfo.Velocity - parentSpatialInfo.Velocity
        );

        Vec2<SDecimal> orbitalPosition = (bodySpatialInfo.Position - parentSpatialInfo.Position).Map<SDecimal>();
        Vec2<SDecimal> orbitalVelocity = (bodySpatialInfo.Velocity - parentSpatialInfo.Velocity).Map<SDecimal>();
        SDecimal pObjectMass = (SDecimal)body.Mass;
        SDecimal pParentMass = (SDecimal)parent.Mass;
        
        Vec2<SDecimal> momentum = orbitalVelocity * pObjectMass;
        Vec3<SDecimal> angularMomentum = Vec2<SDecimal>.Cross(orbitalPosition, momentum);
        Vec2<SDecimal> orbitalDirectionVector = orbitalPosition.Normalize();
        SDecimal forceStrength = pObjectMass * pParentMass * Constants.G;
        Prograde = Math.Acos(angularMomentum.Z.Positive ? 1 : -1) == 0;
        LRLVector = (Vec2<SDecimal>)Vec3<SDecimal>.Cross(momentum, angularMomentum) - 
                    orbitalDirectionVector * pObjectMass * forceStrength;
        SemiLatusRectum = SDecimal.Square(angularMomentum.Magnitude()) / pObjectMass / forceStrength;
        
        // initialize lazy fields
        _lazyEccentricity = new(LazyInitializeEccentricity);
        _lazyPeriapsis = new(LazyInitializePeriapsis);
        _lazyEquation = new(LazyInitializeEquation);
        _lazySemiMajorAxis = new(LazyInitializeSemiMajorAxis);
        _lazySemiMinorAxis = new(LazyInitializeSemiMinorAxis);
        _lazyPeriod = new(LazyInitializePeriod);
        _lazySphereOfInfluenceRadius = new(LazyInitializeSphereOfInfluenceRadius);
        _lazyCenter = new(LazyInitializeCenter);
        _lazyInitialTimeSincePeriapsis = new(LazyInitializeInitialTimeSincePeriapsis);
    }

    /// <summary>
    /// Creates a Keplerian for the orbiting body of a two-body system
    /// </summary>
    /// <param name="body">Orbiting body</param>
    /// <param name="parent">Central force body</param>
    /// <param name="initialCalculationTime">Time of orbit calculation</param>
    public KeplerOrbit(Body body, Body parent, SDecimal initialCalculationTime)
        : this(body, parent, body.SpatialInfo, parent.SpatialInfo, initialCalculationTime)
    { }

    private double LazyInitializePeriapsis()
        => LRLVector != Vec2<SDecimal>.Zero ? Utils.WrapAngle(LRLVector.Direction()) : 0;

    private double LazyInitializeEccentricity()
        => (double)(LRLVector.Magnitude() / SDecimal.Abs(PBodyMass * PBodyMass * PParentMass * Constants.G));
    
    private OrbitEquation LazyInitializeEquation()
    {
        double periapsis = Periapsis;
        double eccentricity = Eccentricity;
        SDecimal semiLatusRectum = SemiLatusRectum;
        return angle => semiLatusRectum / (1 + eccentricity * Math.Cos(angle - periapsis));
    }

    private SDecimal LazyInitializeSemiMajorAxis()
    {
        switch (Eccentricity)
        {
            case <= 0: return SemiLatusRectum;
            case > 0 and < 1: return (Equation(Periapsis) + Equation(Periapsis + Math.PI)) / 2;
            case >= 1: return Equation(Periapsis) / (1 - Eccentricity);
            default: throw new ArgumentOutOfRangeException(nameof(Eccentricity));
        }
    }

    private SDecimal LazyInitializeSemiMinorAxis()
    {
        switch (Eccentricity)
        {
            case <= 0: return SemiLatusRectum;
            case > 0 and < 1: return SDecimal.Sqrt(Equation(Periapsis) * Equation(Periapsis + Math.PI));
            case >= 1: return SemiLatusRectum / Math.Sqrt(Eccentricity * Eccentricity - 1);
            default: throw new ArgumentOutOfRangeException(nameof(Eccentricity));
        }
    }

    private SDecimal LazyInitializePeriod()
    {
        switch (Eccentricity)
        {
            case <= 0: 
            case > 0 and < 1: return
                Math.Tau * SDecimal.Sqrt(SDecimal.IntPow(SemiMajorAxis, 3) / Constants.G / Parent.Mass);
            case >= 1: return SDecimal.PositiveInfinity;
            default: throw new ArgumentOutOfRangeException(nameof(Eccentricity));
        }
    }

    private SDecimal LazyInitializeSphereOfInfluenceRadius()
    {
        switch (Eccentricity)
        {
            case <= 0:
            case > 0 and < 1: return SemiMajorAxis * Math.Pow((double)(Body.Mass / Parent.Mass), 2f / 5f);
            case >= 1: return SDecimal.PositiveInfinity;
            default: throw new ArgumentOutOfRangeException(nameof(Eccentricity));
        }
    }

    private Vec2<SDecimal> LazyInitializeCenter()
    {
        OrbitEquation equation = Equation;
        double periapsis = Periapsis;
        return Vec2<SDecimal>.FromPolar(periapsis, equation(periapsis)) - 
               Vec2<SDecimal>.FromPolar(periapsis, (equation(periapsis) + equation(periapsis + Math.PI)) / 2);
    }

    private SDecimal LazyInitializeInitialTimeSincePeriapsis()
    {
        double trueAnomaly = Utils.WrapAngle(Vec2<SDecimal>.Direction(
            _initialParentSpatialInfo.Position,
            _initialOrbitalSpatialInfo.Position
        ) - Periapsis);
        SDecimal timeSincePeriapsis = GetTimeSincePeriapsisFromTrueAnomaly(trueAnomaly);
        if (!Prograde) return Utils.UnsignedMod(_initialCalculationTime - timeSincePeriapsis, Period);
        return Utils.UnsignedMod(timeSincePeriapsis - _initialCalculationTime, Period);
    }
    
    private static double CalculateTrueFromEccentricAnomalyElliptic(double eccentricity, double eccentricAnomaly)
    {
        double a = eccentricity / (1 + Math.Sqrt(1 - eccentricity * eccentricity));
        double trueAnomaly = eccentricAnomaly +
                             2 * Math.Atan(a * Math.Sin(eccentricAnomaly) / (1 - a * Math.Cos(eccentricAnomaly)));
        return Utils.WrapAngle(trueAnomaly);
    }
    
    private static double CalculateTrueFromEccentricAnomalyHyperbolic(double eccentricity, double eccentricAnomaly)
        => 2 * Math.Atan(Math.Tanh(eccentricAnomaly / 2) * Math.Sqrt((eccentricity + 1) / (eccentricity - 1)));

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
        SDecimal epsilon = Options.EccentricAnomalyApproximationTolerance;
        double estimate = meanAnomaly;
        double finalEccentricAnomaly = estimate;
        int iterations = 0;
        do {
            estimate = finalEccentricAnomaly;
            finalEccentricAnomaly = estimate - (estimate - eccentricity * Math.Sin(estimate) - meanAnomaly) /
                (1 - eccentricity * Math.Cos(estimate));
            iterations++;
        } while (double.Abs(finalEccentricAnomaly - estimate) > epsilon && iterations <= 
                 Options.EccentricAnomalyApproximationMaxIterations);
        return Utils.WrapAngle(finalEccentricAnomaly);
    }

    private static double CalculateEccentricFromMeanAnomalyHyperbolic(double eccentricity, double meanAnomaly)
    {
        SDecimal epsilon = Options.EccentricAnomalyApproximationTolerance;
        double estimate = meanAnomaly;
        double finalEccentricAnomaly = estimate;
        int iterations = 0;
        do
        {
            estimate = finalEccentricAnomaly;
            finalEccentricAnomaly = estimate - (eccentricity * Math.Sinh(estimate) - estimate - meanAnomaly) /
                (eccentricity * Math.Cosh(estimate) - 1);
            iterations++;
            if (double.IsNaN(finalEccentricAnomaly)) return finalEccentricAnomaly;
        } while (double.Abs(finalEccentricAnomaly - estimate) > epsilon && iterations <= 
                 Options.EccentricAnomalyApproximationMaxIterations);
        return finalEccentricAnomaly;
    }

    private static double CalculateMeanFromEccentricAnomalyElliptic(double eccentricity, double eccentricAnomaly)
        => Utils.WrapAngle(eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly)); 
    
    private static double CalculateMeanFromEccentricAnomalyHyperbolic(double eccentricity, double eccentricAnomaly)
        => eccentricity * Math.Sinh(eccentricAnomaly) - eccentricAnomaly;
    
    private double CalculateMeanAnomalyFromTimeSincePeriapsisElliptic(SDecimal timeSincePeriapsis)
        => (double)(timeSincePeriapsis * Math.Tau / Period);
        
    private double CalculateMeanAnomalyFromTimeSincePeriapsisHyperbolic(SDecimal timeSincePeriapsis)
        => (double)(timeSincePeriapsis * SDecimal.Sqrt(Parent.Mass * Constants.G / 
                                                       SDecimal.IntPow(-SemiMajorAxis, 3).Map<SDecimal>()));

    private SDecimal CalculateTimeSincePeriapsisFromMeanAnomalyElliptic(double meanAnomaly)
        => meanAnomaly * Period / Math.Tau;
    
    private SDecimal CalculateTimeSincePeriapsisFromMeanAnomalyHyperbolic(double meanAnomaly)
        => SDecimal.Sqrt(SDecimal.IntPow(-SemiMajorAxis, 3) / (Parent.Mass * Constants.G)) * meanAnomaly;

    public SDecimal GetTimeSincePeriapsisFromTrueAnomaly(double trueAnomaly)
    {
        if (!Prograde) trueAnomaly = Utils.WrapAngle(-trueAnomaly);
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

    public double GetTrueAnomalyFromTimeSincePeriapsis(SDecimal timeFromPeriapsis)
    {
        double trueAnomaly;
        if (Eccentricity < 1)
        {
            double meanAnomaly = CalculateMeanAnomalyFromTimeSincePeriapsisElliptic(timeFromPeriapsis);
            double eccentricAnomaly = CalculateEccentricFromMeanAnomalyElliptic(Eccentricity, meanAnomaly);
            trueAnomaly = CalculateTrueFromEccentricAnomalyElliptic(Eccentricity, eccentricAnomaly);
        }
        else
        {
            double meanAnomaly = CalculateMeanAnomalyFromTimeSincePeriapsisHyperbolic(timeFromPeriapsis);
            double eccentricAnomaly = CalculateEccentricFromMeanAnomalyHyperbolic(Eccentricity, meanAnomaly);
            trueAnomaly = CalculateTrueFromEccentricAnomalyHyperbolic(Eccentricity, eccentricAnomaly);
        }
        if (!Prograde) trueAnomaly = Utils.WrapAngle(-trueAnomaly);
        return trueAnomaly;
    }
    
    public SDecimal GetDistanceFromTrueAnomaly(double trueAnomaly)
    {
        return Equation(trueAnomaly + Periapsis);
    }
    
    public Vec2<SDecimal> GetOrbitPositionFromTrueAnomaly(double trueAnomaly)
    {
        SDecimal distance = GetDistanceFromTrueAnomaly(trueAnomaly);
        return Vec2<SDecimal>.FromPolar(trueAnomaly + Periapsis, distance);
    }

    public Vec2<SDecimal> GetOrbitPositionFromWorldAngle(double worldAngle)
    {
        SDecimal distance = Equation(worldAngle);
        return Vec2<SDecimal>.FromPolar(worldAngle, distance);
    }

    private double GetOrbitalVelocityDirection(double trueAnomaly)
    {
        return Math.Atan2(Eccentricity + Math.Cos(trueAnomaly), -Math.Sin(trueAnomaly)) + Periapsis + 
               (Prograde ? 0 : Math.PI);
    }

    public SpatialInfo GetSpatialInfoAtTime(SDecimal time)
    {
        time += InitialTimeSincePeriapsis;
        if (Eccentricity < 1) Utils.UnsignedMod(time, Period);
        double trueAnomaly = GetTrueAnomalyFromTimeSincePeriapsis(time);
        if (double.IsNaN(trueAnomaly)) return Body.SpatialInfo;
        SDecimal orbitalDistance = Equation(trueAnomaly + Periapsis);
        Vec2<SDecimal> orbitalPosition = Vec2<SDecimal>.FromPolar(trueAnomaly + Periapsis, orbitalDistance);
        SDecimal orbitalSpeed = SDecimal.Sqrt(Parent.Mass * Constants.G * (2 / orbitalDistance - 1 / SemiMajorAxis));
        Vec2<SDecimal> orbitalVelocity = Vec2<SDecimal>.FromPolar(GetOrbitalVelocityDirection(trueAnomaly), orbitalSpeed);
        return new SpatialInfo(
            position: Parent.Position + orbitalPosition,
            velocity: Parent.Velocity + orbitalVelocity
        );
    }

    public double GetTrueAnomalyFromWorldPosition(Vec2<SDecimal> getWorldPosition)
    {
        return Utils.WrapAngle((getWorldPosition - Parent.Position).Direction() - Periapsis);
    }
}