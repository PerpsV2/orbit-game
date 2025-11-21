namespace OrbitGame;

public interface ICollider
{
    public bool IntersectsWith(Vector2 point);

    public bool IntersectsWith(ICollider collider);
    
    public bool CollidesWith(PointParticle point);

    public void CollidesWith(ICollider collider);

    public bool IsEmpty();
}