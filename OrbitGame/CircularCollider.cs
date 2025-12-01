using System.Runtime.CompilerServices;

namespace OrbitGame;

public class CircularCollider(ScientificDecimal radius, KinematicObject parent) : ICollider
{
    private readonly KinematicObject _parent = parent;
    private readonly ScientificDecimal _radius = radius;
    public bool Fixed = false;
    
    public bool IntersectsWith(Vector2 point)
    {
        return (point - _parent.Position).Magnitude() <= _radius;
    }

    public bool IntersectsWith(CircularCollider collider)
    {
        return (collider._parent.Position - _parent.Position).Magnitude() <= collider._radius + _radius;
    }

    public bool IntersectsWith(ConvexCollider collider)
    {
        throw new NotImplementedException();
    }

    public bool IntersectsWith(ICollider collider)
    {
        if (collider.GetType() == typeof(CircularCollider)) return IntersectsWith((CircularCollider)collider);
        if (collider.GetType() == typeof(ConvexCollider)) return IntersectsWith((ConvexCollider)collider);
        throw new NotImplementedException();
    }

    public void CollidesWith(ICollider collider)
    {
        IntersectsWith(collider);
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