using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Wrapper for a Keplerian orbit which contains interactive functionality and drawing operations.
/// </summary>
public class ConicPath
{
    public PatchedConicPath Path { get; }
    public KeplerOrbit? Orbit { get; set; }

    public double? StartAngle;
    public double? EndAngle;

    private bool _onScreen;

    public static KeplerOrbitPoint? HoverPoint;
    public static KeplerOrbitPoint? SelectedPoint;
    private static SDecimal _minMouseDistanceToOrbit = SDecimal.PositiveInfinity;

    private static ScreenMesh _orbitMesh = new();
    
    private readonly OrbitMesh _mesh;
    private readonly Color _colour;

    public ConicPath(
        PatchedConicPath patchedConicPath, 
        OrbitMesh mesh, Color colour, 
        double? startAngle = null, double? endAngle = null)
    {
        Path = patchedConicPath;
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
    /// <param name="externalPoint">Point which may not lie on the orbit in world space</param>
    /// <param name="minimumDistance">Distance between the external point and the parabola</param>
    /// <returns>The true anomaly of the closest point on the orbit</returns>
    private double GetClosestOrbitPoint(Vec2<SDecimal> externalPoint, out SDecimal minimumDistance)
    {
        if (Orbit == null)
        {
            minimumDistance = SDecimal.PositiveInfinity;
            return 0;
        }
        KeplerOrbit orbit = Orbit.Value;
        
        externalPoint -= orbit.Parent.Position;

        double guessPoint = externalPoint.Direction() - orbit.Periapsis;
        if (orbit.GetDistanceFromTrueAnomaly(guessPoint) < 0) guessPoint += Math.PI; 
        double learningRate = 0.1;
        
        double orbitStartAngle = 0;
        double orbitEndAngle = Math.Tau;
        if (StartAngle != null && EndAngle != null)
        {
            orbitStartAngle = StartAngle.Value - orbit.Periapsis;
            orbitEndAngle = EndAngle.Value - orbit.Periapsis;
        }
        
        SDecimal currentGuessDistance = GetGuessDistance(guessPoint);
        SDecimal lastGuessDistance = SDecimal.PositiveInfinity;

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

        guessPoint = Utils.WrapAngle(guessPoint);
        
        if (guessPoint < orbitStartAngle || guessPoint > orbitEndAngle)
        {
            double startAngleArc = Utils.WrapAngle(Math.Abs(guessPoint - orbitStartAngle));
            double endAngleArc = Utils.WrapAngle(Math.Abs(guessPoint - orbitEndAngle));

            if (startAngleArc < endAngleArc)
            {
                minimumDistance = GetGuessDistance(orbitStartAngle);
                return orbitStartAngle;
            }

            minimumDistance = GetGuessDistance(orbitEndAngle);
            return orbitEndAngle;
        }

        minimumDistance = currentGuessDistance;
        return guessPoint;

        SDecimal GetGuessDistance(double point)
        {
            Vec2<SDecimal> orbitalPosition = orbit.GetOrbitPositionFromTrueAnomaly(point);
            return (externalPoint - orbitalPosition).MagnitudeSquared();
        }
    }

    private static void KeplerOrbitPath_UpdateFrame(object? sender, EventArgs e)
    {
        _minMouseDistanceToOrbit = SDecimal.PositiveInfinity;
    }
    
    private void KeplerOrbitPath_MouseHover(object? sender, MouseEventArgs e)
    {
        if (!_onScreen) return;
        Camera camera = OrbitGame.Camera;
        Vec2<SDecimal> mouseWorldPosition = camera.ConvertToWorldCoordinates(e.Position);
        double closestOrbitPointTrueAnomaly = GetClosestOrbitPoint(
            mouseWorldPosition, out SDecimal closestOrbitDistanceSquared
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
            if (HoverPoint.Value.ConicPath == this)
            {
                Vec2<SDecimal> orbitalPosition = orbit.GetOrbitPositionFromTrueAnomaly(HoverPoint.Value.TrueAnomaly);
                Vec2<SDecimal> orbitPointPosition = orbit.Parent.Position + orbitalPosition;
                graphicsDevice.SD_DrawPoint(camera, orbitPointPosition, colour);
            }
        }
        if (SelectedPoint != null)
        {
            if (SelectedPoint.Value.ConicPath == this)
            {
                Vec2<SDecimal> orbitalPosition = orbit.GetOrbitPositionFromTrueAnomaly(SelectedPoint.Value.TrueAnomaly);
                Vec2<SDecimal> orbitPointPosition = orbit.Parent.Position + orbitalPosition;
                graphicsDevice.SD_DrawPoint(camera, orbitPointPosition, Color.Red);
            }
        }
    }
    
    private void DrawPartialEllipseOrbit(KeplerOrbit orbit, double minTrueAnomaly, double maxTrueAnomaly, Color colour)
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;
        
        Body centralForce = orbit.Parent;
        List<Vec2<SDecimal>> orbitPoints = new List<Vec2<SDecimal>>();
        
        // function to redistribute angles between 0 and tau to be biased towards pi (argument of apoapsis)
        double EllipseBiasFunction(double angle, double exponent)
        {
            double result = Math.PI - Math.PI * Math.Pow(1 - angle / Math.PI, exponent);
            if (angle > Math.PI) result = Math.PI + Math.PI * Math.Pow(angle / Math.PI - 1, exponent);
            return result;
        }
        
        double biasExponent = Options.EllipsePointDistributionBiasStrength * 
            Math.Pow(orbit.Eccentricity, 1 - orbit.Eccentricity) + 1;
            
        // apply inverse ellipse bias function on camera angle limits
        double inverseBiasedMinTrueAnomaly = EllipseBiasFunction(minTrueAnomaly, 1 / biasExponent);
        double inverseBiasedMaxTrueAnomaly = EllipseBiasFunction(maxTrueAnomaly, 1 / biasExponent);
        
        // sweep through angle range and re-apply bias function on each point then draw the orbit
        for (double a = inverseBiasedMinTrueAnomaly; a < inverseBiasedMaxTrueAnomaly; 
             a += (inverseBiasedMaxTrueAnomaly - inverseBiasedMinTrueAnomaly) / Options.OrbitResolutionNumPoints)
        {
            double trueAnomaly = EllipseBiasFunction(a, biasExponent);
            orbitPoints.Add(centralForce.Position + orbit.GetOrbitPositionFromTrueAnomaly(trueAnomaly));
        }

        _onScreen = orbitPoints.Count != 0;
        graphicsDevice.SD_DrawPath(camera, orbitPoints, colour);
    }

