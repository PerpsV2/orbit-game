using SkiaSharp;

namespace OrbitGame;

/// <summary>
/// A KinematicObject with information about shape, trajectory, material and methods for physics.
/// </summary>
public abstract class Body : KinematicObject
{
    public SKColor Colour;
    public ICollider? Collider;
    protected Body? Parent;
    protected KeplerOrbit? Orbit;
    
    protected Body(ScientificDecimal mass, 
        Vector2 position, 
        Vector2 velocity, 
        SKColor colour, 
        string name, 
        Body? parent) 
        : base(name, mass, position, velocity)
    {
        Colour = colour;
        Parent = parent;
        Position = position + parent?.Position ?? Vector2.Zero;
        Velocity = velocity + parent?.Velocity ?? Vector2.Zero;
        Orbit = CalculateOrbit(true);
    }

    public abstract void Draw(SKCanvas canvas, Camera camera);
    public abstract void DrawCollider(SKCanvas canvas, Camera camera);

    /// <summary>
    /// Draws a conical section orbit of an object around a parent using the Laplace-Runge-Lenz vector.
    /// </summary>
    public void DrawOrbitalPathLRL(SKCanvas canvas, Camera camera)
    {
        if (Orbit == null || Parent == null) return;
        KeplerOrbit orbit = (KeplerOrbit)Orbit;
        Body centralForce = Parent;
        List<Vector2> orbitPoints = new List<Vector2>();
        
        using SKPaint paint = new SKPaint();
        paint.Color = Colour;
        paint.Style = SKPaintStyle.Stroke;
        
        // find approximate angle of the orbit covered by the camera
        Vector2 relCamPosition = camera.AbsolutePosition - centralForce.Position;
        double minAngle = 0;
        double maxAngle = Math.Tau;
        if (relCamPosition != Vector2.Zero)
        {
            Vector2 maxCamExtentVector = (Vector2)Vector3.Cross(relCamPosition.Normalize(),
                new(0, 0, ScientificDecimal.Max(camera.Width, camera.Height)));
            minAngle = Utils.UnsignedMod((relCamPosition + maxCamExtentVector).GetPrincipalAngle(), Math.Tau);
            maxAngle = Utils.UnsignedMod((relCamPosition - maxCamExtentVector).GetPrincipalAngle(), Math.Tau);
        }
        if (minAngle > maxAngle) maxAngle += Math.Tau;
        
        // redistribute angles between 0 and tau to be biased towards pi (argument of apoapsis)
        double EllipseBiasFunction(double angle, double exponent)
        {
            angle = Utils.UnsignedMod(angle, Math.Tau);
            double result = Math.PI - Math.PI * Math.Pow(1 - angle / Math.PI, exponent);
            if (angle > Math.PI) result = Math.PI + Math.PI * Math.Pow(angle / Math.PI - 1, exponent);
            return result;
        }
        
        double exponent = Options.EllipsePointDistributionBiasStrength * 
            Math.Pow(orbit.Eccentricity, 1 - orbit.Eccentricity) + 1;
        
        // draw circular and elliptical orbits
        if (orbit.Eccentricity < 1)
        {
            ScientificDecimal semiMajorAxis = orbit.SemiMajorAxis;
            ScientificDecimal semiMinorAxis = orbit.SemiMinorAxis;
            
            // orbit is too small to draw
            if (camera.ConvertToScreenDistance(semiMajorAxis) < 1) return;
            // draw partial orbit if camera is zoomed in.
            if (maxAngle - minAngle < Options.OrbitApproximationZoomFraction * Math.PI)
            {
                // apply inverse ellipse bias function of camera angle limits
                double unbiasedMinAngle = EllipseBiasFunction(minAngle - orbit.Periapsis, 1 / exponent);
                double unbiasedMaxAngle = EllipseBiasFunction(maxAngle - orbit.Periapsis, 1 / exponent);
                // sweep through angle range and re-apply bias function on each point then draw the orbit
                for (double a = unbiasedMinAngle; a < unbiasedMaxAngle; 
                     a += (unbiasedMaxAngle - unbiasedMinAngle) / Options.OrbitResolutionNumPoints)
                {
                    double trueAngle = EllipseBiasFunction(a, exponent) + orbit.Periapsis;
                    ScientificDecimal dist = orbit.Equation(trueAngle);
                    orbitPoints.Add(centralForce.Position + Vector2.FromPolar(trueAngle, dist));
                }
            }
            // draw the entire orbit as an ellipse
            else
            {
                if (orbit.Center == null) throw new NullReferenceException("Elliptic orbit must have a center.");
                canvas.GS_DrawEllipseOrbit(camera, centralForce.Position + orbit.Center.Value, semiMajorAxis, 
                    semiMinorAxis, orbit.Periapsis, paint);
            }
        }
        // draw parabolic and hyperbolic orbits
        else
        {
            ScientificDecimal? parentSOIRadius = centralForce.Orbit?.SphereOfInfluenceRadius ?? null;
            double asymptoteAngle = Utils.UnsignedMod(Math.Acos(-(1 / orbit.Eccentricity)), Math.Tau);
            for (double a = -asymptoteAngle; a < asymptoteAngle; a += 2 * asymptoteAngle / Options.OrbitResolutionNumPoints)
            {
                double trueAngle = a + orbit.Periapsis;
                ScientificDecimal dist = orbit.Equation(trueAngle);
                if (parentSOIRadius != null)
                {
                    if (dist > 0 && dist < parentSOIRadius)
                        orbitPoints.Add(centralForce.Position + Vector2.FromPolar(trueAngle, dist));
                }
                else if (dist > 0 && !dist.IsInfinite) 
                    orbitPoints.Add(centralForce.Position + Vector2.FromPolar(trueAngle, dist));
            }

            if (parentSOIRadius != null)
            {
                double escapeAngle = Math.Acos((double)((parentSOIRadius / orbit.SemiLatusRectum - 1) /
                                                        (orbit.Eccentricity * parentSOIRadius /
                                                         orbit.SemiLatusRectum))) + Math.PI;
                orbitPoints.Add(centralForce.Position + Vector2.FromPolar(-escapeAngle + orbit.Periapsis, parentSOIRadius.Value));
                orbitPoints.Insert(0, centralForce.Position + Vector2.FromPolar(escapeAngle + orbit.Periapsis, parentSOIRadius.Value));
            }
        }

        if (orbitPoints.Count > 0) canvas.GS_DrawPath(camera, orbitPoints, paint);
    }
    
