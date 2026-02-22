using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public delegate ScientificDecimal OrbitEquation(double angle);

public readonly record struct KeplerOrbit
{
    public readonly Body Body;
    public readonly Body Parent;
    public readonly double Periapsis;
    public readonly double Eccentricity;
    public readonly ScientificDecimal SemiLatusRectum;
    
    public readonly OrbitEquation Equation;
    public readonly ScientificDecimal SemiMajorAxis;
    public readonly ScientificDecimal SemiMinorAxis;
    public readonly ScientificDecimal Period;
    public readonly SD_Vector2? Center;
    
    public readonly ScientificDecimal? SphereOfInfluenceRadius;
    
    public readonly ScientificDecimal? InitialTime;
    
    public KeplerOrbit(
        Body Body,
        Body Parent,
        double Eccentricity, 
        double Periapsis,
        ScientificDecimal SemiLatusRectum,
        bool initials = false
        )
    {
        this.Body = Body;
        this.Parent = Parent;
        this.Eccentricity = Eccentricity;
        this.SemiLatusRectum = SemiLatusRectum;
        Equation = angle => SemiLatusRectum / (1 + Eccentricity * Math.Cos(angle - Periapsis));
        this.Periapsis = Utils.WrapAngle(Periapsis);
        
        if (Eccentricity == 0)
        {
            SemiMajorAxis = SemiLatusRectum;
            SemiMinorAxis = SemiLatusRectum;
            SphereOfInfluenceRadius = SemiMajorAxis * Math.Pow((double)(Body.Mass / Parent.Mass), 2f / 5f);
            Period = Math.Tau * (SemiMajorAxis * SemiMajorAxis * SemiMajorAxis / Constants.G / Parent.Mass).Sqrt();
            Center = SD_Vector2.FromPolar(Periapsis, Equation(Periapsis)) + 
                     SD_Vector2.FromPolar(Periapsis, -SemiMajorAxis);
        }
        if (Eccentricity is > 0 and < 1)
        {
            SemiMajorAxis = (Equation(Periapsis) + Equation(Periapsis + Math.PI)) / 2;
            SemiMinorAxis = (Equation(Periapsis) * Equation(Periapsis + Math.PI)).Sqrt();
            SphereOfInfluenceRadius = SemiMajorAxis * Math.Pow((double)(Body.Mass / Parent.Mass), 2f / 5f);
            Period = Math.Tau * (SemiMajorAxis * SemiMajorAxis * SemiMajorAxis / Constants.G / Parent.Mass).Sqrt();
            Center = SD_Vector2.FromPolar(Periapsis, Equation(Periapsis)) + 
                     SD_Vector2.FromPolar(Periapsis, -SemiMajorAxis);
        }
        if (Eccentricity >= 1)
        {
            SemiMajorAxis = (Equation(Periapsis) + Equation(Periapsis + Math.PI)) / 2;
            SemiMinorAxis = ScientificDecimal.PosInfinity;
            SphereOfInfluenceRadius = null;
            Period = ScientificDecimal.PosInfinity;
            Center = null;
        }
        
        if (Eccentricity < 1)
        {
            if (initials)
            {
                SD_Vector2 initialPosition = Body.Position - Parent.Position;
                double initialTrueAnomaly = SD_Vector2.Direction(Parent.Position, initialPosition) - Periapsis;
                if (SD_Vector2.Dot(Body.Velocity, Body.Position) > 0) initialTrueAnomaly = Math.Tau - initialTrueAnomaly;

                double initialEccentricAnomaly = Math.Atan2(
                    Math.Sqrt(1 - Eccentricity * Eccentricity) * Math.Sin(initialTrueAnomaly),
                    Eccentricity + Math.Cos(initialTrueAnomaly)
                ) % Math.Tau;

                double initialMeanAnomaly = (initialEccentricAnomaly - Eccentricity * (
                    Math.Sqrt(1 - Eccentricity * Eccentricity) * Math.Sin(initialTrueAnomaly) /
                    1 + Eccentricity * Math.Cos(initialTrueAnomaly)
                )) % Math.Tau;

                InitialTime = initialMeanAnomaly / (Math.Tau / Period);
            }
        }
    }
    
    private double CalculateEccentricAnomaly(double meanAnomaly)
    {
        ScientificDecimal epsilon = new ScientificDecimal(1, -35);
        double eccentricAnomaly = meanAnomaly;
        int iterations = 0;
        while (double.Abs(eccentricAnomaly - Eccentricity * Math.Sin(eccentricAnomaly) - meanAnomaly) > epsilon)
        {
            if (iterations > 100) return eccentricAnomaly;
            eccentricAnomaly -= (eccentricAnomaly - Eccentricity * Math.Sin(eccentricAnomaly) - meanAnomaly) /
                                (1 - Eccentricity * Math.Cos(eccentricAnomaly));
            iterations++;
        }
        return eccentricAnomaly;
    }

    private double CalculateTrueAnomaly(double eccentricAnomaly)
    {
        return 2 * Math.Atan2(Math.Sqrt(1 + Eccentricity) * Math.Sin(eccentricAnomaly / 2), 
            Math.Sqrt(1 - Eccentricity) * Math.Cos(eccentricAnomaly / 2));
    }

    public SpatialInfo GetStateAtTime(SpatialInfo currentState, ScientificDecimal time)
    {
        if (InitialTime == null) return new();
        time %= Period; 
        
        SpatialInfo newState = new SpatialInfo();
        double meanAnomaly = (double)(Math.Tau / Period * (time + InitialTime)) + Periapsis;
        double eccentricAnomaly = CalculateEccentricAnomaly(meanAnomaly - Periapsis) + Periapsis;
        double trueAnomaly = CalculateTrueAnomaly(eccentricAnomaly - Periapsis) + Periapsis;
        newState.Position = Parent.Position + SD_Vector2.FromPolar(trueAnomaly, Equation(trueAnomaly));
        return newState;
    }
    
    private void DrawPartialEllipseOrbit(GraphicsDevice graphicsDevice, Camera camera, double minAngle, double maxAngle,
        Color colour)
    {
        Body centralForce = Parent;
        List<SD_Vector2> orbitPoints = new List<SD_Vector2>();
        
        // redistribute angles between 0 and tau to be biased towards pi (argument of apoapsis)
        double EllipseBiasFunction(double angle, double exponent)
        {
            angle = Utils.WrapAngle(angle);
            double result = Math.PI - Math.PI * Math.Pow(1 - angle / Math.PI, exponent);
            if (angle > Math.PI) result = Math.PI + Math.PI * Math.Pow(angle / Math.PI - 1, exponent);
            return result;
        }
        
        double exponent = Options.EllipsePointDistributionBiasStrength * 
            Math.Pow(Eccentricity, 1 - Eccentricity) + 1;
            
        // orbit is too small to draw
        if (camera.ConvertToScreenDistance(SemiMajorAxis) < 1) return;
        // draw partial orbit if camera is zoomed in.
        if (maxAngle - minAngle < Options.OrbitApproximationZoomFraction * Math.PI)
        {
            // apply inverse ellipse bias function of camera angle limits
            double unbiasedMinAngle = EllipseBiasFunction(minAngle - Periapsis, 1 / exponent);
            double unbiasedMaxAngle = EllipseBiasFunction(maxAngle - Periapsis, 1 / exponent);
            // sweep through angle range and re-apply bias function on each point then draw the orbit
            for (double a = unbiasedMinAngle; a < unbiasedMaxAngle; 
                 a += (unbiasedMaxAngle - unbiasedMinAngle) / Options.OrbitResolutionNumPoints)
            {
                double trueAngle = EllipseBiasFunction(a, exponent) + Periapsis;
                ScientificDecimal dist = Equation(trueAngle);
                orbitPoints.Add(centralForce.Position + SD_Vector2.FromPolar(trueAngle, dist));
            }
        }
        
        for (int i = 0; i < orbitPoints.Count - 1; ++i)
            graphicsDevice.GS_DrawLine(camera, orbitPoints[i], orbitPoints[i + 1], colour);
    }

    private void DrawEllipseOrbit(GraphicsDevice graphicsDevice, Camera camera, OrbitMesh orbitMesh, Body centralForce,
        Color colour)
    {
        Utils.GetMinAngleRange(out double minAngle, out double maxAngle,
            (camera.TopRight - centralForce.Position).Direction(),
            (camera.TopLeft - centralForce.Position).Direction(),
            (camera.BottomLeft - centralForce.Position).Direction(),
            (camera.BottomRight - centralForce.Position).Direction()
        );
        if (maxAngle < minAngle) maxAngle += Math.Tau;

        if (maxAngle - minAngle > Math.PI / 16)
        {
            // draw the entire orbit as an ellipse
            if (Center == null)
                throw new NullReferenceException("Elliptic orbit must have a center.");
            Vector2 screenPosition = camera.ConvertToScreenCoordinates(Center.Value + centralForce.Position);
            float screenMajorRadius = camera.ConvertToScreenDistance(SemiMajorAxis);
            float screenMinorRadius = camera.ConvertToScreenDistance(SemiMinorAxis);

            Matrix transform = Matrix.CreateScale(new Vector3(screenMajorRadius, screenMinorRadius, 1)) *
                               Matrix.CreateRotationZ((float)(Periapsis + camera.Angle)) *
                               Matrix.CreateScale(new Vector3(1, -1, 0)) *
                               Matrix.CreateTranslation(new Vector3(screenPosition.X, screenPosition.Y, 0));
            orbitMesh.Draw(graphicsDevice, transform, new()
            {
                { "Colour", colour.ToVector4() }
            });
        }
        else DrawPartialEllipseOrbit(graphicsDevice, camera, minAngle, maxAngle, colour);
    }

    private void DrawHyperbolaOrbit(GraphicsDevice graphicsDevice, Camera camera, Body centralForce, Color colour)
    {
        List<SD_Vector2> orbitPoints = new List<SD_Vector2>();
        
        ScientificDecimal? parentSOIRadius = centralForce.Orbit?.SphereOfInfluenceRadius ?? null;
        double asymptoteAngle = Utils.WrapAngle(Math.Acos(-(1 / Eccentricity)));
        double objectAngle = Utils.WrapAngle((Body.Position - centralForce.Position).Direction());
        for (double a = -asymptoteAngle; a < asymptoteAngle; a += 2 * asymptoteAngle / Options.OrbitResolutionNumPoints)
        {
            double trueAngle = Utils.WrapAngle(a + Periapsis);
            ScientificDecimal dist = Equation(trueAngle);
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
                orbitPoints.Add(centralForce.Position + SD_Vector2.FromPolar(objectAngle, Equation(objectAngle)));
        }

        if (parentSOIRadius != null)
        {
            double escapeAngle = Math.Acos((double)((parentSOIRadius / SemiLatusRectum - 1) /
                                                    (Eccentricity * parentSOIRadius /
                                                     SemiLatusRectum))) + Math.PI;
            orbitPoints.Add(centralForce.Position + SD_Vector2.FromPolar(-escapeAngle + Periapsis, parentSOIRadius.Value));
            orbitPoints.Insert(0, centralForce.Position + SD_Vector2.FromPolar(escapeAngle + Periapsis, parentSOIRadius.Value));
        }
        
        for (int i = 0; i < orbitPoints.Count - 1; ++i)
            graphicsDevice.GS_DrawLine(camera, orbitPoints[i], orbitPoints[i + 1], colour);
    }

    /// <summary>
    /// Draws a conical section orbit of an object around a parent using the Laplace-Runge-Lenz vector.
    /// </summary>
    public void DrawOrbitalPath(GraphicsDevice graphicsDevice, Camera camera, OrbitMesh orbitMesh, Color colour)
    {
        if (Parent == null) return;
        Body centralForce = Parent;
        
        // draw circular and elliptical orbits
        if (Eccentricity < 1) DrawEllipseOrbit(graphicsDevice, camera, orbitMesh, centralForce, colour);
        // draw parabolic and hyperbolic orbits
        else DrawHyperbolaOrbit(graphicsDevice, camera, centralForce, colour);
    }
}