using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Represents a point on a Keplerian Orbit
/// </summary>
public readonly record struct KeplerOrbitPathPoint(KeplerOrbitPath Path, double TrueAnomaly)
{
    public ScientificDecimal GetTimeAtPoint(ScientificDecimal currentTime)
    {
        if (Path.Orbit == null) throw new NullReferenceException("KeplerOrbitPathPoint has no orbit");
        KeplerOrbit orbit = Path.Orbit.Value;
        ScientificDecimal timeSincePeriapsis = orbit.InitialTimeSincePeriapsis;
        ScientificDecimal selectTimeFromPeriapsis = orbit.CalculateTimeSincePeriapsisFromTrueAnomaly(TrueAnomaly);
        ScientificDecimal timeUntilPoint = selectTimeFromPeriapsis - timeSincePeriapsis - currentTime;
        if (orbit.Eccentricity >= 1) return timeUntilPoint + currentTime + timeSincePeriapsis;
        while (timeUntilPoint < 0) timeUntilPoint += orbit.Period;
        return currentTime + timeUntilPoint;
    }
}

/// <summary>
/// Wrapper for a Keplerian orbit which contains interactive functionality and drawing operations.
/// </summary>
public class KeplerOrbitPath
{
    public KeplerOrbit? Orbit { get; set; }
    private bool _onScreen;

    public static KeplerOrbitPathPoint? HoverPoint;
    public static KeplerOrbitPathPoint? SelectedPoint;
    private static float _minMouseDistanceToOrbit = float.MaxValue;

    public KeplerOrbitPath()
    {
        MouseHandler.MouseHover += OrbitPath_MouseHover;
        MouseHandler.MouseDown += OrbitPathMouseDown;
        OrbitGame.UpdateFrame += OrbitPath_UpdateFrame;
    }

    private static void OrbitPath_UpdateFrame(object? sender, EventArgs e)
    {
        _minMouseDistanceToOrbit = float.MaxValue;
    }
    
    private void OrbitPath_MouseHover(object? sender, MouseEventArgs e)
    {
        if (!_onScreen) return;
        if (Orbit == null) return;
        KeplerOrbit orbit = Orbit.Value;
        SD_Vector2 mouseWorldPosition = OrbitGame.Camera.ConvertToWorldCoordinates(e.Position);
        SD_Vector2 mouseParentDifferenceVector = mouseWorldPosition - orbit.Parent.Position;
        double mouseClickTrueAnomaly = mouseParentDifferenceVector.Direction() - orbit.Periapsis;
        ScientificDecimal mouseClickPathDistance = mouseParentDifferenceVector.Magnitude();
        float thisMouseDistanceToOrbit = OrbitGame.Camera.ConvertToScreenDistance(
            (orbit.Equation(mouseClickTrueAnomaly + orbit.Periapsis) - mouseClickPathDistance).Abs()
        );
        if (thisMouseDistanceToOrbit < _minMouseDistanceToOrbit)
        {
            _minMouseDistanceToOrbit = thisMouseDistanceToOrbit;
            if (thisMouseDistanceToOrbit < 10)
                HoverPoint = new(this, mouseClickTrueAnomaly);
            else HoverPoint = null;
        }
    }

    private void OrbitPathMouseDown(object? sender, MouseEventArgs e)
    {
        if (HoverPoint == null)
            SelectedPoint = null;
        SelectedPoint = HoverPoint;
    }

    private void DrawSelectedOrbitPoint(KeplerOrbit orbit, Color colour)
    {
        Camera camera = OrbitGame.Camera;
        GraphicsDevice graphicsDevice = OrbitGame.Graphics;
        
        if (HoverPoint != null)
        {
            if (HoverPoint.Value.Path == this)
            {
                double absoluteAngle = HoverPoint.Value.TrueAnomaly + orbit.Periapsis;
                SD_Vector2 orbitPointPosition = orbit.Parent.Position +
                                                SD_Vector2.FromPolar(absoluteAngle, orbit.Equation(absoluteAngle));
                graphicsDevice.GS_DrawPoint(camera, orbitPointPosition, colour);
            }
        }
        if (SelectedPoint != null)
        {
            if (SelectedPoint.Value.Path == this)
            {
                double absoluteAngle = SelectedPoint.Value.TrueAnomaly + orbit.Periapsis;
                SD_Vector2 orbitPointPosition = orbit.Parent.Position +
                                                SD_Vector2.FromPolar(absoluteAngle, orbit.Equation(absoluteAngle));
                graphicsDevice.GS_DrawPoint(camera, orbitPointPosition, Color.Red);
                
            }
        }
    }
    
