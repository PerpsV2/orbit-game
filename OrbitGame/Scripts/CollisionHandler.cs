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
                tasks.Add(Task.Run(() => resolver(reference, incident)));
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
        SD_Vector2 pVr = reference.Velocity - (SD_Vector2)SD_Vector3.Cross(cPr, new(0, 0, reference.AngularVelocity));
        SD_Vector2 pVi = incident.Velocity - (SD_Vector2)SD_Vector3.Cross(cPi, new(0, 0, incident.AngularVelocity));
        SD_Vector2 relV = pVi - pVr;

        // calculate collision tangent pointing in the direction of movement
        SD_Vector2 cPerpendicularNormal = new SD_Vector2(-cNormal.Y, cNormal.X);
        SD_Vector2 cTangent = cPerpendicularNormal * (SD_Vector2.Dot(-relV, cPerpendicularNormal).Positive ? 1 : -1);
        
        // calculate the magnitude of impulse along the collision normal (j) and tangentially (jF)
        ScientificDecimal jV = -(1 + restitution) * SD_Vector2.Dot(relV, cNormal);
        SD_Vector3 m1 = SD_Vector3.Cross(SD_Vector2.Cross(cPr, cNormal) / referenceCollider.Inertia, cPr);
        SD_Vector3 m2 = SD_Vector3.Cross(SD_Vector2.Cross(cPi, cNormal) / incidentCollider.Inertia, cPi);
        ScientificDecimal j = jV / (SD_Vector2.Dot(cNormal, cNormal * (1 / reference.Mass + 1 / incident.Mass)) 
                                    + SD_Vector3.Dot(m1 + m2, cNormal));
        ScientificDecimal jS = staticFriction * j;
        ScientificDecimal jD = dynamicFriction * j;
        SD_Vector2 jF = SD_Vector2.Dot(relV, cTangent) == 0 || SD_Vector2.Dot(relV * reference.Mass, cTangent) <= jS
                ? cTangent * -SD_Vector2.Dot(relV * reference.Mass, cTangent) : cTangent * jD;

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
        if (!referenceCollider.Fixed) reference.AngularVelocity -= (double)(SD_Vector2.Cross(cPr, cNormal * j).Z / referenceCollider.Inertia);
        if (!incidentCollider.Fixed) incident.AngularVelocity += (double)(SD_Vector2.Cross(cPi, cNormal * j).Z / incidentCollider.Inertia);
        
        // apply projection method to resolve intersection
        if (!referenceCollider.Fixed && !incidentCollider.Fixed)
        {
            reference.Position += c1.PenetrationVector * incident.Mass / (incident.Mass + reference.Mass);
            incident.Position -= c2.PenetrationVector * reference.Mass / (incident.Mass + reference.Mass);
        }
        else if (referenceCollider.Fixed) incident.Position += c2.PenetrationVector;
        else if (incidentCollider.Fixed) reference.Position += c1.PenetrationVector;

        if (Options.EnablePhysicsCollisionDebug)
            DrawDebug.Add(() => {
                var g = OrbitGame.Graphics;
                var cam = OrbitGame.Camera;
                g.GS_DrawPoint(cam, reference.Position + cPr, Color.Blue);
                g.GS_DrawLineR(cam, reference.Position + cPr, c1.PenetrationVector, Color.Blue);
                g.GS_DrawLineR(cam, reference.Position + cPr, cTangent, Color.Purple);
                foreach (var point in c1.CollisionManifold)
                    g.GS_DrawPoint(cam, reference.Position + point, Color.Red);
                g.GS_DrawLineR(cam, reference.Position + cPr, relV, Color.Yellow);
                g.GS_DrawLineR(cam, reference.Position + cPr, cNormal * (j / reference.Mass), Color.Orange);
            });
    }

    public static void RestShipPlanetCollision(Body reference, Body incident, 
        ScientificDecimal timeStep, ScientificDecimal deltaTime)
    {
        Ship ship = reference as Ship ?? throw new NullReferenceException();
        ResolvePhysicsCollision(reference, incident);
        if (reference.Collider.IntersectsWith(incident.Collider, reference.SpatialInfo, incident.SpatialInfo) !=
            null)
        {
            ScientificDecimal deltaTimeStep = timeStep * deltaTime;
            ScientificDecimal relSpeed = (incident.Velocity - reference.Velocity).Magnitude();
            ScientificDecimal relAngularSpeed = Math.Abs(incident.AngularVelocity - reference.AngularVelocity);
            if (relSpeed > Options.MinimumShipCrashSpeed && ship.LandingState == null)
            {
                ship.Destroy();
                return;
            }

            ResolvePhysicsCollision(reference, incident);
            
            if (relSpeed * deltaTimeStep < Options.MaximumShipRestingSpeed &&
                relAngularSpeed * deltaTimeStep < Options.MaximumShipRestingAngularSpeed)
                ship.SetLandingState(incident);
        }
    }
}