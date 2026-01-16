using SkiaSharp;

namespace OrbitGame;

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