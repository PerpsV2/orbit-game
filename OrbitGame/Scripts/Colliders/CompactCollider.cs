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
    protected ScientificDecimal Inertia;
        
    public bool Fixed;

    public abstract void CalculateInertia(ScientificDecimal mass);
    
    /// <summary>
    /// Method to return the axis-aligned rectangular collider which best fits the set of points in the collider
    /// </summary>
    protected abstract BoundingBox GetBoundingBox(double angle);
    
    public bool NearsWith(CompactCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial)
    {
        return GetBoundingBox(referenceSpatial.Angle).IntersectsWith(
            collider.GetBoundingBox(colliderSpatial.Angle), referenceSpatial, colliderSpatial
        );
    }
    
    public abstract PointCollision IntersectsWith(SD_Vector2 point, SpatialInfo referenceSpatial);
    public PhysicsCollision? IntersectsWith(CompactCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial)
    {
        switch (collider)
        {
            case CircularCollider c: return IntersectsWith(c, referenceSpatial, colliderSpatial);
            case ConvexCollider c: return IntersectsWith(c, referenceSpatial, colliderSpatial);
            default: throw new ArgumentException();
        }
    }

    protected abstract PhysicsCollision? IntersectsWith(
        CircularCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial
    );
    protected abstract PhysicsCollision? IntersectsWith(
        ConvexCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial
    );

    public void CollidesWith(CompactCollider collider, KinematicObject reference, KinematicObject incidence)
    {
        PhysicsCollision? collision1 = this.IntersectsWith(collider, reference.SpatialInfo, incidence.SpatialInfo);
        PhysicsCollision? collision2 = collider.IntersectsWith(this, incidence.SpatialInfo, reference.SpatialInfo);
        if (collision1 == null || collision2 == null) return;
        PhysicsCollision c1 = (PhysicsCollision)collision1;
        PhysicsCollision c2 = (PhysicsCollision)collision2;

        if (c1.PenetrationVector.Magnitude() == 0) return;
        SD_Vector2 cNormal = -c1.PenetrationVector.Normalize();
        
        float restitution = (reference.Material.RestitutionCoefficient + reference.Material.RestitutionCoefficient) / 2;
        float staticFriction = (reference.Material.StaticFrictionCoefficient + reference.Material.StaticFrictionCoefficient) / 2;
        float dynamicFriction = (reference.Material.DynamicFrictionCoefficient + reference.Material.DynamicFrictionCoefficient) / 2;
        
        // TODO: account for multiple points of collision (the manifold) and subsequently calculate the collision point to use 
        SD_Vector2 cPr = SD_Vector2.Zero;
        foreach (var collisionPoint in c1.CollisionManifold)
            cPr = collisionPoint;
        
        SD_Vector2 cPi = SD_Vector2.Zero;
        foreach (var collisionPoint in c2.CollisionManifold)
            cPi = collisionPoint;
        
        // calculation combined linear and angular velocity of collision point
        SD_Vector2 pVr = reference.Velocity - (SD_Vector2)SD_Vector3.Cross(cPr, 
            new(0, 0, reference.AngularVelocity));
        SD_Vector2 pVi = incidence.Velocity - (SD_Vector2)SD_Vector3.Cross(cPi, 
            new(0, 0, incidence.AngularVelocity));
        SD_Vector2 relV = pVi - pVr;

        SD_Vector2 cPerpendicularNormal = new SD_Vector2(-cNormal.Y, cNormal.X);
        SD_Vector2 cTangent = cPerpendicularNormal * (SD_Vector2.Dot(-relV, cPerpendicularNormal).Positive ? 1 : -1);
        
        // calculate the magnitude of impulse
        ScientificDecimal jV = -(1 + restitution) * SD_Vector2.Dot(relV, cNormal);
        SD_Vector3 m1 = SD_Vector3.Cross(SD_Vector2.Cross(cPr, cNormal) / Inertia, cPr);
        SD_Vector3 m2 = SD_Vector3.Cross(SD_Vector2.Cross(cPi, cNormal) / collider.Inertia, cPi);
        ScientificDecimal j = jV / (SD_Vector2.Dot(cNormal, cNormal * (1 / reference.Mass + 1 / incidence.Mass)) 
                                    + SD_Vector3.Dot(m1 + m2, cNormal));
        ScientificDecimal jS = staticFriction * j;
        ScientificDecimal jD = dynamicFriction * j;
        SD_Vector2 jF = SD_Vector2.Dot(relV, cTangent) == 0 || SD_Vector2.Dot(relV * reference.Mass, cTangent) <= jS
                ? cTangent * -SD_Vector2.Dot(relV * reference.Mass, cTangent)
                : cTangent * jD;

        if (!Fixed)
        {
            reference.Velocity -= cNormal * (j / reference.Mass);
            reference.Velocity -= jF / reference.Mass;
        }

        if (!collider.Fixed)
        {
            incidence.Velocity += cNormal * (j / incidence.Mass);
            incidence.Velocity += jF / incidence.Mass;
        }
        
        if (!Fixed)reference.AngularVelocity -= (double)(SD_Vector2.Cross(cPr, cNormal * j).Z / Inertia);
        if (!collider.Fixed) incidence.AngularVelocity += (double)(SD_Vector2.Cross(cPi, cNormal * j).Z / collider.Inertia);
        
        if (!Fixed && !collider.Fixed)
        {
            reference.Position += c1.PenetrationVector * incidence.Mass / (incidence.Mass + reference.Mass);
            incidence.Position -= c2.PenetrationVector * reference.Mass / (incidence.Mass + reference.Mass);
        }
        else if (Fixed) incidence.Position += c2.PenetrationVector;
        else if (collider.Fixed) reference.Position += c1.PenetrationVector;

        if (Options.EnableCollisionDebug)
            DrawDebug.Add((g, cam) => {
                g.GS_DrawPoint(cam, reference.Position + cPr, DrawDebug.Blue);
                g.GS_DrawLineR(cam, reference.Position + cPr, c1.PenetrationVector, DrawDebug.Blue);
                g.GS_DrawLineR(cam, reference.Position + cPr, cTangent, DrawDebug.Purple);
                foreach (var point in c1.CollisionManifold)
                    g.GS_DrawPoint(cam, reference.Position + point, DrawDebug.Red);
                g.GS_DrawLineR(cam, reference.Position + cPr, relV, DrawDebug.Yellow);
                g.GS_DrawLineR(cam, reference.Position + cPr, cNormal * (j / reference.Mass), DrawDebug.Orange);
            });
    }
    
    
    public abstract bool IsEmpty();
}