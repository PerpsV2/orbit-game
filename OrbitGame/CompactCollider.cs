using System.Drawing;
using SkiaSharp;

namespace OrbitGame;

/// <summary>
/// A standard finite-sized collider with a parent object at its origin
/// </summary>
/// <remarks>
/// Classes which inherit from CompactCollider should all be able to collide with each other.
/// To prevent unnecessary code, there should only be one implementation for collisions between two types of colliders
/// located in the more 'complex' collider.
/// Complexity of colliders follows this order
/// Convex > Rectangular > Circular
/// </remarks>
public abstract class CompactCollider(KinematicObject parent, Material material) : ICollider
{
    public readonly Material Material = material;
    public readonly KinematicObject Parent = parent;
    public ScientificDecimal Inertia;
        
    public Vector2 Position => Parent.Position;
    public bool Fixed;
    
    /// <summary>
    /// Method to return the axis-aligned rectangular collider which best fits the set of points in the collider
    /// </summary>
    protected abstract RectangularCollider GetBoundingBox();
    
    public bool NearsWith(object? obj)
    {
        if (obj is ICollider col) return NearsWith(col);
        throw new ArgumentException();
    }
    
    public bool NearsWith(ICollider collider)
    {
        switch (collider)
        {
            case CompactCollider c: 
                return GetBoundingBox().IntersectsWith(c.GetBoundingBox()) != null;
            default: throw new NotSupportedException();
        }
    }
    
    
    public IIntersection? IntersectsWith(object? obj)
    {
        switch (obj)
        {
            case CircularCollider c: return IntersectsWith(c);
            case ConvexCollider c: return IntersectsWith(c);
            case RectangularCollider c : return IntersectsWith(c);
            case PointCollision c : return IntersectsWith(c);
            default: throw new ArgumentException();
        }
    }
    
    protected abstract PointCollision IntersectsWith(Vector2 point);
    protected PhysicsCollision? IntersectsWith(ICollider collider)
    {
        switch (collider)
        {
            case CircularCollider c: return IntersectsWith(c);
            case ConvexCollider c: return IntersectsWith(c);
            case RectangularCollider c : return IntersectsWith(c);
            default: throw new ArgumentException();
        }
    }

    protected abstract PhysicsCollision? IntersectsWith(CircularCollider collider);
    protected abstract PhysicsCollision? IntersectsWith(ConvexCollider collider);
    protected abstract PhysicsCollision? IntersectsWith(RectangularCollider collider);
    
    
    public void CollidesWith(object? obj, SKCanvas canvas, Camera camera)
    {
        if (obj is ICollider col) CollidesWith(col, canvas, camera);
        else throw new ArgumentException();
    }

    private void CollidesWith(ICollider collider, SKCanvas canvas, Camera camera)
    {
        if (collider is CompactCollider c) CollidesWith(c, canvas, camera);
        else throw new ArgumentException();
    }

    public void CollidesWith(CompactCollider collider, SKCanvas canvas, Camera camera)
    {
        PhysicsCollision? collision1 = IntersectsWith(collider);
        PhysicsCollision? collision2 = collider.IntersectsWith(this);
        if (collision1 == null || collision2 == null) return;
        PhysicsCollision c1 = (PhysicsCollision)collision1;
        PhysicsCollision c2 = (PhysicsCollision)collision2;
        
        SKPaint red = new SKPaint { Color = SKColors.Red, StrokeWidth = 4 };
        SKPaint orange = new SKPaint { Color = SKColors.Orange, StrokeWidth = 4 };
        SKPaint yellow = new SKPaint { Color = SKColors.Yellow, StrokeWidth = 4 };
        SKPaint green = new SKPaint { Color = SKColors.Green, StrokeWidth = 4 };
        SKPaint blue = new SKPaint { Color = SKColors.DodgerBlue, StrokeWidth = 4 };
        SKPaint purple = new SKPaint { Color = SKColors.Purple, StrokeWidth = 4 };
        SKPaint white = new SKPaint { Color = SKColors.White, StrokeWidth = 4 };
        
        canvas.GS_DrawPoint(camera, Parent.ObjectToWorldSpace(Vector2.Zero) + c1.CollisionManifold[0], blue);
        canvas.GS_DrawLineR(camera, Parent.ObjectToWorldSpace(Vector2.Zero) + c1.CollisionManifold[0], c1.PenetrationVector, blue);

        foreach (var point in c1.CollisionManifold)
            canvas.GS_DrawPoint(camera, Parent.ObjectToWorldSpace(Vector2.Zero) + point, red);

        c1.Reference.Parent.Position += c1.PenetrationVector * c1.Incident.Parent.Mass / (c1.Reference.Parent.Mass + c1.Incident.Parent.Mass);
        c2.Reference.Parent.Position += c2.PenetrationVector * c2.Incident.Parent.Mass / (c2.Reference.Parent.Mass + c2.Incident.Parent.Mass);
    }
    
    
    public abstract bool IsEmpty();
}