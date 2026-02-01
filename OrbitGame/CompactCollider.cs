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
public abstract class CompactCollider
{
    public ScientificDecimal Inertia;
        
    public bool Fixed;
    
    /// <summary>
    /// Method to return the axis-aligned rectangular collider which best fits the set of points in the collider
    /// </summary>
    public abstract RectangularCollider GetBoundingBox();
    
    public virtual bool NearsWith(CompactCollider collider, SD_Vector2 relPosition, double relAngle)
    { 
        return GetBoundingBox().NearsWith(collider.GetBoundingBox(), relPosition, relAngle);
    }
    
    public abstract PointCollision IntersectsWith(SD_Vector2 position, SD_Vector2 point);
    public PhysicsCollision? IntersectsWith(CompactCollider collider, SD_Vector2 relPosition, double relAngle)
    {
        switch (collider)
        {
            case CircularCollider c: return IntersectsWith(c, relPosition, relAngle);
            case ConvexCollider c: return IntersectsWith(c, relPosition, relAngle);
            case RectangularCollider c : return IntersectsWith(c, relPosition, relAngle);
            default: throw new ArgumentException();
        }
    }

    protected abstract PhysicsCollision? IntersectsWith(CircularCollider collider, SD_Vector2 relPosition, double relAngle);
    protected abstract PhysicsCollision? IntersectsWith(ConvexCollider collider, SD_Vector2 relPosition, double relAngle);
    protected abstract PhysicsCollision? IntersectsWith(RectangularCollider collider, SD_Vector2 relPosition, double relAngle);

    public void CollidesWith(KinematicObject reference, CompactCollider collider, KinematicObject incident)
    {
        /*SD_Vector2 relPosition = incident.Position - reference.Position;
        double relAngle = incident.Angle - reference.Angle;
        PhysicsCollision? collision1 = IntersectsWith(collider, relPosition, relAngle);
        PhysicsCollision? collision2 = collider.IntersectsWith(this, -relPosition, -relAngle);
        if (collision1 == null || collision2 == null) return;
        PhysicsCollision c1 = (PhysicsCollision)collision1;
        PhysicsCollision c2 = (PhysicsCollision)collision2;

        if (c1.PenetrationVector.Magnitude() == 0) return;
        SD_Vector2 cNormal = -c1.PenetrationVector.Normalize();
        
        // TODO: account for multiple points of collision (the manifold) and subsequently calculate the collision point to use 
        SD_Vector2 cPr = SD_Vector2.Zero;
        foreach (var collisionPoint in c1.CollisionManifold)
            cPr = collisionPoint;
        
        SD_Vector2 cPi = SD_Vector2.Zero;
        foreach (var collisionPoint in c2.CollisionManifold)
            cPi = collisionPoint;
        
        // calculation combined linear and angular velocity of collision point
        SD_Vector2 pVr = reference.Velocity - (SD_Vector2)SD_Vector3.Cross(cPr, new(0, 0, reference.AngularVelocity));
        SD_Vector2 pVi = incident.Velocity - (SD_Vector2)SD_Vector3.Cross(cPi, new(0, 0, incident.AngularVelocity));
        SD_Vector2 relV = pVi - pVr;
        
        // calculate the magnitude of impulse
        ScientificDecimal jV = -(1 + c1.Restitution) * SD_Vector2.Dot(relV, cNormal);
        SD_Vector3 m1 = SD_Vector3.Cross(SD_Vector2.Cross(cPr, cNormal) / c1.Reference.Inertia, cPr);
        SD_Vector3 m2 = SD_Vector3.Cross(SD_Vector2.Cross(cPi, cNormal) / c2.Reference.Inertia, cPi);
        ScientificDecimal j = jV / (SD_Vector2.Dot(cNormal, cNormal * (1 / reference.Mass + 1 / incident.Mass)) 
                                    + SD_Vector3.Dot(m1 + m2, cNormal));
        
        if (!Fixed) reference.Velocity -= cNormal * (j / reference.Mass);
        if (!collider.Fixed) incident.Velocity += cNormal * (j / incident.Mass);

        if (!Fixed)reference.AngularVelocity -= (double)(SD_Vector2.Cross(cPr, cNormal * j).Z / c1.Reference.Inertia);
        if (!collider.Fixed) incident.AngularVelocity += (double)(SD_Vector2.Cross(cPi, cNormal * j).Z / c2.Reference.Inertia);

        if (!Fixed && !collider.Fixed)
        {
            reference.Position += c1.PenetrationVector * incident.Mass / (incident.Mass + reference.Mass);
            incident.Position += c2.PenetrationVector * reference.Mass / (incident.Mass + reference.Mass);
        }
        else if (Fixed) incident.Position += c2.PenetrationVector;
        else if (collider.Fixed) reference.Position += c1.PenetrationVector;*/

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