using System;

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
    public delegate ScientificDecimal OrbitEquation(double angle);
    
    public readonly Body Body;
    public readonly Body Parent;
    
    // ----- Initials ----- 
    /// <summary>
    /// Time at which the orbit was calculated.
    /// </summary>
    private readonly ScientificDecimal _initialCalculationTime;
    private readonly SpatialInfo _initialParentSpatialInfo;
    private readonly SpatialInfo _initialOrbitalSpatialInfo;
    
    // ----- Orbital Parameters ----- 
    public readonly SD_Vector2 LRLVector;
    public readonly ScientificDecimal SemiLatusRectum;
    
    private readonly Lazy<double> _lazyPeriapsis;
    public double Periapsis => _lazyPeriapsis.Value;
    
    private readonly Lazy<double> _lazyEccentricity;
    public double Eccentricity => _lazyEccentricity.Value;
    
    private readonly Lazy<OrbitEquation> _lazyEquation;
    public OrbitEquation Equation => _lazyEquation.Value;
    
    /// <summary>
    /// Semi-major axis of the orbital conic section. Is always negative for hyperbolas.
    /// </summary>
    private readonly Lazy<ScientificDecimal> _lazySemiMajorAxis;
    public ScientificDecimal SemiMajorAxis => _lazySemiMajorAxis.Value;
    
    private readonly Lazy<ScientificDecimal> _lazySemiMinorAxis;
    public ScientificDecimal SemiMinorAxis => _lazySemiMinorAxis.Value;
    
    private readonly Lazy<ScientificDecimal> _lazyPeriod;
    public ScientificDecimal Period => _lazyPeriod.Value;
    
    private readonly Lazy<ScientificDecimal> _lazySphereOfInfluenceRadius;
    public ScientificDecimal SphereOfInfluenceRadius => _lazySphereOfInfluenceRadius.Value;
    
    private readonly Lazy<SD_Vector2> _lazyCenter;
    public SD_Vector2 Center => _lazyCenter.Value;
    
    private readonly Lazy<ScientificDecimal> _lazyInitialTimeSincePeriapsis;
    public ScientificDecimal InitialTimeSincePeriapsis => _lazyInitialTimeSincePeriapsis.Value;

    /// <summary>
    /// Creates a Keplerian orbit given two bodies
    /// </summary>
    /// <param name="Body">Orbiting body</param>
    /// <param name="Parent">Central force body</param>
    /// <param name="initialCalculationTime">Time of orbit calculation</param>
    public KeplerOrbit(Body Body, Body Parent, ScientificDecimal initialCalculationTime)
    {
        this.Body = Body;
        this.Parent = Parent;
        _initialCalculationTime = initialCalculationTime;
        
        _initialParentSpatialInfo = Parent.SpatialInfo;
        _initialOrbitalSpatialInfo = new SpatialInfo(Body.Position - Parent.Position, Body.Velocity - Parent.Velocity);
        
        SD_Vector2 momentum = _initialOrbitalSpatialInfo.Velocity * Body.Mass;
        SD_Vector3 angularMomentum = SD_Vector2.Cross(_initialOrbitalSpatialInfo.Position, momentum);
        SD_Vector2 orbitalDirectionVector = _initialOrbitalSpatialInfo.Position.Normalize();
        ScientificDecimal forceStrength = Body.Mass * Parent.Mass * Constants.G;
        LRLVector = (SD_Vector2)SD_Vector3.Cross(momentum, angularMomentum) - 
                    orbitalDirectionVector * Body.Mass * forceStrength;
        SemiLatusRectum = ScientificDecimal.Square(angularMomentum.Magnitude()) / Body.Mass / forceStrength;
        
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
        => LRLVector != SD_Vector2.Zero ? Utils.WrapAngle(LRLVector.Direction()) : 0;

    private double LazyInitializeEccentricity()
        => (double)(LRLVector.Magnitude() / ScientificDecimal.Abs(Body.Mass * Body.Mass * Parent.Mass * Constants.G));
    
    private OrbitEquation LazyInitializeEquation()
    {
        double periapsis = Periapsis;
        double eccentricity = Eccentricity;
        ScientificDecimal semiLatusRectum = SemiLatusRectum;
        return angle => semiLatusRectum / (1 + eccentricity * Math.Cos(angle - periapsis));
    }

    private ScientificDecimal LazyInitializeSemiMajorAxis()
    {
        switch (Eccentricity)
        {
            case <= 0: return SemiLatusRectum;
            case > 0 and < 1: return (Equation(Periapsis) + Equation(Periapsis + Math.PI)) / 2;
            case >= 1: return Equation(Periapsis) / (1 - Eccentricity);
            default: throw new ArgumentOutOfRangeException(nameof(Eccentricity));
        }
    }

    private ScientificDecimal LazyInitializeSemiMinorAxis()
    {
        switch (Eccentricity)
        {
            case <= 0: return SemiLatusRectum;
            case > 0 and < 1: return ScientificDecimal.Sqrt(Equation(Periapsis) * Equation(Periapsis + Math.PI));
            case >= 1: return SemiLatusRectum / Math.Sqrt(Eccentricity * Eccentricity - 1);
            default: throw new ArgumentOutOfRangeException(nameof(Eccentricity));
        }
    }

    private ScientificDecimal LazyInitializePeriod()
    {
        switch (Eccentricity)
        {
            case <= 0: 
            case > 0 and < 1: return Math.Tau * ScientificDecimal.Sqrt(
                ScientificDecimal.IntPow(SemiMajorAxis, 3) / Constants.G / Parent.Mass);
            case >= 1: return ScientificDecimal.PosInfinity;
            default: throw new ArgumentOutOfRangeException(nameof(Eccentricity));
        }
    }

    private ScientificDecimal LazyInitializeSphereOfInfluenceRadius()
    {
        switch (Eccentricity)
        {
            case <= 0:
            case > 0 and < 1: return SemiMajorAxis * Math.Pow((double)(Body.Mass / Parent.Mass), 2f / 5f);
            case >= 1: return ScientificDecimal.PosInfinity;
            default: throw new ArgumentOutOfRangeException(nameof(Eccentricity));
        }
    }

    private SD_Vector2 LazyInitializeCenter()
    {
        OrbitEquation equation = Equation;
        double periapsis = Periapsis;
        return SD_Vector2.FromPolar(periapsis, equation(periapsis)) - 
               SD_Vector2.FromPolar(periapsis, (equation(periapsis) + equation(periapsis + Math.PI)) / 2);
    }

    private ScientificDecimal LazyInitializeInitialTimeSincePeriapsis()
    {
        double trueAnomaly = Utils.WrapAngle(SD_Vector2.Direction(
            _initialParentSpatialInfo.Position,
            _initialOrbitalSpatialInfo.Position
        ) - Periapsis);
        ScientificDecimal timeSincePeriapsis = CalculateTimeSincePeriapsisFromTrueAnomaly(trueAnomaly);
        if (Eccentricity < 1) return Utils.UnsignedMod(timeSincePeriapsis - _initialCalculationTime, Period);
        return timeSincePeriapsis - _initialCalculationTime;
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
        ScientificDecimal epsilon = Options.EccentricAnomalyApproximationTolerance;
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
        ScientificDecimal epsilon = Options.EccentricAnomalyApproximationTolerance;
        double estimate = meanAnomaly;
        double finalEccentricAnomaly = estimate;
        int iterations = 0;
        do
        {
            estimate = finalEccentricAnomaly;
            finalEccentricAnomaly = estimate - (eccentricity * Math.Sinh(estimate) - estimate - meanAnomaly) /
                (eccentricity * Math.Cosh(estimate) - 1);
            iterations++;
        } while (double.Abs(finalEccentricAnomaly - estimate) > epsilon && iterations <= 
                 Options.EccentricAnomalyApproximationMaxIterations);
        return finalEccentricAnomaly;
    }

    private static double CalculateMeanFromEccentricAnomalyElliptic(double eccentricity, double eccentricAnomaly)
        => Utils.WrapAngle(eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly)); 
    
    private static double CalculateMeanFromEccentricAnomalyHyperbolic(double eccentricity, double eccentricAnomaly)
        => eccentricity * Math.Sinh(eccentricAnomaly) - eccentricAnomaly;
    
    private double CalculateMeanAnomalyFromTimeSincePeriapsisElliptic(ScientificDecimal timeSincePeriapsis)
        => (double)(timeSincePeriapsis * Math.Tau / Period);
        
    private double CalculateMeanAnomalyFromTimeSincePeriapsisHyperbolic(ScientificDecimal timeSincePeriapsis)
        => (double)(timeSincePeriapsis / ScientificDecimal.Sqrt(ScientificDecimal.IntPow(-SemiMajorAxis, 3) /
                                                                (Parent.Mass * Constants.G)));

    private ScientificDecimal CalculateTimeSincePeriapsisFromMeanAnomalyElliptic(double meanAnomaly)
        => meanAnomaly * Period / Math.Tau;
    
    private ScientificDecimal CalculateTimeSincePeriapsisFromMeanAnomalyHyperbolic(double meanAnomaly)
        => ScientificDecimal.Sqrt(ScientificDecimal.IntPow(-SemiMajorAxis, 3) / (Parent.Mass * Constants.G)) * meanAnomaly;

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
    
    public SD_Vector2 GetOrbitPositionFromTrueAnomaly(double trueAnomaly)
    {
        ScientificDecimal distance = Equation(trueAnomaly + Periapsis);
        return SD_Vector2.FromPolar(trueAnomaly + Periapsis, distance);
    }

    public SpatialInfo GetStateAtTime(ScientificDecimal time)
    {
        if (Eccentricity < 1) time = Utils.UnsignedMod(time + InitialTimeSincePeriapsis, Period);
        SpatialInfo newState = new();
        double trueAnomaly = CalculateTrueAnomalyFromTimeSincePeriapsis(time);
        newState.Position = Parent.Position + GetOrbitPositionFromTrueAnomaly(trueAnomaly);
        return newState;
    }
}