    private Vector2 CalculateGravitationalAcceleration(Body attractor)
    {
        Vector2 direction = Vector2.DirectionVectorBetween(Position, attractor.Position);
        ScientificDecimal distance = (Position - attractor.Position).Magnitude();
        ScientificDecimal magnitude = Constants.G * attractor.Mass / (distance * distance);
        return direction * magnitude;
    }

    public Vector2 SetNetGravitationalAcceleration(IEnumerable<Body> attractors)
    {
        Vector2 result = Vector2.Zero;
        return Acceleration = attractors
            .Where(x => x != this)
            .Aggregate(result, (sum, next) => 
                sum + CalculateGravitationalAcceleration(next));
    }
    
    private KeplerOrbit CalculateOrbit(Body centralForce, bool initials)
    {
        Vector2 relVelocity = Velocity - centralForce.Velocity;
        Vector2 relPosition = Position - centralForce.Position;
        
        Vector2 momentum = relVelocity * Mass;
        Vector3 angularMomentum = Vector2.Cross(relPosition, momentum);
        Vector2 directionVector = relPosition.Normalize();
        ScientificDecimal forceStrength = Mass * centralForce.Mass * Constants.G;

        Vector2 lrlVector = Matrix3X3.Scale(-1, 1) * ((Vector2)Vector3.Cross(momentum, angularMomentum) -
                            directionVector * Mass * forceStrength);

        double periapsis = Utils.UnsignedMod(Math.PI - lrlVector.GetPrincipalAngle(), Math.Tau);
        ScientificDecimal c = Mass * forceStrength / angularMomentum.Magnitude().Square();
        ScientificDecimal eccentricity = lrlVector.Magnitude() / (Mass * forceStrength).Abs();
        ScientificDecimal semiLatusRectum = 1 / c;

        return new KeplerOrbit(this, centralForce, (double)eccentricity, periapsis, semiLatusRectum, initials);
    }

    private KeplerOrbit? CalculateOrbit(bool initials)
    {
        return Parent == null ? null : CalculateOrbit(Parent, initials);
    }

    public void RecalculateOrbit(List<Body> bodies)
    {
        
        if (Parent == null) return;
        ScientificDecimal? parentSOIRadius = Parent.Orbit?.SphereOfInfluenceRadius;
        if (parentSOIRadius != null)
            if ((Position - Parent.Position).Magnitude() > (ScientificDecimal)parentSOIRadius)
                Parent = Parent.Parent ?? throw new ArgumentException("Parent with SOI has no parent itself.");
        foreach (Body body in bodies)
        {
            if (body == Parent || body == this) continue;
            ScientificDecimal? bodySOIRadius = body.Orbit?.SphereOfInfluenceRadius;
            if (bodySOIRadius != null)
                if ((Position - body.Position).Magnitude() < (ScientificDecimal)bodySOIRadius)
                    Parent = body;
        }
        Orbit = CalculateOrbit(false);
    }

