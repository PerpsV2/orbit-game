using SkiaSharp;

namespace OrbitGame;

public delegate ScientificDecimal OrbitEquation(double angle);

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

    public Vector2 CalculateLRLVector(Body centralForce)
    {
        Vector2 relVelocity = Velocity - centralForce.Velocity;
        Vector2 relPosition = Position - centralForce.Position;
        
        Vector2 momentum = relVelocity * Mass;
        Vector3 angularMomentum = Vector2.Cross(relPosition, momentum);
        Vector2 directionVector = relPosition.Normalize();
        ScientificDecimal k = Mass * centralForce.Mass * Constants.G;

        Vector2 lrlVector = (Vector2)Vector3.Cross(momentum, angularMomentum) -
                            directionVector * Mass * k;
        return lrlVector;
    }

    public OrbitEquation CalculateOrbitEquation(Body centralForce)
    {
        Vector2 relVelocity = Velocity - centralForce.Velocity;
        Vector2 relPosition = Position - centralForce.Position;
        Vector2 momentum = relVelocity * Mass;
        Vector3 angularMomentum = Vector2.Cross(relPosition, momentum);
        
        Vector2 lrlVector = Matrix3X3.Scale(-1, 1) * CalculateLRLVector(centralForce);
        ScientificDecimal k = Mass * centralForce.Mass * Constants.G;

        ScientificDecimal c = Mass * k / angularMomentum.Magnitude().Square();
        ScientificDecimal e = lrlVector.Magnitude() / (Mass * k).Abs();

        double periapsis = lrlVector.GetPrincipalAngle() + Math.PI;

        ScientificDecimal Orbit(double angle) => 1 / (c * (1 + e * Math.Cos(angle + periapsis)));
        
        DebugCanvas.Add((cnv, cam) => {
            List<Vector2> orbitPoints = new List<Vector2>();
            Vector2 relCamPosition = cam.AbsolutePosition - centralForce.Position;
            Vector2 angleRangeVector = (Vector2)Vector3.Cross(relCamPosition.Normalize(), new(0, 0, cam.Width));
            double minAngle = Utils.UnsignedMod((relCamPosition + angleRangeVector).GetPrincipalAngle(), Math.Tau);
            double maxAngle = Utils.UnsignedMod((relCamPosition - angleRangeVector).GetPrincipalAngle(), Math.Tau);
            if (minAngle > maxAngle) maxAngle += Math.Tau;
            
            if (maxAngle - minAngle > Math.PI * 0.75f) 
                for (double a = 0; a < Math.Tau; a += Math.Tau / Options.OrbitResolutionNumPoints)
                    orbitPoints.Add(centralForce.Position + Vector2.FromPolar(a, Orbit(a)));
            else 
                for (double a = minAngle; a < maxAngle; a += (maxAngle - minAngle) / Options.OrbitResolutionNumPoints)
                    orbitPoints.Add(centralForce.Position + Vector2.FromPolar(a, Orbit(a)));
            if (orbitPoints.Count > 0) cnv.GS_DrawPath(cam, orbitPoints, DebugCanvas.Purple);
        });
        
        return Orbit;
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