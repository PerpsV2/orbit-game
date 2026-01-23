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
    public readonly ScientificDecimal? SemiMajorAxis = Eccentricity < 1 ? (Equation(-Periapsis) + Equation(-Periapsis + Math.PI)) / 2 : null;
    public readonly ScientificDecimal? SemiMinorAxis = Eccentricity < 1 ? (Equation(-Periapsis) * Equation(-Periapsis + Math.PI)).Sqrt() : null;

}

/// <summary>
/// A KinematicObject with information about shape and material and methods for physics.
/// </summary>
public abstract class Body(ScientificDecimal mass, Vector2 position, Vector2 velocity, SKColor colour, string name) 
    : KinematicObject(name, mass, position, velocity)
{
    public SKColor Colour = colour;
    public ICollider? Collider;
    
    public abstract void Draw(SKCanvas canvas, Camera camera);
    public abstract void DrawCollider(SKCanvas canvas, Camera camera);

    public void DrawOrbitalPathLRL(SKCanvas canvas, Camera camera, Body centralForce)
    {
        Orbit orbit = CalculateOrbit(centralForce);
        List<Vector2> orbitPoints = new List<Vector2>();
        Vector2 relCamPosition = camera.AbsolutePosition - centralForce.Position;
        Vector2 maxCamExtentVector = (Vector2)Vector3.Cross(relCamPosition.Normalize(), 
            new(0, 0, ScientificDecimal.Max(camera.Width, camera.Height)));
        double minAngle = Utils.UnsignedMod((relCamPosition + maxCamExtentVector).GetPrincipalAngle(), Math.Tau);
        double maxAngle = Utils.UnsignedMod((relCamPosition - maxCamExtentVector).GetPrincipalAngle(), Math.Tau);
        if (minAngle > maxAngle) maxAngle += Math.Tau;
        
        double EllipseBiasFunction(double angle, double exponent)
        {
            angle = Utils.UnsignedMod(angle, Math.Tau);
            double result = Math.PI - Math.PI * Math.Pow(1 - angle / Math.PI, exponent);
            if (angle > Math.PI) result = Math.PI + Math.PI * Math.Pow(angle / Math.PI - 1, exponent);
            return result;
        }
        
        double exponent = Options.EllipsePointDistributionBiasStrength * 
            Math.Pow(orbit.Eccentricity, 1 - orbit.Eccentricity) + 1;

        if (orbit.Eccentricity < 1)
        {
            if (maxAngle - minAngle < 0.01 * Math.PI)
            {
                double unbiasedMinAngle = EllipseBiasFunction(minAngle + orbit.Periapsis, 1 / exponent);
                double unbiasedMaxAngle = EllipseBiasFunction(maxAngle + orbit.Periapsis, 1 / exponent);
                for (double a = unbiasedMinAngle; a < unbiasedMaxAngle; 
                     a += (unbiasedMaxAngle - unbiasedMinAngle) / Options.OrbitResolutionNumPoints)
                {
                    double trueAngle = EllipseBiasFunction(a, exponent) - orbit.Periapsis;
                    ScientificDecimal dist = orbit.Equation(trueAngle);
                    orbitPoints.Add(centralForce.Position + Vector2.FromPolar(trueAngle, dist));
                    canvas.GS_DrawPoint(camera, centralForce.Position + Vector2.FromPolar(trueAngle, dist), DebugCanvas.Red);
                }
            }
            else
            {
                Vector2 center = Vector2.FromPolar(-orbit.Periapsis, orbit.Equation(-orbit.Periapsis)) +
                                 Vector2.FromPolar(-orbit.Periapsis, -(ScientificDecimal)orbit.SemiMajorAxis!);
                canvas.GS_DrawEllipseOrbit(camera, centralForce.Position + center, (ScientificDecimal)orbit.SemiMajorAxis,
                    (ScientificDecimal)orbit.SemiMinorAxis!, orbit.Periapsis, DebugCanvas.Blue);
            }
        }
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

    public Orbit CalculateOrbit(Body centralForce)
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
    
    public void NI_UpdatePosition(ScientificDecimal timeStep, NumericalIntegrator integrator)
    {
        switch (integrator)
        {
            case NumericalIntegrator.ExplicitEuler:
                Velocity += Acceleration * timeStep;
                Position += Velocity * timeStep;
                Angle += AngularVelocity * (double)timeStep;
                break;
            case NumericalIntegrator.ImplicitEuler:
                throw new NotImplementedException();
            case NumericalIntegrator.RungeKutta4:
                throw new NotImplementedException();
        }
    }
}