    public void NI_UpdatePosition(ScientificDecimal timeStep, NumericalIntegrator integrator, Action<Body> updateAcceleration)
    {
        switch (integrator)
        {
            case NumericalIntegrator.ExplicitEuler:
                Velocity += Acceleration * timeStep;
                Position += Velocity * timeStep;
                Angle += AngularVelocity * (double)timeStep;
                break;
            case NumericalIntegrator.ImplicitEuler:
                Position += Velocity * timeStep;
                Velocity += Acceleration * timeStep;
                Angle += AngularVelocity * (double)timeStep;
                break;
            case NumericalIntegrator.RungeKutta4:
                Vector2 originalPosition = Position;
                Vector2 originalVelocity = Velocity;
                
                Vector2 originalAcceleration = Acceleration;
                Vector2 velocityK1 = originalAcceleration * timeStep;
                Vector2 positionK1 = Velocity * timeStep;

                Position = originalPosition + positionK1 * 0.5f;
                Velocity = originalVelocity + velocityK1 * 0.5f;
                
                updateAcceleration(this);
                Vector2 velocityK2 = Acceleration * timeStep;
                Vector2 positionK2 = Velocity * timeStep;

                Position = originalPosition + positionK2 * 0.5f;
                Velocity = originalVelocity + velocityK2 * 0.5f;
                
                updateAcceleration(this);
                Vector2 velocityK3 = Acceleration * timeStep;
                Vector2 positionK3 = Velocity * timeStep;

                Position = originalPosition + positionK3;
                Velocity = originalVelocity + velocityK3;
                
                updateAcceleration(this);
                Vector2 velocityK4 = Acceleration * timeStep;
                Vector2 positionK4 = Velocity * timeStep;

                Velocity = originalVelocity + (velocityK1 + velocityK2 * 2 + velocityK3 * 2 + velocityK4) * (1f / 6f);
                Position = originalPosition + (positionK1 + positionK2 * 2 + positionK3 * 2 + positionK4) * (1f / 6f);
                
                Angle += AngularVelocity * (double)timeStep;
                break;
        }
    }

    private double CalculateEccentricAnomaly(double meanAnomaly)
    {
        if (Orbit == null) throw new NullReferenceException("Orbit cannot be null.");
        KeplerOrbit orbit = (KeplerOrbit)Orbit;
        ScientificDecimal epsilon = new ScientificDecimal(1m, -35);
        double eccentricAnomaly = meanAnomaly;
        int iterations = 0;
        while (double.Abs(eccentricAnomaly - orbit.Eccentricity * Math.Sin(eccentricAnomaly) - meanAnomaly) > epsilon)
        {
            if (iterations > 100) return eccentricAnomaly;
            eccentricAnomaly -= (eccentricAnomaly - orbit.Eccentricity * Math.Sin(eccentricAnomaly) - meanAnomaly) /
                                (1 - orbit.Eccentricity * Math.Cos(eccentricAnomaly));
            iterations++;
        }
        return eccentricAnomaly;
    }

    private double CalculateTrueAnomaly(double eccentricAnomaly)
    {
        if (Orbit == null) throw new NullReferenceException("Orbit cannot be null.");
        KeplerOrbit orbit = (KeplerOrbit)Orbit;
        return 2 * Math.Atan2(Math.Sqrt(1 + orbit.Eccentricity) * Math.Sin(eccentricAnomaly / 2), 
            Math.Sqrt(1 - orbit.Eccentricity) * Math.Cos(eccentricAnomaly / 2));
    }

    public void Kepler_UpdatePosition(ScientificDecimal totalTime)
    {
        if (Orbit == null) return;
        if (Parent == null) return;
        KeplerOrbit orbit = (KeplerOrbit)Orbit;
        if (orbit.Center == null) return;
        Vector2 center = (Vector2)orbit.Center;
        
        double meanAnomaly = (double)(Math.Tau / orbit.Period * (totalTime + orbit.InitialTime!)) + orbit.Periapsis;
        double eccentricAnomaly = CalculateEccentricAnomaly(meanAnomaly - orbit.Periapsis) + orbit.Periapsis;
        double trueAnomaly = CalculateTrueAnomaly(eccentricAnomaly - orbit.Periapsis) + orbit.Periapsis;
        Position = Parent.Position + Vector2.FromPolar(trueAnomaly, orbit.Equation(trueAnomaly));
        
        ScientificDecimal relDist = (Position - Parent.Position).Magnitude();
        ScientificDecimal relSpeed = (Constants.G * Parent.Mass * (2 / relDist - 1 / orbit.SemiMajorAxis)).Sqrt();
        Vector2 relVelocity = Vector2.FromPolar(Vector2.DirectionVectorBetween(center, Position).GetPrincipalAngle() + 
                                                Math.PI / 2, relSpeed);
        Velocity = Parent.Velocity + relVelocity;
    }
}