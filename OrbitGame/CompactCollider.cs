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
        PhysicsCollision? collision = IntersectsWith(collider);
        if (collision == null) return;
        PhysicsCollision c1 = (PhysicsCollision)collision;
        PhysicsCollision c2 = c1.GetInverse();

        //
        Vector2 cN = Matrix3X3.Rotation(-Parent.Angle)*c1.CollisionNormal;
        Vector2 cP1 = Matrix3X3.Rotation(-Parent.Angle)*c1.CollisionPoint;
        Vector2 cP2 = Matrix3X3.Rotation(-collider.Parent.Angle)*c2.CollisionPoint;
        
        // combined linear and angular velocities
        Vector2 cV1 = c1.Velocity - Vector2.FromPolar(cP1.GetPrincipalAngle() - Math.PI / 2, c1.AngularVelocity);
        Vector2 cV2 = c2.Velocity - Vector2.FromPolar(cP2.GetPrincipalAngle() - Math.PI / 2, c2.AngularVelocity);
        
        // relative velocity
        Vector2 relV = cV2 - cV1;
        ScientificDecimal jV = Vector2.Dot(relV, cN) * -(1+ c1.Restitution);
        ScientificDecimal j = jV / (1 / c1.Mass + 1 / c2.Mass);
        
        /*
        SKPaint red = new SKPaint { Color = SKColors.Red, StrokeWidth = 4 };
        SKPaint white = new SKPaint { Color = SKColors.White, StrokeWidth = 4 };
        SKPaint blue = new SKPaint { Color = SKColors.Blue, StrokeWidth = 4 };
        SKPaint purple = new SKPaint { Color = SKColors.Purple, StrokeWidth = 4 };
         
        canvas.GS_DrawPoint(camera, Parent.ObjectToWorldSpace(cP1), blue);
        canvas.GS_DrawLine(camera, Parent.ObjectToWorldSpace(cP1), Parent.ObjectToWorldSpace(cP1 + c1.PenetrationVector), blue);
        canvas.GS_DrawLine(camera, Parent.ObjectToWorldSpace(cP1), Parent.ObjectToWorldSpace(cP1) + relV, purple);
        canvas.GS_DrawLine(camera, Parent.ObjectToWorldSpace(cP1), Parent.ObjectToWorldSpace(cP1) + Vector2.FromPolar((Matrix3X3.Rotation(-Parent.Angle)*cP1).GetPrincipalAngle() - Math.PI / 2, c1.AngularVelocity), white);
        
        canvas.GS_DrawPoint(camera, Parent.ObjectToWorldSpace(Vector2.Zero), red);
        canvas.GS_DrawLine(camera, Parent.ObjectToWorldSpace(Vector2.Zero), Parent.ObjectToWorldSpace(Vector2.Zero) - cN * j / c1.Mass, red);
        */
        
        Console.WriteLine();
        
        // apply impulse along collision normal
        Parent.Velocity -= cN * j / c1.Mass;
        collider.Parent.Velocity += cN * j / c2.Mass;
        
        // apply projection method
        Parent.Position += Matrix3X3.Rotation(-Parent.Angle) * c1.PenetrationVector * c2.Mass / (c1.Mass + c2.Mass);
        collider.Parent.Position -= Matrix3X3.Rotation(-collider.Parent.Angle) * c2.PenetrationVector * c1.Mass / (c1.Mass + c2.Mass);
    }
    
    
    public abstract bool IsEmpty();
}