    private void DrawPartialEllipseOrbit(KeplerOrbit orbit, 
        double minAngle, double maxAngle, Color colour)
    {
        Camera camera = OrbitGame.Camera;
        GraphicsDevice graphicsDevice = OrbitGame.Graphics;
        
        Body centralForce = orbit.Parent;
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
            Math.Pow(orbit.Eccentricity, 1 - orbit.Eccentricity) + 1;
            
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

        _onScreen = orbitPoints.Count != 0;
        for (int i = 0; i < orbitPoints.Count - 1; ++i)
            graphicsDevice.GS_DrawLine(camera, orbitPoints[i], orbitPoints[i + 1], colour);
    }

    private void DrawEllipseOrbit(OrbitMesh orbitMesh, KeplerOrbit orbit, 
        Body centralForce, Color colour)
    {
        Camera camera = OrbitGame.Camera;
        GraphicsDevice graphicsDevice = OrbitGame.Graphics;
        
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
            Vector2 screenPosition = camera.ConvertToScreenCoordinates(orbit.Center + centralForce.Position);
            float screenMajorRadius = camera.ConvertToScreenDistance(orbit.SemiMajorAxis);
            float screenMinorRadius = camera.ConvertToScreenDistance(orbit.SemiMinorAxis);

            if (screenMajorRadius < 1)
            {
                _onScreen = false;
                return;
            }
            _onScreen = true;

            Matrix transform = Matrix.CreateScale(new Vector3(screenMajorRadius, screenMinorRadius, 1)) *
                               Matrix.CreateRotationZ((float)(orbit.Periapsis + camera.Angle)) *
                               Matrix.CreateScale(new Vector3(1, -1, 0)) *
                               Matrix.CreateTranslation(new Vector3(screenPosition.X, screenPosition.Y, 0));
            orbitMesh.Draw(graphicsDevice, transform, new()
            {
                { "Colour", colour.ToVector4() }
            });
        }
        else DrawPartialEllipseOrbit(orbit, minAngle, maxAngle, colour);
    }

    private void DrawHyperbolaOrbit(KeplerOrbit orbit, 
        Body centralForce, Color colour)
    {
        Camera camera = OrbitGame.Camera;
        GraphicsDevice graphicsDevice = OrbitGame.Graphics;
        
        List<SD_Vector2> orbitPoints = new List<SD_Vector2>();
        
        ScientificDecimal? parentSOIRadius = centralForce.KeplerOrbitPath.Orbit?.SphereOfInfluenceRadius ?? null;
        double asymptoteAngle = Utils.WrapAngle(Math.Acos(-(1 / orbit.Eccentricity)));
        double objectAngle = Utils.WrapAngle((orbit.Body.Position - centralForce.Position).Direction());
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

        orbitPoints.RemoveAll(x => x.MagnitudeSquared().IsInfinite);
        _onScreen = orbitPoints.Count != 0;
        for (int i = 0; i < orbitPoints.Count - 1; ++i)
            graphicsDevice.GS_DrawLine(camera, orbitPoints[i], orbitPoints[i + 1], colour);
    }

    /// <summary>
    /// Draws a conical section orbit of an object around a parent using the Laplace-Runge-Lenz vector.
    /// </summary>
    public void DrawOrbitalPath(OrbitMesh orbitMesh, Color colour)
    {
        if (Orbit == null)
        {
            _onScreen = false;
            return;
        }
        KeplerOrbit orbit = Orbit.Value;
        Body centralForce = orbit.Parent;
        
        // draw circular and elliptical orbits
        if (orbit.Eccentricity < 1) DrawEllipseOrbit(orbitMesh, orbit, centralForce, colour);
        // draw parabolic and hyperbolic orbits
        else DrawHyperbolaOrbit(orbit, centralForce, colour);
        DrawSelectedOrbitPoint(orbit, colour);
    }
}