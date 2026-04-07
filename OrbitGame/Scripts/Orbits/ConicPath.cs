using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public bool MouseDetectionEnabled = true;

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
        if (!_onScreen || !MouseDetectionEnabled) return;
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
        
        float[] conicCoefficients = new float[6];
        double e = orbit.Eccentricity;
        double e2 = e * e;
        double p = Math.Tau - orbit.Periapsis;
        double l = camera.ConvertToScreenDistance(orbit.SemiLatusRectum);
        Vec2<SDecimal> fVec = camera.SD_ConvertToScreenCoordinates(orbit.Parent.Position);
        SDecimal fX = fVec.X;
        SDecimal fY = fVec.Y;
        double cosP = Math.Cos(p);
        double sinP = Math.Sin(p);
        conicCoefficients[0] = (float)(1 - e2 * cosP * cosP);
        conicCoefficients[1] = -(float)(2 * e2 * cosP * sinP);
        conicCoefficients[2] = (float)(1 - e2 * sinP * sinP);
        conicCoefficients[3] = -2 * (float)(fX * (1 - e2 * cosP * cosP) - fY / 2 * e2 * Math.Sin(2 * p) - e * l * cosP);
        conicCoefficients[4] = -2 * (float)(fY * (1 - e2 * sinP * sinP) - fX / 2 * e2 * Math.Sin(2 * p) - e * l * sinP);
        conicCoefficients[5] = (float)(fX * fX * (1 - e2 * cosP * cosP) + 
                                       fY * fY * (1 - e2 * sinP * sinP) - 
                                       fX * fY * e2 * Math.Sin(2 * p) - 
                                       2 * e * l * (fX * cosP + fY * sinP) - l * l);

        /*if (conicCoefficients.Any(x => x > 1000000))
        {
            Utils.GetMinAngleRange(out double minAngle, out double maxAngle, 
                (camera.TopRight - orbit.Parent.Position).Direction(),
                (camera.TopLeft - orbit.Parent.Position).Direction(),
                (camera.BottomLeft - orbit.Parent.Position).Direction(),
                (camera.BottomRight - orbit.Parent.Position).Direction());

            if (minAngle > maxAngle) maxAngle += Math.Tau;
            
            Vector2 start = camera.ConvertToScreenCoordinates(orbit.GetOrbitPositionFromWorldAngle(minAngle) + orbit.Parent.Position);
            Vector2 end = camera.ConvertToScreenCoordinates(orbit.GetOrbitPositionFromWorldAngle(maxAngle) + orbit.Parent.Position);
            graphics.DrawLine(start, end, _colour);
            return;
        }*/

        float drawnStartAngle = (float)0;
        float drawnEndAngle = (float)Math.Tau;
        
        if (StartAngle is not null && EndAngle is not null)
        {
            drawnStartAngle = (float)Utils.WrapAngle(StartAngle.Value - orbit.Periapsis);
            drawnEndAngle = (float)Utils.WrapAngle(EndAngle.Value - orbit.Periapsis);
        }
        
        if (drawnStartAngle > drawnEndAngle) (drawnStartAngle, drawnEndAngle) = (drawnEndAngle, drawnStartAngle);

        Color colour = _colour;
        
        graphics.DrawMesh(_orbitMesh, Matrix.Identity, new Dictionary<string, object>
        {
            { "Colour", colour.ToVector4() },
            { "Center", camera.ConvertToScreenCoordinates(orbit.Center + orbit.Parent.Position) },
            { "Focus", camera.ConvertToScreenCoordinates(orbit.Parent.Position) },
            { "Eccentricity", (float)orbit.Eccentricity },
            { "ConicSymmetryAngle", (float)(orbit.Periapsis - Math.PI / 2) },
            { "C1", conicCoefficients[0] },
            { "C2", conicCoefficients[1] },
            { "C3", conicCoefficients[2] },
            { "C4", conicCoefficients[3] },
            { "C5", conicCoefficients[4] },
            { "C6", conicCoefficients[5] },
            { "StartAngle", drawnStartAngle },
            { "EndAngle", drawnEndAngle },
            { "TexelSize", new Vector2(0.5f, 0.5f) }
        });

        DrawSelectedOrbitPoint(orbit, _colour);
    }
}