using System;
using System.Drawing;

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
public abstract class CompactCollider(KinematicObject parent) : ICollider
{
    public readonly KinematicObject Parent = parent;
    public ScientificDecimal Inertia;
        
    public SD_Vector2 Position => Parent.Position;
    public bool Fixed;
    
    /// <summary>
    /// Method to return the axis-aligned rectangular collider which best fits the set of points in the collider
    /// </summary>
    public abstract RectangularCollider GetBoundingBox();
    
    public bool NearsWith(object? obj)
    {
        if (obj is ICollider col) return NearsWith(col);
        throw new ArgumentException();
    }
    
    protected virtual bool NearsWith(ICollider collider)
    {
        switch (collider)
        {
            case CompactCollider c: 
                return GetBoundingBox().NearsWith(c.GetBoundingBox());
            default: throw new NotSupportedException();
        }
    }
    
    
    public IIntersection? IntersectsWith(object? obj)
    {
        switch (obj)
        {
            case ICollider c: return IntersectsWith(c);
            case SD_Vector2 c : return IntersectsWith(c);
            default: throw new ArgumentException();
        }
    }
    
    protected abstract PointCollision IntersectsWith(SD_Vector2 point);
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
    
    
    public void CollidesWith(object? obj)
    {
        if (obj is ICollider col) CollidesWith(col);
        else throw new ArgumentException();
    }

    private void CollidesWith(ICollider collider)
    {
        if (collider is CompactCollider c) CollidesWith(c);
        else throw new ArgumentException();
    }

    public void CollidesWith(CompactCollider collider)
    {
        PhysicsCollision? collision1 = IntersectsWith(collider);
        PhysicsCollision? collision2 = collider.IntersectsWith(this);
        if (collision1 == null || collision2 == null) return;
        PhysicsCollision c1 = (PhysicsCollision)collision1;
        PhysicsCollision c2 = (PhysicsCollision)collision2;

        if (c1.PenetrationVector.Magnitude() == 0) return;
        SD_Vector2 cNormal = -c1.PenetrationVector.Normalize();
        
        KinematicObject reference = c1.Reference.Parent;
        KinematicObject incidence = c2.Reference.Parent;
        
        // TODO: account for multiple points of collision (the manifold) and subsequently calculate the collision point to use 
        SD_Vector2 cPr = SD_Vector2.Zero;
        foreach (var collisionPoint in c1.CollisionManifold)
            cPr = collisionPoint;
        
        SD_Vector2 cPi = SD_Vector2.Zero;
        foreach (var collisionPoint in c2.CollisionManifold)
            cPi = collisionPoint;
        
        // calculation combined linear and angular velocity of collision point
        SD_Vector2 pVr = reference.Velocity - (SD_Vector2)SD_Vector3.Cross(cPr, new(0, 0, reference.AngularVelocity));
        SD_Vector2 pVi = incidence.Velocity - (SD_Vector2)SD_Vector3.Cross(cPi, new(0, 0, incidence.AngularVelocity));
        SD_Vector2 relV = pVi - pVr;
        
        // calculate the magnitude of impulse
        ScientificDecimal jV = -(1 + c1.Restitution) * SD_Vector2.Dot(relV, cNormal);
        SD_Vector3 m1 = SD_Vector3.Cross(SD_Vector2.Cross(cPr, cNormal) / c1.Reference.Inertia, cPr);
        SD_Vector3 m2 = SD_Vector3.Cross(SD_Vector2.Cross(cPi, cNormal) / c2.Reference.Inertia, cPi);
        ScientificDecimal j = jV / (SD_Vector2.Dot(cNormal, cNormal * (1 / reference.Mass + 1 / incidence.Mass)) 
                                    + SD_Vector3.Dot(m1 + m2, cNormal));
        
        if (!Fixed) reference.Velocity -= cNormal * (j / reference.Mass);
        if (!collider.Fixed) incidence.Velocity += cNormal * (j / incidence.Mass);

        if (!Fixed)reference.AngularVelocity -= (double)(SD_Vector2.Cross(cPr, cNormal * j).Z / c1.Reference.Inertia);
        if (!collider.Fixed) incidence.AngularVelocity += (double)(SD_Vector2.Cross(cPi, cNormal * j).Z / c2.Reference.Inertia);

        if (!Fixed && !collider.Fixed)
        {
            reference.Position += c1.PenetrationVector * incidence.Mass / (incidence.Mass + reference.Mass);
            incidence.Position += c2.PenetrationVector * reference.Mass / (incidence.Mass + reference.Mass);
        }
        else if (Fixed) incidence.Position += c2.PenetrationVector;
        else if (collider.Fixed) reference.Position += c1.PenetrationVector;

        /*if (Options.EnableCollisionDebug)
            DebugCanvas.Add((cnv, cam) => {
                cnv.GS_DrawPoint(cam, reference.Position + cPr, DebugCanvas.Blue);
                cnv.GS_DrawLineR(cam, reference.Position + cPr, c1.PenetrationVector, DebugCanvas.Blue);
                foreach (var point in c1.CollisionManifold)
                    cnv.GS_DrawPoint(cam, reference.Position + point, DebugCanvas.Red);
                cnv.GS_DrawLineR(cam, reference.Position + cPr, relV, DebugCanvas.Yellow);
                cnv.GS_DrawLineR(cam, reference.Position + cPr, cNormal * (j / reference.Mass), DebugCanvas.Orange);
            });*/
    }
    
    
    public abstract bool IsEmpty();
}