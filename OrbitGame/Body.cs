using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// A KinematicObject with information about shape, trajectory, material and methods for physics.
/// </summary>
public abstract class Body : KinematicObject, IGameDrawable
{
    public Color Colour;
    public Body? Parent;
    public KeplerOrbit? Orbit;
    
    protected Body(
        KinematicObjectTemplate template,
        string identifier, 
        ScientificDecimal mass, 
        SD_Vector2 position,
        SD_Vector2 velocity,
        Color colour,
        Body? parent,
        IMesh? mesh = null,
        CompactCollider? collider = null,
        Material? material = null
        ) 
        : base(template, identifier, mass, position, velocity, mesh, collider, material)
    {
        Colour = colour;
        Parent = parent;
        Position = position + (parent?.Position ?? SD_Vector2.Zero);
        Velocity = velocity + (parent?.Velocity ?? SD_Vector2.Zero);
        Orbit = CalculateOrbit(true);
    }
    
    public abstract void Draw(GraphicsDevice graphicsDevice, Camera camera, Effect effect);
    public abstract void DrawCollider(GraphicsDevice graphicsDevice, Camera camera, Effect effect);

    /// <summary>
    /// Draws a conical section orbit of an object around a parent using the Laplace-Runge-Lenz vector.
    /// </summary>
    public void DrawOrbitalPathLRL(GraphicsDevice graphicsDevice, Camera camera, Effect effect, OrbitMesh orbitMesh)
    {
        if (Orbit == null || Parent == null) return;
        KeplerOrbit orbit = (KeplerOrbit)Orbit;
        Body centralForce = Parent;
        List<SD_Vector2> orbitPoints = new List<SD_Vector2>();
        
        // find approximate angle of the orbit covered by the camera
        SD_Vector2 relCamPosition = camera.AbsolutePosition - centralForce.Position;
        double minAngle = 0;
        double maxAngle = Math.Tau;
        if (relCamPosition != SD_Vector2.Zero)
        {
            SD_Vector2 maxCamExtentVector = (SD_Vector2)SD_Vector3.Cross(relCamPosition.Normalize(),
                new(0, 0, ScientificDecimal.Max(camera.Width, camera.Height)));
            minAngle = Utils.UnsignedMod((relCamPosition + maxCamExtentVector).GetPrincipalAngle(), Math.Tau);
            maxAngle = Utils.UnsignedMod((relCamPosition - maxCamExtentVector).GetPrincipalAngle(), Math.Tau);
        }
        if (minAngle > maxAngle) maxAngle += Math.Tau;
        
        // redistribute angles between 0 and tau to be biased towards pi (argument of apoapsis)
        double EllipseBiasFunction(double angle, double exponent)
        {
            angle = Utils.UnsignedMod(angle, Math.Tau);
            double result = Math.PI - Math.PI * Math.Pow(1 - angle / Math.PI, exponent);
            if (angle > Math.PI) result = Math.PI + Math.PI * Math.Pow(angle / Math.PI - 1, exponent);
            return result;
        }
        
        double exponent = Options.EllipsePointDistributionBiasStrength * 
            Math.Pow(orbit.Eccentricity, 1 - orbit.Eccentricity) + 1;
        
        // draw circular and elliptical orbits
        if (orbit.Eccentricity < 1)
        {
            ScientificDecimal semiMajorAxis = orbit.SemiMajorAxis;
            ScientificDecimal semiMinorAxis = orbit.SemiMinorAxis;
            
            // orbit is too small to draw
            if (camera.ConvertToScreenDistance(semiMajorAxis) < 1) return;
            // draw partial orbit if camera is zoomed in.
            if (maxAngle - minAngle < Options.OrbitApproximationZoomFraction * Math.PI)
            {
                // apply inverse ellipse bias function of camera angle limits
                double unbiasedMinAngle = EllipseBiasFunction(minAngle - orbit.Periapsis, 1 / exponent);
                double unbiasedMaxAngle = EllipseBiasFunction(maxAngle - orbit.Periapsis, 1 / exponent);
                // sweep through angle range and re-apply bias function on each point then draw the orbit
                for (double a = unbiasedMinAngle; a < unbiasedMaxAngle; 
                     a += (unbiasedMaxAngle - unbiasedMinAngle) / Options.OrbitResolutionNumPoints)
                {
                    double trueAngle = EllipseBiasFunction(a, exponent) + orbit.Periapsis;
                    ScientificDecimal dist = orbit.Equation(trueAngle);
                    orbitPoints.Add(centralForce.Position + SD_Vector2.FromPolar(trueAngle, dist));
                }
            }
            // draw the entire orbit as an ellipse
            else
            {
                if (orbit.Center == null) 
                    throw new NullReferenceException("Elliptic orbit must have a center.");
                Vector2 screenPosition = camera.ConvertToScreenCoordinates(orbit.Center.Value);
                float screenMajorRadius = camera.ConvertToScreenDistance(orbit.SemiMajorAxis);
                float screenMinorRadius = camera.ConvertToScreenDistance(orbit.SemiMinorAxis);
        
                Matrix transform = Matrix.CreateScale(new Vector3(screenMajorRadius, screenMinorRadius, 1)) *
                                   Matrix.CreateRotationZ(-(float)orbit.Periapsis) *
                                   Matrix.CreateTranslation(new Vector3(screenPosition.X, screenPosition.Y, 0));
                orbitMesh.Draw(graphicsDevice, effect, transform, new() {
                    {"colour", Colour.ToVector4()}
                });
            }
        }
        // draw parabolic and hyperbolic orbits
        else
        {
            ScientificDecimal? parentSOIRadius = centralForce.Orbit?.SphereOfInfluenceRadius ?? null;
            double asymptoteAngle = Utils.UnsignedMod(Math.Acos(-(1 / orbit.Eccentricity)), Math.Tau);
            double objectAngle = Utils.UnsignedMod((Position - centralForce.Position).GetPrincipalAngle(), Math.Tau);
            for (double a = -asymptoteAngle; a < asymptoteAngle; a += 2 * asymptoteAngle / Options.OrbitResolutionNumPoints)
            {
                double trueAngle = Utils.UnsignedMod(a + orbit.Periapsis, Math.Tau);
                ScientificDecimal dist = orbit.Equation(trueAngle);
                SD_Vector2 orbitPoint = centralForce.Position + SD_Vector2.FromPolar(trueAngle, dist);
                if (parentSOIRadius != null)
                {
                    if (dist > 0 && dist < parentSOIRadius)
                        orbitPoints.Add(orbitPoint);
                }
                else if (dist > 0 && !dist.IsInfinite) 
                    orbitPoints.Add(orbitPoint);
                if (double.IsPositive(trueAngle - objectAngle) !=  
                    double.IsPositive(trueAngle + 2 * asymptoteAngle / Options.OrbitResolutionNumPoints - objectAngle))
                    orbitPoints.Add(centralForce.Position + SD_Vector2.FromPolar(objectAngle, orbit.Equation(objectAngle)));
            }

            if (parentSOIRadius != null)
            {
                double escapeAngle = Math.Acos((double)((parentSOIRadius / orbit.SemiLatusRectum - 1) /
                                                        (orbit.Eccentricity * parentSOIRadius /
                                                         orbit.SemiLatusRectum))) + Math.PI;
                orbitPoints.Add(centralForce.Position + SD_Vector2.FromPolar(-escapeAngle + orbit.Periapsis, parentSOIRadius.Value));
                orbitPoints.Insert(0, centralForce.Position + SD_Vector2.FromPolar(escapeAngle + orbit.Periapsis, parentSOIRadius.Value));
            }
        }

        //if (orbitPoints.Count > 0) Utils.GS_DrawPath(graphicsDevice, camera, effect, orbitPoints, Colour);
    }
    
