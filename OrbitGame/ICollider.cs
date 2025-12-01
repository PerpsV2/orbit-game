namespace OrbitGame;

public interface ICollider
{
    public bool IntersectsWith(Vector2 point);
    public bool IntersectsWith(CircularCollider collider);
    public bool IntersectsWith(ConvexCollider collider);
    public bool IntersectsWith(ICollider collider);
    public void CollidesWith(ICollider collider);
    public void CollidesWith(Body body);
    public bool IsEmpty();
}