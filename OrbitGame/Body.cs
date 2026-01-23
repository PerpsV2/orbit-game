using SkiaSharp;

namespace OrbitGame;

public delegate ScientificDecimal OrbitEquation(double angle);

public readonly record struct Orbit(
    OrbitEquation Equation, 
    double Eccentricity, 
    double Periapsis,
    ScientificDecimal SemiLatusRectum
    )
{
    public readonly double Periapsis = Utils.UnsignedMod(Periapsis, Math.Tau);
    public readonly double Apoapsis = Utils.UnsignedMod(Periapsis + Math.PI, Math.Tau);
    public readonly ScientificDecimal? SemiMajorAxis = 
        Eccentricity < 1 ? (Equation(-Periapsis) + Equation(-Periapsis + Math.PI)) / 2 : null;
    public readonly ScientificDecimal? SemiMinorAxis = 
        Eccentricity < 1 ? (Equation(-Periapsis) * Equation(-Periapsis + Math.PI)).Sqrt() : null;
}

/// <summary>
/// A KinematicObject with information about shape and material and methods for physics.
/// </summary>
public abstract class Body(
    ScientificDecimal mass, 
    Vector2 position, 
    Vector2 velocity, 
    SKColor colour, 
    string name, 
    Body? parent = null
    ) 
    : KinematicObject(name, mass, position, velocity)
{
    public SKColor Colour = colour;
    public ICollider? Collider;
    public Body? Parent = parent;
    
    public abstract void Draw(SKCanvas canvas, Camera camera);
    public abstract void DrawCollider(SKCanvas canvas, Camera camera);

    /// <summary>
    /// Draws a conical section orbit of an object around a parent using the Laplace-Runge-Lenz vector.
    /// </summary>
    public void DrawOrbitalPathLRL(SKCanvas canvas, Camera camera, Body centralForce)
    {
        // paint for orbits
        using SKPaint paint = new SKPaint();
        paint.Color = Colour;
        paint.Style = SKPaintStyle.Stroke;

        Orbit orbit = CalculateOrbit(centralForce);
        CalculateSphereOfInfluenceRadius(orbit);
        List<Vector2> orbitPoints = new List<Vector2>();
        
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
            if (orbit.SemiMajorAxis == null || orbit.SemiMinorAxis == null) 
                throw new NullReferenceException("Elliptic orbit must have a semi-major axis.");
            ScientificDecimal semiMajorAxis = (ScientificDecimal)orbit.SemiMajorAxis;
            ScientificDecimal semiMinorAxis = (ScientificDecimal)orbit.SemiMinorAxis;
            // orbit is too small to draw
            if (camera.ConvertToScreenDistance(semiMajorAxis) < 1) return;
            // draw partial orbit if camera is zoomed in.
            if (maxAngle - minAngle < Options.OrbitApproximationZoomFraction * Math.PI)
            {
                // apply inverse ellipse bias function of camera angle limits
                double unbiasedMinAngle = EllipseBiasFunction(minAngle + orbit.Periapsis, 1 / exponent);
                double unbiasedMaxAngle = EllipseBiasFunction(maxAngle + orbit.Periapsis, 1 / exponent);
                // sweep through angle range and re-apply bias function on each point then draw the orbit
                for (double a = unbiasedMinAngle; a < unbiasedMaxAngle; 
                     a += (unbiasedMaxAngle - unbiasedMinAngle) / Options.OrbitResolutionNumPoints)
                {
                    double trueAngle = EllipseBiasFunction(a, exponent) - orbit.Periapsis;
                    ScientificDecimal dist = orbit.Equation(trueAngle);
                    orbitPoints.Add(centralForce.Position + Vector2.FromPolar(trueAngle, dist));
                }
            }
            // draw the entire orbit as an ellipse
            else
            {
                Vector2 center = Vector2.FromPolar(-orbit.Periapsis, orbit.Equation(-orbit.Periapsis)) +
                                 Vector2.FromPolar(-orbit.Periapsis, -semiMajorAxis);
                canvas.GS_DrawEllipseOrbit(camera, centralForce.Position + center, semiMajorAxis, semiMinorAxis, 
                    orbit.Periapsis, DebugCanvas.Blue);
            }
        }
        // draw parabolic and hyperbolic orbits
        else
        {
            double asymptoteAngle = Utils.UnsignedMod(Math.Acos(-(1 / orbit.Eccentricity)), Math.Tau);
            for (double a = -asymptoteAngle; a < asymptoteAngle; a += 2 * asymptoteAngle / Options.OrbitResolutionNumPoints)
            {
                double trueAngle = a - orbit.Periapsis;
                ScientificDecimal dist = orbit.Equation(trueAngle);
                if (dist > 0) orbitPoints.Add(centralForce.Position + Vector2.FromPolar(trueAngle, dist));
            }
        }

        if (orbitPoints.Count > 0) canvas.GS_DrawPath(camera, orbitPoints, DebugCanvas.Purple);
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

    private Orbit CalculateOrbit(Body centralForce)
    {
        Vector2 relVelocity = Velocity - centralForce.Velocity;
        Vector2 relPosition = Position - centralForce.Position;
        
        Vector2 momentum = relVelocity * Mass;
        Vector3 angularMomentum = Vector2.Cross(relPosition, momentum);
        Vector2 directionVector = relPosition.Normalize();
        ScientificDecimal forceStrength = Mass * centralForce.Mass * Constants.G;

        Vector2 lrlVector = Matrix3X3.Scale(-1, 1) * ((Vector2)Vector3.Cross(momentum, angularMomentum) -
                            directionVector * Mass * forceStrength);

        ScientificDecimal c = Mass * forceStrength / angularMomentum.Magnitude().Square();
        ScientificDecimal eccentricity = lrlVector.Magnitude() / (Mass * forceStrength).Abs();
        ScientificDecimal semiLatusRectum = 1 / c;
        double periapsis = lrlVector.GetPrincipalAngle() + Math.PI;

        return new Orbit(
            angle => 1 / (c * (1 + eccentricity * Math.Cos(-angle - periapsis))), 
            (double)eccentricity, periapsis, semiLatusRectum
            );
    }

    public ScientificDecimal? CalculateSphereOfInfluenceRadius(Orbit orbit)
    {
        if (Parent == null) return null;
        if (orbit.SemiMajorAxis == null) return null;
        SKPaint paint = new SKPaint();
        paint.Color = new SKColor(255, 255, 255, 40);
        ScientificDecimal result = (ScientificDecimal)orbit.SemiMajorAxis * Math.Pow((double)(Mass / Parent.Mass), 2f/5f);
        DebugCanvas.Add((cnv, cam) =>
        {
            cnv.GS_DrawCircle(cam, Position, result, paint);
        });
        return result;
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
                break;
        }
    }
}