    private SD_Vector2 CalculateGravitationalAcceleration(Body attractor)
    {
        double angle = SD_Vector2.GetPrincipalAngle(Position, attractor.Position);
        SD_Vector2 direction = SD_Vector2.FromPolar(angle);
        ScientificDecimal magnitude = Constants.G * attractor.Mass / (Position - attractor.Position).MagnitudeSquared();
        return direction * magnitude;
    }

    public SD_Vector2 SetNetGravitationalAcceleration(IEnumerable<Body> attractors)
    {
        SD_Vector2 result = SD_Vector2.Zero;
        return Acceleration = attractors
            .Where(x => x != this)
            .Aggregate(result, (sum, next) => 
                sum + CalculateGravitationalAcceleration(next));
    }
    
    private KeplerOrbit? CalculateOrbit(Body? centralForce, bool initials)
    {
        if (centralForce == null) return null;
        
        SD_Vector2 relVelocity = Velocity - centralForce.Velocity;
        SD_Vector2 relPosition = Position - centralForce.Position;
        
        SD_Vector2 momentum = relVelocity * Mass;
        SD_Vector3 angularMomentum = SD_Vector2.Cross(relPosition, momentum);
        SD_Vector2 directionVector = relPosition.Normalize();
        ScientificDecimal forceStrength = Mass * centralForce.Mass * Constants.G;

        SD_Vector2 lrlVector = Matrix3X3.Scale(-1, 1) * ((SD_Vector2)SD_Vector3.Cross(momentum, angularMomentum) -
                            directionVector * Mass * forceStrength);
        if (lrlVector == SD_Vector2.Zero) return null;

        double periapsis = Utils.UnsignedMod(Math.PI - lrlVector.GetPrincipalAngle(), Math.Tau);
        ScientificDecimal c = Mass * forceStrength / angularMomentum.Magnitude().Square();
        ScientificDecimal eccentricity = lrlVector.Magnitude() / (Mass * forceStrength).Abs();
        ScientificDecimal semiLatusRectum = 1 / c;

        if (semiLatusRectum == 0) return null;

        return new KeplerOrbit(this, centralForce, (double)eccentricity, periapsis, semiLatusRectum, initials);
    }

    protected KeplerOrbit? CalculateOrbit(bool initials)
        => CalculateOrbit(Parent, initials);

