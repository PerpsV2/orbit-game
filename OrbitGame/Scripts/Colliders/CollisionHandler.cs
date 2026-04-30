using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace OrbitGame;

/// <summary>
/// Handler class to detect and resolve collisions between objects in the game scene.
/// </summary>
public class CollisionHandler
{
    public void ResolveCollisions()
    {
        Ship[] ships = OrbitGame.Hierarchy.GetObjectsOfType<Ship>();
        Planet[] planets = OrbitGame.Hierarchy.GetObjectsOfType<Planet>();
        
        List<Task> tasks = new List<Task>();

        SDecimal maxShipRadius = ships.MaxBy(x => x.Collider.MaxRadius)?.Collider.MaxRadius ?? 0;
        foreach (var reference in ships) 
            foreach (var incident in OrbitGame.Hierarchy.GetObjectsInRadius(reference.Position, 
                         reference.Collider.MaxRadius + maxShipRadius).OfType<Ship>())
                tasks.Add(Task.Run(() => { if (incident != reference) ResolvePhysicsCollision(reference, incident); }));
        
        foreach (var reference in ships)
            foreach (var incident in planets)
                tasks.Add(Task.Run(() => { BodyPlanetTerrainCollision(reference, incident); }));
        
        Task.WaitAll(tasks.ToArray());
    }

    public static void ResolveTerrainCollision(Body body, Planet planet)
    {
        ConvexCollider convexCollider = (ConvexCollider)body.Collider;
        CompactCollider terrainCollider = planet.Collider;
        PhysicsCollision? collision = terrainCollider.IntersectsWith(convexCollider, body.SpatialInfo, planet.SpatialInfo);
        if (collision == null) return;
        PhysicsCollision c = (PhysicsCollision)collision;
        
        Vec2Double cPr = c.CollisionManifold.FirstOrDefault();

        // calculation combined linear and angular velocity of collision point
        Vec2<SDecimal> pVr = body.Velocity - (Vec2<SDecimal>)Vec3Double.Cross(cPr, new(0, 0, body.AngularVelocity));
        Vec2<SDecimal> relV = pVr - planet.Velocity;
        
        Vec2<SDecimal> cNormal = c.PenetrationVector.Normalize();
        Vec2<SDecimal> cPerpendicularNormal = new Vec2<SDecimal>(-cNormal.Y, cNormal.X);
        Vec2<SDecimal> cTangent = cPerpendicularNormal * (Vec2<SDecimal>.Dot(-relV, cPerpendicularNormal).Positive ? 1 : -1);
        
        SDecimal jV = -(1 + body.Material.RestitutionCoefficient) * Vec2<SDecimal>.Dot(relV, cNormal);
        Vec3<SDecimal> m1 = Vec3<SDecimal>.Cross(Vec2<SDecimal>.Cross(cPr, cNormal) / body.Collider.Inertia, cPr);
        SDecimal j = jV / (Vec2<SDecimal>.Dot(cNormal, cNormal * (1 / body.Mass)) + Vec3<SDecimal>.Dot(m1, cNormal));
        SDecimal jS = body.Material.StaticFrictionCoefficient * j;
        SDecimal jD = body.Material.DynamicFrictionCoefficient * j;
        Vec2<SDecimal> jF = Vec2<SDecimal>.Dot(relV, cTangent) == 0 && 
                            Vec2<SDecimal>.Dot(relV * body.Mass, cTangent) <= jS
            ? cTangent * -Vec2<SDecimal>.Dot(relV * body.Mass, cTangent) : cTangent * jD;

        if (Options.EnableCollisions)
        {
            body.Velocity += cNormal * j / body.Mass;
            body.Velocity += jF / body.Mass;
            body.AngularVelocity += (double)(Vec2<SDecimal>.Cross(cPr, cNormal * j).Z / body.Collider.Inertia);
            body.Position += c.PenetrationVector;
        }

        if (Options.EnablePhysicsCollisionDebug)
            DrawDebug.Add(() => {
                var g = OrbitGame.Graphics;
                var cam = OrbitGame.Camera;
                g.SD_DrawPoint(cam, body.Position + cPr, Color.Blue);
                g.SD_DrawLineR(cam, body.Position + cPr, c.PenetrationVector, Color.Red);
                g.SD_DrawLineR(cam, body.Position + cPr, cTangent, Color.Purple);
                foreach (var point in c.CollisionManifold)
                    g.SD_DrawPoint(cam, body.Position + point, Color.Red);
                g.SD_DrawLineR(cam, body.Position + cPr, relV, Color.Yellow);
                g.SD_DrawLineR(cam, body.Position + cPr, cNormal * (j / body.Mass), Color.Orange);
            });
    }
    
