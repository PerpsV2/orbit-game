using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Delegate for calculating instant acceleration given other spatial info.
/// </summary>
public delegate SD_Vector2 CalculateAccelerationMethod();

/// <summary>
/// A KinematicObject with drawing and physics information.
/// </summary>
public abstract class Body : KinematicObject, IGameDrawable
{
    public ScientificDecimal Mass;
    public Color Colour;
    public Body? Parent;
    public KeplerOrbit? Orbit;

    private ObjectInfo _objectInfo;
    
    public IMesh Mesh => _objectInfo.Mesh;
    public CompactCollider Collider => _objectInfo.Collider;
    public Material Material => _objectInfo.Material;

    private IEnumerable<Body> _attractors = OrbitGame.Bodies;
    
    protected Body(
        string identifier, 
        SpatialInfo spatialInfo,
        ObjectInfo objectInfo,
        ScientificDecimal mass,
        Color colour,
        Body? parent)
        : base(identifier, spatialInfo)
    {
        Mass = mass;
        Colour = colour;
        Parent = parent;
        _objectInfo = objectInfo;
        Position = spatialInfo.Position + (parent?.Position ?? SD_Vector2.Zero);
        Velocity = spatialInfo.Velocity + (parent?.Velocity ?? SD_Vector2.Zero);
        Orbit = CalculateOrbit(Parent, true);
    }
    
    public abstract void Draw(GraphicsDevice graphicsDevice, Camera camera);
    public abstract void DrawCollider(GraphicsDevice graphicsDevice, Camera camera);

    /// <summary>
    /// Draws a conical section orbit of an object around a parent using the Laplace-Runge-Lenz vector.
    /// </summary>
    protected void DrawOrbitalPathLRL(GraphicsDevice graphicsDevice, Camera camera, OrbitMesh orbitMesh)
    {
        if (Orbit == null || Parent == null) return;
        KeplerOrbit orbit = (KeplerOrbit)Orbit;
        Body centralForce = Parent;
        List<SD_Vector2> orbitPoints = new List<SD_Vector2>();
        
        // find approximate angle of the orbit covered by the camera
        SD_Vector2 relCamPosition = camera.Position - centralForce.Position;
        double minAngle = 0;
        double maxAngle = Math.Tau;
        if (relCamPosition != SD_Vector2.Zero)
        {
            SD_Vector2 maxCamExtentVector = (SD_Vector2)SD_Vector3.Cross(relCamPosition.Normalize(),
                new(0, 0, ScientificDecimal.Max(camera.Width, camera.Height)));
            minAngle = Utils.WrapAngle((relCamPosition + maxCamExtentVector).GetPrincipalAngle());
            maxAngle = Utils.WrapAngle((relCamPosition - maxCamExtentVector).GetPrincipalAngle());
        }
        if (minAngle > maxAngle) maxAngle += Math.Tau;
        
        // redistribute angles between 0 and tau to be biased towards pi (argument of apoapsis)
        double EllipseBiasFunction(double angle, double exponent)
        {
            angle = Utils.WrapAngle(angle);
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
                Vector2 screenPosition = camera.ConvertToScreenCoordinates(orbit.Center.Value + Parent.Position);
                float screenMajorRadius = camera.ConvertToScreenDistance(semiMajorAxis);
                float screenMinorRadius = camera.ConvertToScreenDistance(semiMinorAxis);
        
                Matrix transform = Matrix.CreateScale(new Vector3(screenMajorRadius, screenMinorRadius, 1)) *
                                   Matrix.CreateRotationZ((float)(orbit.Periapsis + camera.Angle)) *
                                   Matrix.CreateScale(new Vector3(1, -1, 0)) *
                                   Matrix.CreateTranslation(new Vector3(screenPosition.X, screenPosition.Y, 0));
                orbitMesh.Draw(graphicsDevice, transform, new() {
                    {"colour", Colour.ToVector4()}
                });
            }
        }
        // draw parabolic and hyperbolic orbits
        else
        {
            ScientificDecimal? parentSOIRadius = centralForce.Orbit?.SphereOfInfluenceRadius ?? null;
            double asymptoteAngle = Utils.WrapAngle(Math.Acos(-(1 / orbit.Eccentricity)));
            double objectAngle = Utils.WrapAngle((Position - centralForce.Position).GetPrincipalAngle());
            for (double a = -asymptoteAngle; a < asymptoteAngle; a += 2 * asymptoteAngle / Options.OrbitResolutionNumPoints)
            {
                double trueAngle = Utils.WrapAngle(a + orbit.Periapsis);
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

        for (int i = 0; i < orbitPoints.Count - 1; ++i)
        {
            graphicsDevice.GS_DrawLine(camera, orbitPoints[i], orbitPoints[i + 1], Colour);
        }
    }
    
    /// <summary>
    /// Calculate the gravitational acceleration caused by the attraction of one other body.
    /// </summary>
    private SD_Vector2 CalculateGravitationalAcceleration(Body attractor)
    {
        double angle = SD_Vector2.GetPrincipalAngle(Position, attractor.Position);
        SD_Vector2 direction = SD_Vector2.FromPolar(angle);
        ScientificDecimal magnitude = Constants.G * attractor.Mass / (Position - attractor.Position).MagnitudeSquared();
        return direction * magnitude;
    }

    /// <summary>
    /// Calculate the net gravitational acceleration with all bodies in the scene.
    /// </summary>
    private SD_Vector2 CalculateNetGravitationalAcceleration(IEnumerable<Body> attractors)
    {
        SD_Vector2 result = SD_Vector2.Zero;
        return attractors
            .Where(x => x != this)
            .Aggregate(result, (sum, next) => 
                sum + CalculateGravitationalAcceleration(next));
    }
    
    /// <summary>
    /// Calculate the net acceleration from gravity and other sources.
    /// </summary>
    public virtual SD_Vector2 CalculateNetAcceleration()
    {
        return CalculateNetGravitationalAcceleration(_attractors);
    }
    
    /// <summary>
    /// Calculate the Keplerian orbit around a central force with the option to calculate certain orbital initials
    /// </summary>
    protected KeplerOrbit? CalculateOrbit(Body? centralForce, bool initials)
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

        double periapsis = Utils.WrapAngle(Math.PI - lrlVector.GetPrincipalAngle());
        ScientificDecimal eccentricity = lrlVector.Magnitude() / (Mass * forceStrength).Abs();
        ScientificDecimal semiLatusRectum = angularMomentum.Magnitude().Square() / Mass / forceStrength;

        if (semiLatusRectum == 0) return null;

        return new KeplerOrbit(this, centralForce, (double)eccentricity, periapsis, semiLatusRectum, initials);
    }

