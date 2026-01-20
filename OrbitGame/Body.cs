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
        
        Vector2 lrlVector = CalculateLRLVector(centralForce);
        ScientificDecimal k = Mass * centralForce.Mass * Constants.G;

        ScientificDecimal c = Mass * k / angularMomentum.Magnitude().Square();
        ScientificDecimal e = lrlVector.Magnitude() / (Mass * k).Abs();

        ScientificDecimal Orbit(double angle) => 1 / (c * (1 + e * Math.Cos(angle)));
        
        DebugCanvas.Add((cnv, cam) => {
            List<Vector2> orbitPoints = new List<Vector2>();
            for (double i = 0; i < 2 * Math.PI; i += Math.PI / 30)
                orbitPoints.Add(centralForce.Position + Vector2.FromPolar(i, Orbit(i)));
            cnv.GS_DrawPath(cam, orbitPoints, DebugCanvas.Purple);
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