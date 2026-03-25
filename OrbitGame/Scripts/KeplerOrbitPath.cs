using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OrbitGame;

/// <summary>
/// Wrapper for a Keplerian orbit which contains interactive functionality and drawing operations.
/// </summary>
public class KeplerOrbitPath
{
    public List<ManeuverNode> ManeuverNodes { get; set; } = [];
    public KeplerOrbit? Orbit { get; set; }
    public double StartAngle { get; }
    public double EndAngle { get; }
    private bool _onScreen;

    public static KeplerOrbitPoint? HoverPoint;
    public static KeplerOrbitPoint? SelectedPoint;
    private static SDecimal _minMouseDistanceToOrbit = SDecimal.PosInfinity;

    private readonly OrbitMesh _mesh;
    private readonly Color _colour;

    public KeplerOrbitPath(OrbitMesh mesh, Color colour, double startAngle = 0, double endAngle = Math.Tau)
    {
        _mesh = mesh;
        _colour = colour;
        
        StartAngle = startAngle;
        EndAngle = endAngle;
        
        MouseHandler.MouseHover += KeplerOrbitPath_MouseHover;
        MouseHandler.MouseDown += KeplerOrbitPath_MouseDown;
        OrbitGame.UpdateFrame += KeplerOrbitPath_UpdateFrame;
    }

    /// <summary>
    /// Returns the point on the orbit with the minimum Euclidean distance to a chosen point.
    /// </summary>
    /// <param name="orbit">Keplerian orbit</param>
    /// <param name="externalPoint">Point which may not lie on the orbit in world space</param>
    /// <param name="minimumDistance">Distance between the external point and the parabola</param>
    /// <returns>The true anomaly of the closest point on the orbit</returns>
    private double GetClosestOrbitPoint(KeplerOrbit orbit, DVector2<SDecimal> externalPoint, out SDecimal minimumDistance)
    {
        externalPoint -= orbit.Parent.Position;

        double guessPoint = externalPoint.Direction() - orbit.Periapsis;
        if (orbit.Equation(guessPoint + orbit.Periapsis) < 0) guessPoint += Math.PI; 
        double learningRate = 0.1;
        
        SDecimal currentGuessDistance = GetGuessDistance(guessPoint);
        SDecimal lastGuessDistance = SDecimal.PosInfinity;

        int iterations = 0;

        do
        {
            while (true)
            {
                SDecimal nextGuessDistance = GetGuessDistance(guessPoint + learningRate);
                if (nextGuessDistance >= currentGuessDistance) break;
                lastGuessDistance = currentGuessDistance;
                currentGuessDistance = nextGuessDistance;
                guessPoint += learningRate;
            } 

            while (true)
            {
                SDecimal nextIntervalDistance = GetGuessDistance(guessPoint - learningRate);
                if (nextIntervalDistance >= currentGuessDistance) break;
                lastGuessDistance = currentGuessDistance;
                currentGuessDistance = nextIntervalDistance;
                guessPoint -= learningRate;
            }

            learningRate /= 2;
            iterations++;
        } while (SDecimal.Abs(lastGuessDistance - currentGuessDistance) > 100 && iterations < 25);

        minimumDistance = currentGuessDistance;
        return guessPoint;

        SDecimal GetGuessDistance(double point)
        {
            DVector2<SDecimal> orbitalPosition = orbit.GetOrbitPositionFromTrueAnomaly(point);
            return (externalPoint - orbitalPosition).MagnitudeSquared();
        }
    }

    private static void KeplerOrbitPath_UpdateFrame(object? sender, EventArgs e)
    {
        _minMouseDistanceToOrbit = SDecimal.PosInfinity;
    }
    