    public static void ResolvePhysicsCollision(Body reference, Body incident)
    {
        CompactCollider referenceCollider = reference.Collider;
        CompactCollider incidentCollider = incident.Collider;
        PhysicsCollision? collision1 = referenceCollider.IntersectsWith(incidentCollider, reference.SpatialInfo, incident.SpatialInfo);
        PhysicsCollision? collision2 = incidentCollider.IntersectsWith(referenceCollider, incident.SpatialInfo, reference.SpatialInfo);
        if (collision1 == null || collision2 == null) return;
        PhysicsCollision c1 = (PhysicsCollision)collision1;
        PhysicsCollision c2 = (PhysicsCollision)collision2;

        if (c1.PenetrationVector.Magnitude() == 0) return;
        Vec2Double cNormal = -c1.PenetrationVector.Normalize();
        
        float restitution = (reference.Material.RestitutionCoefficient + reference.Material.RestitutionCoefficient) / 2;
        float staticFriction = (reference.Material.StaticFrictionCoefficient + reference.Material.StaticFrictionCoefficient) / 2;
        float dynamicFriction = (reference.Material.DynamicFrictionCoefficient + reference.Material.DynamicFrictionCoefficient) / 2;
        
        // TODO: account for multiple points of collision (the manifold) and subsequently calculate the collision point to use 
        Vec2Double cPr = Vec2Double.Zero;
        foreach (var collisionPoint in c1.CollisionManifold)
            cPr = collisionPoint;
        
        Vec2Double cPi = Vec2Double.Zero;
        foreach (var collisionPoint in c2.CollisionManifold)
            cPi = collisionPoint;
        
        // calculation combined linear and angular velocity of collision point
        Vec2<SDecimal> pVr = reference.Velocity - (Vec2<SDecimal>)Vec3Double.Cross(cPr, new(0, 0, reference.AngularVelocity));
        Vec2<SDecimal> pVi = incident.Velocity - (Vec2<SDecimal>)Vec3Double.Cross(cPi, new(0, 0, incident.AngularVelocity));
        Vec2<SDecimal> relV = pVi - pVr;

        // calculate collision tangent pointing in the direction of movement
        Vec2<SDecimal> cPerpendicularNormal = new Vec2<SDecimal>(-cNormal.Y, cNormal.X);
        Vec2<SDecimal> cTangent = cPerpendicularNormal * 
                                      (Vec2<SDecimal>.Dot(-relV, cPerpendicularNormal).Positive ? 1 : -1);
        
        // calculate the magnitude of impulse along the collision normal (j) and tangentially (jF)
        SDecimal jV = -(1 + restitution) * Vec2<SDecimal>.Dot(relV, cNormal);
        Vec3<SDecimal> m1 = Vec3<SDecimal>.Cross(Vec2<SDecimal>.Cross(cPr, cNormal) / referenceCollider.Inertia, cPr);
        Vec3<SDecimal> m2 = Vec3<SDecimal>.Cross(Vec2<SDecimal>.Cross(cPi, cNormal) / incidentCollider.Inertia, cPi);
        SDecimal j = jV / (Vec2<SDecimal>.Dot(cNormal, (Vec2<SDecimal>)cNormal * (1 / reference.Mass + 1 / incident.Mass)) 
                           + Vec3<SDecimal>.Dot(m1 + m2, (Vec2<SDecimal>)cNormal));
        SDecimal jS = staticFriction * j;
        SDecimal jD = dynamicFriction * j;
        Vec2<SDecimal> jF = Vec2<SDecimal>.Dot(relV, cTangent) == 0 && 
                                Vec2<SDecimal>.Dot(relV * reference.Mass, cTangent) <= jS
                ? cTangent * -Vec2<SDecimal>.Dot(relV * reference.Mass, cTangent) : cTangent * jD;

        if (Options.EnableCollisions)
        {
            // apply linear impulse
            if (!referenceCollider.Fixed)
            {
                reference.Velocity -= cNormal * (j / reference.Mass);
                reference.Velocity -= jF / reference.Mass;
            }

            if (!incidentCollider.Fixed)
            {
                incident.Velocity += cNormal * (j / incident.Mass);
                incident.Velocity += jF / incident.Mass;
            }

            // apply angular impulse
            if (!referenceCollider.Fixed)
                reference.AngularVelocity -=
                    (double)(Vec2<SDecimal>.Cross(cPr, cNormal * j).Z / referenceCollider.Inertia);
            if (!incidentCollider.Fixed)
                incident.AngularVelocity +=
                    (double)(Vec2<SDecimal>.Cross(cPi, cNormal * j).Z / incidentCollider.Inertia);
            // apply projection method to resolve intersection

            if (!referenceCollider.Fixed && !incidentCollider.Fixed)
            {
                reference.Position += c1.PenetrationVector * incident.Mass / (incident.Mass + reference.Mass);
                incident.Position += c2.PenetrationVector * reference.Mass / (incident.Mass + reference.Mass);
            }
            else if (referenceCollider.Fixed) incident.Position += c2.PenetrationVector;
            else if (incidentCollider.Fixed) reference.Position += c1.PenetrationVector;
        }

        if (Options.EnablePhysicsCollisionDebug)
            DrawDebug.Add(() => {
                var g = OrbitGame.Graphics;
                var cam = OrbitGame.Camera;
                g.SD_DrawPoint(cam, reference.Position + cPr, Color.Blue);
                g.SD_DrawLineR(cam, reference.Position + cPr, c1.PenetrationVector, Color.Red);
                g.SD_DrawLineR(cam, reference.Position + cPr, cTangent, Color.Purple);
                foreach (var point in c1.CollisionManifold)
                    g.SD_DrawPoint(cam, reference.Position + point, Color.Red);
                g.SD_DrawLineR(cam, reference.Position + cPr, relV, Color.Yellow);
                g.SD_DrawLineR(cam, reference.Position + cPr, cNormal * (j / reference.Mass), Color.Orange);
            });
    }

    public static void BodyPlanetTerrainCollision(Body reference, Body incident)
    {
        Planet planet = (Planet)incident;
        if (planet.Collider.NearsWith(reference.Collider, reference.SpatialInfo, incident.SpatialInfo))
            ResolveTerrainCollision(reference, planet);
    }

    public static void RestShipPlanetCollision(Body reference, Body incident)
    {
        Ship ship = reference as Ship ?? throw new NullReferenceException();
        ResolvePhysicsCollision(reference, incident);
        if (reference.Collider.IntersectsWith(incident.Collider, reference.SpatialInfo, incident.SpatialInfo) !=
            null)
        {
            SDecimal relSpeed = (incident.Velocity - reference.Velocity).Magnitude();
            SDecimal relAngularSpeed = Math.Abs(incident.AngularVelocity - reference.AngularVelocity);
            if (relSpeed > Options.MinimumShipCrashSpeed && ship.LandingState == null)
            {
                ship.Destroy();
                return;
            }

            ResolvePhysicsCollision(reference, incident);
        }
    }
}