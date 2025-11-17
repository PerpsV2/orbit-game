using SkiaSharp;

namespace OrbitGame;

public abstract class Body(ScientificDecimal mass, Vector2 position, Vector2 velocity, string name)
{
    protected ScientificDecimal Mass = mass;
    public Vector2 Position = position;
    public Vector2 Velocity = velocity;
    public string Name = name;
    
    private Vector2 CalculateGravitationalAcceleration(Body actor)
    {
        Vector2 direction = Vector2.DirectionVectorBetween(Position, actor.Position);
        ScientificDecimal distance = (Position - actor.Position).Magnitude();
        ScientificDecimal magnitude = Constants.G * actor.Mass / (distance * distance);
        return direction * magnitude;
    }

    private Vector2 CalculateNetAcceleration(IEnumerable<Body> actors)
    {
        Vector2 result = Vector2.Zero;
        return actors
            .Where(x => x != this)
            .Aggregate(result, (sum, next) => sum + CalculateGravitationalAcceleration(next)
        );
    }

    public void UpdatePosition(IEnumerable<Body> actors, ScientificDecimal timeStep)
    {
        Velocity += CalculateNetAcceleration(actors) * timeStep;
        Position += Velocity * timeStep;
    }

    public virtual void Draw(SKCanvas canvas, Camera camera) { }
    public virtual void DrawCollider(SKCanvas canvas, Camera camera) { }
}