    public void UpdatePosition_PreservingIntegrator(
        ScientificDecimal initialTimeStep,
        ScientificDecimal endTime)
    {
        ScientificDecimal h = initialTimeStep;
        SD_Vector2 q = Position;
        SD_Vector2 p = Velocity * Mass;
        ScientificDecimal m = Mass;

        ScientificDecimal s = SD_Vector2.Dot(q * h, p) / (q.Magnitude() * m);
    }

    public void UpdatePosition_Integrator(
        ScientificDecimal timeStep, 
        NumericalIntegrator integrator, 
        CalculateAccelerationMethod calculateAcceleration)
    {
        timeStep /= Options.IntegratorIterationAmount;
        for (int i = 0; i < Options.IntegratorIterationAmount; ++i)
        {
            switch (integrator)
            {
                case NumericalIntegrator.ExplicitEuler:
                    Acceleration = calculateAcceleration();
                    Velocity += Acceleration * timeStep;
                    Position += Velocity * timeStep;
                    Angle += AngularVelocity * (double)timeStep;
                    break;
                case NumericalIntegrator.ImplicitEuler:
                    Acceleration = calculateAcceleration();
                    Position += Velocity * timeStep;
                    Velocity += Acceleration * timeStep;
                    Angle += AngularVelocity * (double)timeStep;
                    break;
                case NumericalIntegrator.VelocityVerlet:
                    SD_Vector2 acceleration1 = calculateAcceleration();
                    Position += Velocity * timeStep + acceleration1 * 0.5f * timeStep * timeStep;
                    SD_Vector2 acceleration2 = calculateAcceleration();
                    Velocity += (acceleration1 + acceleration2) * 0.5f * timeStep;
                    Angle += AngularVelocity * (double)timeStep;
                    break;
                case NumericalIntegrator.RungeKutta4:
                    SD_Vector2 originalPosition = Position;
                    SD_Vector2 originalVelocity = Velocity;

                    Acceleration = calculateAcceleration();
                    SD_Vector2 originalAcceleration = Acceleration;
                    SD_Vector2 velocityK1 = originalAcceleration * timeStep;
                    SD_Vector2 positionK1 = Velocity * timeStep;

                    Position = originalPosition + positionK1 * 0.5f;
                    Velocity = originalVelocity + velocityK1 * 0.5f;

                    Acceleration = calculateAcceleration();
                    SD_Vector2 velocityK2 = Acceleration * timeStep;
                    SD_Vector2 positionK2 = Velocity * timeStep;

                    Position = originalPosition + positionK2 * 0.5f;
                    Velocity = originalVelocity + velocityK2 * 0.5f;

                    Acceleration = calculateAcceleration();
                    SD_Vector2 velocityK3 = Acceleration * timeStep;
                    SD_Vector2 positionK3 = Velocity * timeStep;

                    Position = originalPosition + positionK3;
                    Velocity = originalVelocity + velocityK3;

                    Acceleration = calculateAcceleration();
                    SD_Vector2 velocityK4 = Acceleration * timeStep;
                    SD_Vector2 positionK4 = Velocity * timeStep;

                    Velocity = originalVelocity +
                               (velocityK1 + velocityK2 * 2 + velocityK3 * 2 + velocityK4) * (1f / 6f);
                    Position = originalPosition +
                               (positionK1 + positionK2 * 2 + positionK3 * 2 + positionK4) * (1f / 6f);

                    Angle += AngularVelocity * (double)timeStep;
                    break;
            }
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

    public void UpdatePosition_Kepler(ScientificDecimal totalTime, ScientificDecimal timeDiff)
    {
        if (Orbit == null || Parent == null) return;
        KeplerOrbit orbit = Orbit.Value;
        
        if (orbit.InitialTime == null) return;
        ScientificDecimal initialTime = orbit.InitialTime.Value;
        
        SD_Vector2 lastPosition = Position;
        
        double meanAnomaly = (double)(Math.Tau / orbit.Period * (totalTime + initialTime)) + orbit.Periapsis;
        double eccentricAnomaly = CalculateEccentricAnomaly(meanAnomaly - orbit.Periapsis) + orbit.Periapsis;
        double trueAnomaly = CalculateTrueAnomaly(eccentricAnomaly - orbit.Periapsis) + orbit.Periapsis;
        Position = Parent.Position + SD_Vector2.FromPolar(trueAnomaly, orbit.Equation(trueAnomaly));
        Velocity = (Position - lastPosition) / timeDiff;
    }
}