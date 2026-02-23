using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public readonly record struct OrbitPathPoint(
    OrbitPath Path,
    double TrueAnomaly
);

public class OrbitPath
{
    public KeplerOrbit? Orbit { get; set; }

    private static OrbitPathPoint? _hoverPoint;
    private static OrbitPathPoint? _selectedPoint;
    private static float _minMouseDistanceToOrbit = float.MaxValue;

    public OrbitPath()
    {
        MouseHandler.MouseMove += OrbitPath_MouseMove;
        MouseHandler.MouseDown += OrbitPathMouseDown;
        OrbitGame.UpdateFrame += OrbitPath_UpdateFrame;
    }

    private void OrbitPath_UpdateFrame(object? sender, EventArgs e)
    {
        _minMouseDistanceToOrbit = float.MaxValue;
    }
    
    private void OrbitPath_MouseMove(object? sender, MouseEventArgs e)
    {
        if (Orbit == null) return;
        KeplerOrbit orbit = Orbit.Value;
        SD_Vector2 mouseWorldPosition = OrbitGame.Camera.ConvertToWorldCoordinates(e.Position);
        SD_Vector2 mouseParentDifferenceVector = mouseWorldPosition - orbit.Parent.Position;
        double mouseClickTrueAnomaly = mouseParentDifferenceVector.Direction();
        ScientificDecimal mouseClickPathDistance = mouseParentDifferenceVector.Magnitude();
        float thisMouseDistanceToOrbit = OrbitGame.Camera.ConvertToScreenDistance((orbit.Equation(mouseClickTrueAnomaly) -
                                                                          mouseClickPathDistance).Abs());
        if (thisMouseDistanceToOrbit < _minMouseDistanceToOrbit)
        {
            _minMouseDistanceToOrbit = thisMouseDistanceToOrbit;
            if (thisMouseDistanceToOrbit < 10)
                _hoverPoint = new(this, mouseClickTrueAnomaly);
            else _hoverPoint = null;
        }
    }

    private void OrbitPathMouseDown(object? sender, MouseEventArgs e)
    {
        if (_hoverPoint == null)
            _selectedPoint = null;
        _selectedPoint = _hoverPoint;
    }

    private void DrawSelectedOrbitPoint(GraphicsDevice graphicsDevice, Camera camera, KeplerOrbit orbit, Color colour)
    {
        if (_selectedPoint != null)
        {
            if (_selectedPoint.Value.Path == this)
            {
                double trueAnomaly = _selectedPoint.Value.TrueAnomaly;
                SD_Vector2 orbitPointPosition = orbit.Parent.Position +
                                                SD_Vector2.FromPolar(trueAnomaly, orbit.Equation(trueAnomaly));
                graphicsDevice.GS_DrawPoint(camera, orbitPointPosition, Color.Red);
                
                if (_hoverPoint?.Path == this) return;
            }
        }
        if (_hoverPoint != null)
        {
            if (_hoverPoint.Value.Path == this)
            {
                double trueAnomaly = _hoverPoint.Value.TrueAnomaly;
                SD_Vector2 orbitPointPosition = orbit.Parent.Position +
                                                SD_Vector2.FromPolar(trueAnomaly, orbit.Equation(trueAnomaly));
                graphicsDevice.GS_DrawPoint(camera, orbitPointPosition, colour);
            }
        }
    }
    
    private void DrawPartialEllipseOrbit(GraphicsDevice graphicsDevice, Camera camera, KeplerOrbit orbit, 
        double minAngle, double maxAngle, Color colour)
    {
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
            
        // orbit is too small to draw
        if (camera.ConvertToScreenDistance(orbit.SemiMajorAxis) < 1) return;
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
        
        for (int i = 0; i < orbitPoints.Count - 1; ++i)
            graphicsDevice.GS_DrawLine(camera, orbitPoints[i], orbitPoints[i + 1], colour);
    }

    private void DrawEllipseOrbit(GraphicsDevice graphicsDevice, Camera camera, OrbitMesh orbitMesh, KeplerOrbit orbit, 
        Body centralForce, Color colour)
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
            if (orbit.Center == null)
                throw new NullReferenceException("Elliptic orbit must have a center.");
            Vector2 screenPosition = camera.ConvertToScreenCoordinates(orbit.Center.Value + centralForce.Position);
            float screenMajorRadius = camera.ConvertToScreenDistance(orbit.SemiMajorAxis);
            float screenMinorRadius = camera.ConvertToScreenDistance(orbit.SemiMinorAxis);

            Matrix transform = Matrix.CreateScale(new Vector3(screenMajorRadius, screenMinorRadius, 1)) *
                               Matrix.CreateRotationZ((float)(orbit.Periapsis + camera.Angle)) *
                               Matrix.CreateScale(new Vector3(1, -1, 0)) *
                               Matrix.CreateTranslation(new Vector3(screenPosition.X, screenPosition.Y, 0));
            orbitMesh.Draw(graphicsDevice, transform, new()
            {
                { "Colour", colour.ToVector4() }
            });
        }
        else DrawPartialEllipseOrbit(graphicsDevice, camera, orbit, minAngle, maxAngle, colour);
    }

    private void DrawHyperbolaOrbit(GraphicsDevice graphicsDevice, Camera camera, KeplerOrbit orbit, 
        Body centralForce, Color colour)
    {
        List<SD_Vector2> orbitPoints = new List<SD_Vector2>();
        
        ScientificDecimal? parentSOIRadius = centralForce.OrbitPath.Orbit?.SphereOfInfluenceRadius ?? null;
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
        
        for (int i = 0; i < orbitPoints.Count - 1; ++i)
            graphicsDevice.GS_DrawLine(camera, orbitPoints[i], orbitPoints[i + 1], colour);
    }

    /// <summary>
    /// Draws a conical section orbit of an object around a parent using the Laplace-Runge-Lenz vector.
    /// </summary>
    public void DrawOrbitalPath(GraphicsDevice graphicsDevice, Camera camera, OrbitMesh orbitMesh, Color colour)
    {
        if (Orbit == null) return;
        KeplerOrbit orbit = Orbit.Value;
        Body centralForce = orbit.Parent;
        
        // draw circular and elliptical orbits
        if (orbit.Eccentricity < 1) DrawEllipseOrbit(graphicsDevice, camera, orbitMesh, orbit, centralForce, colour);
        // draw parabolic and hyperbolic orbits
        else DrawHyperbolaOrbit(graphicsDevice, camera, orbit, centralForce, colour);
        DrawSelectedOrbitPoint(graphicsDevice, camera, orbit, colour);
    }
}