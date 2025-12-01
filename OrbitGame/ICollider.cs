namespace OrbitGame;

public interface ICollider
{
    public bool NearsWith(ICollider collider);
    public bool IntersectsWith(Vector2 point);
    public bool IntersectsWith(ICollider collider);
    public void CollidesWith(ICollider collider);
    public bool IsEmpty();
}