    private void DrawEllipseOrbit(OrbitMesh orbitMesh, KeplerOrbit orbit, Body centralForce, Color colour)
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;

        if (orbit.Eccentricity < 0)
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
        }
        else
        {
            double minTrueAnomaly = 0;
            double maxTrueAnomaly = Math.Tau;
            if (StartAngle is not null && EndAngle is not null)
            {
                minTrueAnomaly = StartAngle.Value;
                maxTrueAnomaly = EndAngle.Value;
            }
            if (minTrueAnomaly > maxTrueAnomaly) (minTrueAnomaly, maxTrueAnomaly) = (maxTrueAnomaly, minTrueAnomaly);
            DrawPartialEllipseOrbit(orbit, minTrueAnomaly, maxTrueAnomaly, colour);
        }
    }

    private void DrawHyperbolaOrbit(KeplerOrbit orbit, Body centralForce, Color colour)
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;
        
        Utils.GetMinAngleRange(out double minTrueAnomaly, out double maxTrueAnomaly,
            (camera.TopRight - centralForce.Position).Direction(),
            (camera.TopLeft - centralForce.Position).Direction(),
            (camera.BottomLeft - centralForce.Position).Direction(),
            (camera.BottomRight - centralForce.Position).Direction()
        );
        // convert world angles to true anomaly
        minTrueAnomaly -= orbit.Periapsis;
        maxTrueAnomaly -= orbit.Periapsis;
        if (maxTrueAnomaly < minTrueAnomaly) maxTrueAnomaly += Math.Tau;
        
        List<Vec2<SDecimal>> orbitPoints = new List<Vec2<SDecimal>>();
        
        SDecimal? parentSOIRadius = centralForce.OrbitPath.GetSphereOfInfluenceRadius();
        double asymptoteTrueAnomaly = Utils.WrapAngle(Math.Acos(-(1 / orbit.Eccentricity)));
        double bodyTrueAnomaly = Utils.WrapAngle((orbit.Body.Position - centralForce.Position).Direction()) - orbit.Periapsis;

        double sweepStart = -asymptoteTrueAnomaly;
        double sweepEnd = asymptoteTrueAnomaly;
        if (maxTrueAnomaly - minTrueAnomaly <= Options.OrbitApproximationZoomFraction * Math.PI)
        {
            sweepStart = double.Max(minTrueAnomaly, -asymptoteTrueAnomaly);
            sweepEnd = double.Min(maxTrueAnomaly, asymptoteTrueAnomaly);
        }

        double lastTrueAnomaly = Utils.WrapAngle(-asymptoteTrueAnomaly);
        for (double a = sweepStart; a < sweepEnd; 
             a += (sweepEnd - sweepStart) / Options.OrbitResolutionNumPoints)
        {
            double currentTrueAnomaly = Utils.WrapAngle(a);
            SDecimal dist = orbit.GetDistanceFromTrueAnomaly(currentTrueAnomaly);
            Vec2<SDecimal> orbitPoint = centralForce.Position + orbit.GetOrbitPositionFromTrueAnomaly(currentTrueAnomaly);
            if (parentSOIRadius != null)
            {
                if (dist > 0 && dist < parentSOIRadius)
                    orbitPoints.Add(orbitPoint);
            }
            // TODO: this
            else if (dist > 0 /*&& SDecimal.IsFinite(dist)*/) 
                orbitPoints.Add(orbitPoint);

            if (double.IsPositive(currentTrueAnomaly - bodyTrueAnomaly) !=
                double.IsPositive(lastTrueAnomaly - bodyTrueAnomaly))
                orbitPoints.Add(centralForce.Position + orbit.GetOrbitPositionFromTrueAnomaly(bodyTrueAnomaly));

            lastTrueAnomaly = currentTrueAnomaly;
        }

        if (parentSOIRadius != null)
        {
            SDecimal semiLatusRectum = (SDecimal)orbit.SemiLatusRectum;
            double escapeAngle = Math.Acos((double)((parentSOIRadius / semiLatusRectum - 1) /
                                                    (orbit.Eccentricity * parentSOIRadius /
                                                     semiLatusRectum))) + Math.PI;
            orbitPoints.Add(centralForce.Position + Vec2<SDecimal>.FromPolar(-escapeAngle + orbit.Periapsis, parentSOIRadius.Value));
            orbitPoints.Insert(0, centralForce.Position + Vec2<SDecimal>.FromPolar(escapeAngle + orbit.Periapsis, parentSOIRadius.Value));
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
        _onScreen = true;
        KeplerOrbit orbit = Orbit.Value;

        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphics = OrbitGame.Graphics;

        Color colour = _colour;
        float semiLatusRectum = camera.ConvertToScreenDistance(orbit.SemiLatusRectum.Map<SDecimal>());
        Vector2 centerCoords = camera.ConvertToScreenCoordinates(orbit.Parent.Position);

        float startAnomaly;
        float endAnomaly;
        if (StartAngle is null || EndAngle is null)
        {
            startAnomaly = 0;
            endAnomaly = (float)Math.Tau;
        }
        else
        {
            startAnomaly = (float)StartAngle;
            endAnomaly = (float)EndAngle;
        }
        
        graphics.DrawMesh(_orbitMesh, Matrix.Identity, new Dictionary<string, object> {
            { "Colour", colour.ToVector4() },
            { "Eccentricity", (float)orbit.Eccentricity },
            { "SemiLatusRectum", semiLatusRectum },
            { "ArgumentOfPeriapsis", (float)(orbit.Periapsis + camera.Angle + Math.PI / 2) },
            { "Center", centerCoords },
            { "StartAnomaly", startAnomaly },
            { "EndAnomaly", endAnomaly },
            { "TexelSize", new Vector2(0.5f, 0.5f) }
        });
        
        /*// draw circular and elliptical orbits
        if (orbit.Eccentricity < 1) DrawEllipseOrbit(_mesh, orbit, centralForce, _colour);
        // draw parabolic and hyperbolic orbits
        //else DrawHyperbolaOrbit(orbit, centralForce, _colour);*/
        DrawSelectedOrbitPoint(orbit, _colour);
    }
}