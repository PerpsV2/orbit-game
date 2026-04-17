using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace OrbitGame;
using CollisionBehaviours = Dictionary<(Type referenceType, Type incidentType), ResolveCollisionMethod>;

/// <summary>
/// Collision resolution method between two colliding bodies.
/// </summary>
public delegate void ResolveCollisionMethod(Body reference, Body incident);

/// <summary>
/// Handler class to detect and resolve collisions between objects in the game scene.
/// </summary>
/// <param name="bodies">List of bodies in the scene that have collisions enabled</param>
/// <param name="collisionBehaviours">List of collision resolution methods that should be used between types of objects</param>
public class CollisionHandler(IReadOnlyList<Body> bodies, CollisionBehaviours collisionBehaviours)
{
    public void ResolveCollisions()
    {
        TraverseCollisions(ResolveCollision);
    }

    private void TraverseCollisions(Action<Body, Body> resolver)
    {
        List<Task> tasks = new List<Task>();
        foreach (var reference in bodies)
        foreach (var incident in bodies)
        {
            tasks.Add(Task.Run(() =>
            {
                if (reference != incident) resolver(reference, incident);
            }));
        }
        Task.WaitAll(tasks.ToArray());
    }
    
    private void ResolveCollision(Body reference, Body incident)
    {
        collisionBehaviours.GetValueOrDefault((reference.GetType(), incident.GetType()))?.Invoke(reference, incident);
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
        Vec2<SDecimal> jF = Vec2<SDecimal>.Dot(relV, cTangent) == 0 || 
                                Vec2<SDecimal>.Dot(relV * reference.Mass, cTangent) <= jS
                ? cTangent * -Vec2<SDecimal>.Dot(relV * reference.Mass, cTangent) : cTangent * jD;

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
        if (!referenceCollider.Fixed) reference.AngularVelocity -= 
            (double)(Vec2<SDecimal>.Cross(cPr, cNormal * j).Z / referenceCollider.Inertia);
        if (!incidentCollider.Fixed) incident.AngularVelocity += 
            (double)(Vec2<SDecimal>.Cross(cPi, cNormal * j).Z / incidentCollider.Inertia);
        // apply projection method to resolve intersection
        
        if (!referenceCollider.Fixed && !incidentCollider.Fixed)
        {
            reference.Position += c1.PenetrationVector * incident.Mass / (incident.Mass + reference.Mass);
            incident.Position += c2.PenetrationVector * reference.Mass / (incident.Mass + reference.Mass);
        }
        else if (referenceCollider.Fixed) incident.Position += c2.PenetrationVector;
        else if (incidentCollider.Fixed) reference.Position += c1.PenetrationVector;

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
        double shipPlanetAngle = (reference.Position - planet.Position).Direction();
        if ((reference.Position - planet.Position).MagnitudeSquared() <
            SDecimal.Square(planet.GetElevationAtPoint(shipPlanetAngle) + planet.Radius))
        {
            reference.Velocity *= -1;
        }
    }

    public static void RestShipPlanetCollision(Body reference, Body incident, 
        SDecimal timeStep, SDecimal deltaTime)
    {
        Ship ship = reference as Ship ?? throw new NullReferenceException();
        ResolvePhysicsCollision(reference, incident);
        if (reference.Collider.IntersectsWith(incident.Collider, reference.SpatialInfo, incident.SpatialInfo) !=
            null)
        {
            SDecimal deltaTimeStep = timeStep * deltaTime;
            SDecimal relSpeed = (incident.Velocity - reference.Velocity).Magnitude();
            SDecimal relAngularSpeed = Math.Abs(incident.AngularVelocity - reference.AngularVelocity);
            // if (relSpeed > Options.MinimumShipCrashSpeed && ship.LandingState == null)
            // {
            //     KinematicObject.KinematicObjectTemplate.DestroyGlobal(ship.Identifier);
            //     return;
            // }

            ResolvePhysicsCollision(reference, incident);
            
            if (relSpeed * deltaTimeStep < Options.MaximumShipRestingSpeed &&
                relAngularSpeed * deltaTimeStep < Options.MaximumShipRestingAngularSpeed)
                ship.SetLandingState(incident);
        }
    }
}