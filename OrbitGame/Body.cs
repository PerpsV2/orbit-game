using SkiaSharp;

namespace OrbitGame;

public abstract class Body(ScientificDecimal mass, Vector2 position, Vector2 velocity, string name)
    : PointParticle(mass, position, velocity)
{
    public string Name = name;
    protected ICollider collider;

    public abstract void Draw(SKCanvas canvas, Camera camera);
    public abstract void DrawCollider(SKCanvas canvas, Camera camera);

    public bool CollidesWith(Body body)
    {
        collider.IntersectsWith(body.collider);
    }
}