    private void KeplerOrbitPath_MouseHover(object? sender, MouseEventArgs e)
    {
        if (!_onScreen) return;
        if (Orbit == null) return;
        Camera camera = OrbitGame.Camera;
        KeplerOrbit orbit = Orbit.Value;
        DVector2<SDecimal> mouseWorldPosition = camera.ConvertToWorldCoordinates(e.Position);
        double closestOrbitPointTrueAnomaly = GetClosestOrbitPoint(
            orbit, mouseWorldPosition, out SDecimal closestOrbitDistanceSquared
        );
        
        if (closestOrbitDistanceSquared < _minMouseDistanceToOrbit)
        {
            _minMouseDistanceToOrbit = closestOrbitDistanceSquared;
            if (!SDecimal.IsInfinity(closestOrbitDistanceSquared))
            {
                float screenMouseDistanceToOrbit = camera.ConvertToScreenDistance(
                    SDecimal.Sqrt(closestOrbitDistanceSquared));
                if (screenMouseDistanceToOrbit < 10)
                    HoverPoint = new(this, closestOrbitPointTrueAnomaly);
                else HoverPoint = null;
            }
            else HoverPoint = null;
        }
    }

    private void KeplerOrbitPath_MouseDown(object? sender, MouseEventArgs e)
    {
        if (HoverPoint == null)
            SelectedPoint = null;
        SelectedPoint = HoverPoint;
    }

    private void DrawSelectedOrbitPoint(KeplerOrbit orbit, Color colour)
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;
        
