namespace OrbitGame;

public class CircularCollider(ScientificDecimal radius) : ICollider
{
    public bool IntersectsWith(Vector2 point)
    {
        throw new NotImplementedException();
    }

    public bool IntersectsWith(ICollider collider)
    {
        throw new NotImplementedException();
    }

    public bool CollidesWith(PointParticle point)
    {
        throw new NotImplementedException();
    }

    public void CollidesWith(ICollider collider)
    {
        throw new NotImplementedException();
    }

    public bool IsEmpty()
    {
        throw new NotImplementedException();
    }
}