    public void NI_UpdatePosition(ScientificDecimal timeStep, NumericalIntegrator integrator, Action<Body> updateAcceleration)
    {
        switch (integrator)
        {
            case NumericalIntegrator.ExplicitEuler:
                Velocity += Acceleration * timeStep;
                Position += Velocity * timeStep;
                Angle += AngularVelocity * (double)timeStep;
                break;
            case NumericalIntegrator.ImplicitEuler:
                Position += Velocity * timeStep;
                Velocity += Acceleration * timeStep;
                Angle += AngularVelocity * (double)timeStep;
                break;
            case NumericalIntegrator.RungeKutta4:
                SD_Vector2 originalPosition = Position;
                SD_Vector2 originalVelocity = Velocity;
                
                SD_Vector2 originalAcceleration = Acceleration;
                SD_Vector2 velocityK1 = originalAcceleration * timeStep;
                SD_Vector2 positionK1 = Velocity * timeStep;

                Position = originalPosition + positionK1 * 0.5f;
                Velocity = originalVelocity + velocityK1 * 0.5f;
                
                updateAcceleration(this);
                SD_Vector2 velocityK2 = Acceleration * timeStep;
                SD_Vector2 positionK2 = Velocity * timeStep;

                Position = originalPosition + positionK2 * 0.5f;
                Velocity = originalVelocity + velocityK2 * 0.5f;
                
                updateAcceleration(this);
                SD_Vector2 velocityK3 = Acceleration * timeStep;
                SD_Vector2 positionK3 = Velocity * timeStep;

                Position = originalPosition + positionK3;
                Velocity = originalVelocity + velocityK3;
                
                updateAcceleration(this);
                SD_Vector2 velocityK4 = Acceleration * timeStep;
                SD_Vector2 positionK4 = Velocity * timeStep;

                Velocity = originalVelocity + (velocityK1 + velocityK2 * 2 + velocityK3 * 2 + velocityK4) * (1f / 6f);
                Position = originalPosition + (positionK1 + positionK2 * 2 + positionK3 * 2 + positionK4) * (1f / 6f);
                
                Angle += AngularVelocity * (double)timeStep;
                break;
        }
    }

    private double CalculateEccentricAnomaly(double meanAnomaly)
    {
        if (Orbit == null) throw new NullReferenceException("Orbit cannot be null.");
        KeplerOrbit orbit = (KeplerOrbit)Orbit;
        ScientificDecimal epsilon = new ScientificDecimal(1, -35);
        double eccentricAnomaly = meanAnomaly;
        int iterations = 0;
        while (double.Abs(eccentricAnomaly - orbit.Eccentricity * Math.Sin(eccentricAnomaly) - meanAnomaly) > epsilon)
        {
            if (iterations > 100) return eccentricAnomaly;
            eccentricAnomaly -= (eccentricAnomaly - orbit.Eccentricity * Math.Sin(eccentricAnomaly) - meanAnomaly) /
                                (1 - orbit.Eccentricity * Math.Cos(eccentricAnomaly));
            iterations++;
        }
        return eccentricAnomaly;
    }

    private double CalculateTrueAnomaly(double eccentricAnomaly)
    {
        if (Orbit == null) throw new NullReferenceException("Orbit cannot be null.");
        KeplerOrbit orbit = (KeplerOrbit)Orbit;
        return 2 * Math.Atan2(Math.Sqrt(1 + orbit.Eccentricity) * Math.Sin(eccentricAnomaly / 2), 
            Math.Sqrt(1 - orbit.Eccentricity) * Math.Cos(eccentricAnomaly / 2));
    }

    public void Kepler_UpdatePosition(ScientificDecimal totalTime)
    {
        if (Orbit == null) return;
        if (Parent == null) return;
        KeplerOrbit orbit = (KeplerOrbit)Orbit;
        if (orbit.Center == null) return;
        SD_Vector2 center = (SD_Vector2)orbit.Center;
        
        double meanAnomaly = (double)(Math.Tau / orbit.Period * (totalTime + orbit.InitialTime!)) + orbit.Periapsis;
        double eccentricAnomaly = CalculateEccentricAnomaly(meanAnomaly - orbit.Periapsis) + orbit.Periapsis;
        double trueAnomaly = CalculateTrueAnomaly(eccentricAnomaly - orbit.Periapsis) + orbit.Periapsis;
        Position = Parent.Position + SD_Vector2.FromPolar(trueAnomaly, orbit.Equation(trueAnomaly));
        
        ScientificDecimal relDist = (Position - Parent.Position).Magnitude();
        ScientificDecimal relSpeed = (Constants.G * Parent.Mass * (2 / relDist - 1 / orbit.SemiMajorAxis)).Sqrt();
        SD_Vector2 relVelocity = SD_Vector2.FromPolar(SD_Vector2.DirectionVectorBetween(center, Position).GetPrincipalAngle() + 
                                                Math.PI / 2, relSpeed);
        Velocity = Parent.Velocity + relVelocity;
    }
}