        if (HoverPoint != null)
        {
            if (HoverPoint.Value.Path == this)
            {
                DVector2<SDecimal> orbitalPosition = orbit.GetOrbitPositionFromTrueAnomaly(HoverPoint.Value.TrueAnomaly);
                DVector2<SDecimal> orbitPointPosition = orbit.Parent.Position + orbitalPosition;
                graphicsDevice.SD_DrawPoint(camera, orbitPointPosition, colour);
            }
        }
        if (SelectedPoint != null)
        {
            if (SelectedPoint.Value.Path == this)
            {
                DVector2<SDecimal> orbitalPosition = orbit.GetOrbitPositionFromTrueAnomaly(SelectedPoint.Value.TrueAnomaly);
                DVector2<SDecimal> orbitPointPosition = orbit.Parent.Position + orbitalPosition;
                graphicsDevice.SD_DrawPoint(camera, orbitPointPosition, Color.Red);
            }
        }
    }
    
    private void DrawPartialEllipseOrbit(KeplerOrbit orbit, double minAngle, double maxAngle, Color colour)
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;
        
        Body centralForce = orbit.Parent;
        List<DVector2<SDecimal>> orbitPoints = new List<DVector2<SDecimal>>();
        
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
            
        // apply inverse ellipse bias function of camera angle limits
        double unbiasedMinAngle = EllipseBiasFunction(minAngle - orbit.Periapsis, 1 / exponent);
        double unbiasedMaxAngle = EllipseBiasFunction(maxAngle - orbit.Periapsis, 1 / exponent);
        
        // sweep through angle range and re-apply bias function on each point then draw the orbit
        for (double a = unbiasedMinAngle; a < unbiasedMaxAngle; 
             a += (unbiasedMaxAngle - unbiasedMinAngle) / Options.OrbitResolutionNumPoints)
        {
            double trueAngle = EllipseBiasFunction(a, exponent) + orbit.Periapsis;
            SDecimal dist = orbit.Equation(trueAngle);
            orbitPoints.Add(centralForce.Position + DVector2<SDecimal>.FromPolar(trueAngle, dist));
        }

        _onScreen = orbitPoints.Count != 0;
        for (int i = 0; i < orbitPoints.Count - 1; ++i)
            graphicsDevice.SD_DrawLine(camera, orbitPoints[i], orbitPoints[i + 1], colour);
    }

    private void DrawEllipseOrbit(OrbitMesh orbitMesh, KeplerOrbit orbit, Body centralForce, Color colour)
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;

        Utils.GetMinAngleRange(out double minAngle, out double maxAngle,
            (camera.TopRight - centralForce.Position).Direction(),
            (camera.TopLeft - centralForce.Position).Direction(),
            (camera.BottomLeft - centralForce.Position).Direction(),
            (camera.BottomRight - centralForce.Position).Direction()
        );
        if (maxAngle < minAngle) maxAngle += Math.Tau;

        if (maxAngle - minAngle > Options.OrbitApproximationZoomFraction * Math.PI)
        {
            if (Math.Abs(EndAngle - StartAngle) >= Math.Tau)
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
                graphicsDevice.DrawMesh(orbitMesh, transform, new()
                {
                    { "Colour", colour.ToVector4() }
                });
                return;
            }
            minAngle = orbit.Periapsis + StartAngle;
            maxAngle = orbit.Periapsis + EndAngle;
        }
        
        double drawnMinAngle = Math.Max(minAngle, orbit.Periapsis + StartAngle);
        double drawnMaxAngle = Math.Min(maxAngle, orbit.Periapsis + EndAngle);
        if (orbit.Body.Identifier == "Earth")
        {
            Console.WriteLine($"{drawnMinAngle} {drawnMaxAngle}");
        }

        if (drawnMinAngle > drawnMaxAngle) return;
        DrawPartialEllipseOrbit(orbit, drawnMinAngle, drawnMaxAngle, colour);
    }

    private void DrawHyperbolaOrbit(KeplerOrbit orbit, Body centralForce, Color colour)
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;
        
        List<DVector2<SDecimal>> orbitPoints = new List<DVector2<SDecimal>>();
        
        SDecimal? parentSOIRadius = centralForce.OrbitPath.GetSphereOfInfluenceRadius();
        double asymptoteAngle = Utils.WrapAngle(Math.Acos(-(1 / orbit.Eccentricity)));
        double objectAngle = Utils.WrapAngle((orbit.Body.Position - centralForce.Position).Direction());
        for (double a = -asymptoteAngle; a < asymptoteAngle; a += 2 * asymptoteAngle / Options.OrbitResolutionNumPoints)
        {
            double trueAngle = Utils.WrapAngle(a + orbit.Periapsis);
            SDecimal dist = orbit.Equation(trueAngle);
            DVector2<SDecimal> orbitPoint = centralForce.Position + DVector2<SDecimal>.FromPolar(trueAngle, dist);
            if (parentSOIRadius != null)
            {
                if (dist > 0 && dist < parentSOIRadius)
                    orbitPoints.Add(orbitPoint);
            }
            else if (dist > 0 && SDecimal.IsFinite(dist)) 
                orbitPoints.Add(orbitPoint);
            if (double.IsPositive(trueAngle - objectAngle) !=  
                double.IsPositive(trueAngle + 2 * asymptoteAngle / Options.OrbitResolutionNumPoints - objectAngle))
                orbitPoints.Add(centralForce.Position + orbit.GetOrbitPositionFromWorldAngle(objectAngle));
        }

        if (parentSOIRadius != null)
        {
            SDecimal semiLatusRectum = (SDecimal)orbit.SemiLatusRectum;
            double escapeAngle = Math.Acos((double)((parentSOIRadius / semiLatusRectum - 1) /
                                                    (orbit.Eccentricity * parentSOIRadius /
                                                     semiLatusRectum))) + Math.PI;
            orbitPoints.Add(centralForce.Position + DVector2<SDecimal>.FromPolar(-escapeAngle + orbit.Periapsis, parentSOIRadius.Value));
            orbitPoints.Insert(0, centralForce.Position + DVector2<SDecimal>.FromPolar(escapeAngle + orbit.Periapsis, parentSOIRadius.Value));
        }

        orbitPoints.RemoveAll(x => SDecimal.IsInfinity(x.MagnitudeSquared()));
        _onScreen = orbitPoints.Count != 0;
        for (int i = 0; i < orbitPoints.Count - 1; ++i)
            graphicsDevice.SD_DrawLine(camera, orbitPoints[i], orbitPoints[i + 1], colour);
    }

    /// <summary>
    /// Draws a conical section orbit of an object around a parent using the Laplace-Runge-Lenz vector.
    /// </summary>
    public void Draw()
    {
        if (Orbit == null)
        {
            _onScreen = false;
            return;
        }
        KeplerOrbit orbit = Orbit.Value;
        Body centralForce = orbit.Parent;
        
        // draw circular and elliptical orbits
        if (orbit.Eccentricity < 1) DrawEllipseOrbit(_mesh, orbit, centralForce, _colour);
        // draw parabolic and hyperbolic orbits
        else DrawHyperbolaOrbit(orbit, centralForce, _colour);
        DrawSelectedOrbitPoint(orbit, _colour);
    }

    public void DrawCollider()
        => Draw();
}