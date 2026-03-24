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
    
    private PDecimal PBodyMass => (PDecimal)Body.Mass;
    private PDecimal PParentMass => (PDecimal) Parent.Mass;
    
    // ----- Orbital Parameters ----- 
    public readonly DVector2<PDecimal> LRLVector;
    public readonly PDecimal SemiLatusRectum;
    public readonly bool Prograde;
    
    private readonly Lazy<double> _lazyPeriapsis;
    public double Periapsis => _lazyPeriapsis.Value;
    
    private readonly Lazy<double> _lazyEccentricity;
    public double Eccentricity => _lazyEccentricity.Value;
    
    private readonly Lazy<OrbitEquation> _lazyEquation;
    public OrbitEquation Equation => _lazyEquation.Value;
    
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
    
    private readonly Lazy<DVector2<SDecimal>> _lazyCenter;
    public DVector2<SDecimal> Center => _lazyCenter.Value;
    
    private readonly Lazy<SDecimal> _lazyInitialTimeSincePeriapsis;
    public SDecimal InitialTimeSincePeriapsis => _lazyInitialTimeSincePeriapsis.Value;

    /// <summary>
    /// Creates a Keplerian orbit given two bodies
    /// </summary>
    /// <param name="Body">Orbiting body</param>
    /// <param name="Parent">Central force body</param>
    /// <param name="initialCalculationTime">Time of orbit calculation</param>
    public KeplerOrbit(Body Body, Body Parent, SDecimal initialCalculationTime)
    {
        this.Body = Body;
        this.Parent = Parent;
        _initialCalculationTime = initialCalculationTime;
        
        _initialParentSpatialInfo = Parent.SpatialInfo;
        _initialOrbitalSpatialInfo = new SpatialInfo(Body.Position - Parent.Position, Body.Velocity - Parent.Velocity);

        DVector2<PDecimal> orbitalPosition = (Body.Position - Parent.Position).Map<PDecimal>();
        DVector2<PDecimal> orbitalVelocity = (Body.Velocity - Parent.Velocity).Map<PDecimal>();
        PDecimal objectMass = (PDecimal)Body.Mass;
        PDecimal parentMass = (PDecimal)Parent.Mass;
        
        DVector2<PDecimal> momentum = orbitalVelocity * objectMass;
        DVector3<PDecimal> angularMomentum = DVector2<PDecimal>.Cross(orbitalPosition, momentum);
        DVector2<PDecimal> orbitalDirectionVector = orbitalPosition.Normalize();
        PDecimal forceStrength = objectMass * parentMass * Constants.GPrecise;
        Prograde = Math.Acos(angularMomentum.Z.Positive ? 1 : -1) == 0;
        LRLVector = (DVector2<PDecimal>)DVector3<PDecimal>.Cross(momentum, angularMomentum) - 
                    orbitalDirectionVector * objectMass * forceStrength;
        SemiLatusRectum = PDecimal.Square(angularMomentum.Magnitude()) / objectMass / forceStrength;
        
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

    private double LazyInitializePeriapsis()
        => LRLVector != DVector2<PDecimal>.Zero ? Utils.WrapAngle(LRLVector.Direction()) : 0;

    private double LazyInitializeEccentricity()
        => (double)(LRLVector.Magnitude() / PDecimal.Abs(PBodyMass * PBodyMass * PParentMass * Constants.GPrecise));
    
    private OrbitEquation LazyInitializeEquation()
    {
        double periapsis = Periapsis;
        double eccentricity = Eccentricity;
        PDecimal semiLatusRectum = SemiLatusRectum;
        return angle => (SDecimal)(semiLatusRectum / (1 + eccentricity * Math.Cos(angle - periapsis)));
    }

    private SDecimal LazyInitializeSemiMajorAxis()
    {
        switch (Eccentricity)
        {
            case <= 0: return (SDecimal)SemiLatusRectum;
            case > 0 and < 1: return (Equation(Periapsis) + Equation(Periapsis + Math.PI)) / 2;
            case >= 1: return Equation(Periapsis) / (1 - Eccentricity);
            default: throw new ArgumentOutOfRangeException(nameof(Eccentricity));
        }
    }

    private SDecimal LazyInitializeSemiMinorAxis()
    {
        switch (Eccentricity)
        {
            case <= 0: return (SDecimal)SemiLatusRectum;
            case > 0 and < 1: return SDecimal.Sqrt(Equation(Periapsis) * Equation(Periapsis + Math.PI));
            case >= 1: return (SDecimal)SemiLatusRectum / Math.Sqrt(Eccentricity * Eccentricity - 1);
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
            case >= 1: return SDecimal.PosInfinity;
            default: throw new ArgumentOutOfRangeException(nameof(Eccentricity));
        }
    }

    private SDecimal LazyInitializeSphereOfInfluenceRadius()
    {
        switch (Eccentricity)
        {
            case <= 0:
            case > 0 and < 1: return SemiMajorAxis * Math.Pow((double)(Body.Mass / Parent.Mass), 2f / 5f);
            case >= 1: return SDecimal.PosInfinity;
            default: throw new ArgumentOutOfRangeException(nameof(Eccentricity));
        }
    }

    private DVector2<SDecimal> LazyInitializeCenter()
    {
        OrbitEquation equation = Equation;
        double periapsis = Periapsis;
        return DVector2<SDecimal>.FromPolar(periapsis, equation(periapsis)) - 
               DVector2<SDecimal>.FromPolar(periapsis, (equation(periapsis) + equation(periapsis + Math.PI)) / 2);
    }

    private SDecimal LazyInitializeInitialTimeSincePeriapsis()
    {
        double trueAnomaly = Utils.WrapAngle(DVector2<SDecimal>.Direction(
            _initialParentSpatialInfo.Position,
            _initialOrbitalSpatialInfo.Position
        ) - Periapsis);
        SDecimal timeSincePeriapsis = CalculateTimeSincePeriapsisFromTrueAnomaly(trueAnomaly);
        if (Eccentricity < 1) return Utils.UnsignedMod(timeSincePeriapsis - _initialCalculationTime, Period);
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
        SDecimal epsilon = Options.EccentricAnomalyApproximationTolerance;
        double estimate = eccentricity > 0.8 ? Math.PI : meanAnomaly;
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
        => (double)(timeSincePeriapsis / SDecimal.Sqrt(SDecimal.IntPow(-SemiMajorAxis, 3).Map<SDecimal>() /
                                                       (Parent.Mass * Constants.G)));

    private SDecimal CalculateTimeSincePeriapsisFromMeanAnomalyElliptic(double meanAnomaly)
        => meanAnomaly * Period / Math.Tau;
    
    private SDecimal CalculateTimeSincePeriapsisFromMeanAnomalyHyperbolic(double meanAnomaly)
        => SDecimal.Sqrt(SDecimal.IntPow(-SemiMajorAxis, 3)) / (Parent.Mass * Constants.G) * meanAnomaly;

    public SDecimal CalculateTimeSincePeriapsisFromTrueAnomaly(double trueAnomaly)
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

    public double CalculateTrueAnomalyFromTimeSincePeriapsis(SDecimal timeFromPeriapsis)
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
    
    public DVector2<SDecimal> GetOrbitPositionFromTrueAnomaly(double trueAnomaly)
    {
        SDecimal distance = Equation(trueAnomaly + Periapsis);
        return DVector2<SDecimal>.FromPolar(trueAnomaly + Periapsis, distance);
    }

    public DVector2<SDecimal> GetOrbitPositionFromWorldAngle(double worldAngle)
    {
        SDecimal distance = Equation(worldAngle);
        return DVector2<SDecimal>.FromPolar(worldAngle, distance);
    }

    private double GetOrbitalVelocityDirection(double trueAnomaly)
    {
        return Math.Atan2(Eccentricity + Math.Cos(trueAnomaly), -Math.Sin(trueAnomaly)) + Periapsis + 
               (Prograde ? 0 : Math.PI);
    }

    public SpatialInfo GetStateAtTime(SDecimal time)
    {
        time += InitialTimeSincePeriapsis;
        if (Eccentricity < 1) Utils.UnsignedMod(time, Period);
        double trueAnomaly = CalculateTrueAnomalyFromTimeSincePeriapsis(time);
        if (double.IsNaN(trueAnomaly)) return Body.SpatialInfo;
        SDecimal orbitalDistance = Equation(trueAnomaly + Periapsis);
        DVector2<SDecimal> orbitalPosition = DVector2<SDecimal>.FromPolar(trueAnomaly + Periapsis, orbitalDistance);
        SDecimal orbitalSpeed = SDecimal.Sqrt(Parent.Mass * Constants.G * (2 / orbitalDistance - 1 / SemiMajorAxis));
        DVector2<SDecimal> orbitalVelocity = DVector2<SDecimal>.FromPolar(GetOrbitalVelocityDirection(trueAnomaly), orbitalSpeed);
        return new SpatialInfo(
            position: Parent.Position + orbitalPosition,
            velocity: Parent.Velocity + orbitalVelocity
        );
    }
}