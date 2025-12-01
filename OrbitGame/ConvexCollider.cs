namespace OrbitGame;

public class ConvexCollider(Vector2 points) : ICollider
{
    public bool IntersectsWith(Vector2 point)
    {
        throw new NotImplementedException();
    }
    
    public bool IntersectsWith(ICollider collider)
    {
        throw new NotImplementedException();
    }

    public void CollidesWith(ICollider collider)
    {
        throw new NotImplementedException();
    }

    public void CollidesWith(Body body)
    {
        throw new NotImplementedException();
    }

    public bool IsEmpty()
    {
        throw new NotImplementedException();
    }
}