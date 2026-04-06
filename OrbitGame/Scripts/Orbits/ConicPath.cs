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
    public bool MouseDetectionEnabled;

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

    private void DrawZoomedOutConic(KeplerOrbit orbit)
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphics = OrbitGame.Graphics;

        Color colour = _colour;
        
        graphics.DrawMesh(_orbitMesh, Matrix.Identity, new Dictionary<string, object> {
            { "Colour", colour.ToVector4() },
            { "Focus", camera.ConvertToScreenCoordinates(orbit.Parent.Position) },
            { "TexelSize", new Vector2(0.5f, 0.5f) }
        });
        
        //float semiLatusRectum = camera.ConvertToScreenDistance(orbit.SemiLatusRectum.Map<SDecimal>());
        //double argumentOfPeriapsis = orbit.Periapsis + camera.Angle;
        /*Vector2 focusCoords = camera.ConvertToScreenCoordinates(orbit.Parent.Position);
        Vector2 oppositeFocusCoords = camera.ConvertToScreenCoordinates(orbit.Parent.Position +
                                                                        orbit.GetOrbitPositionFromTrueAnomaly(0) +
                                                                        orbit.GetOrbitPositionFromTrueAnomaly(Math.PI));

        if (focusCoords.Length() > oppositeFocusCoords.Length())
        {
            focusCoords = oppositeFocusCoords;
            argumentOfPeriapsis -= Math.PI;
        }*/
        
        /*float startAnomaly;
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
            { "ArgumentOfPeriapsis", (float)-argumentOfPeriapsis },
            { "Center", camera.ConvertToScreenCoordinates(orbit.Parent.Position + orbit.Center) },
            { "MajorAxis", camera.ConvertToScreenDistance(orbit.SemiMajorAxis) },
            { "MinorAxis", camera.ConvertToScreenDistance(orbit.SemiMinorAxis) },
            { "StartAnomaly", startAnomaly },
            { "EndAnomaly", endAnomaly },
            { "TexelSize", new Vector2(0.5f, 0.5f) }
        });*/
        
        /*graphics.DrawMesh(_orbitMesh, Matrix.Identity, new Dictionary<string, object> {
            { "Colour", colour.ToVector4() },
            { "Eccentricity", (float)orbit.Eccentricity },
            { "SemiLatusRectum", semiLatusRectum },
            { "ArgumentOfPeriapsis", (float)argumentOfPeriapsis },
            { "Focus", focusCoords },
            { "StartAnomaly", startAnomaly },
            { "EndAnomaly", endAnomaly },
            { "TexelSize", new Vector2(0.5f, 0.5f) }
        });*/
    }

    private void DrawZoomedInConic(KeplerOrbit orbit)
    {
        
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
        
        Utils.GetMinAngleRange(out double minAngle, out double maxAngle, 
            (camera.TopRight - orbit.Parent.Position).Direction(),
            (camera.TopLeft - orbit.Parent.Position).Direction(),
            (camera.BottomLeft - orbit.Parent.Position).Direction(),
            (camera.BottomRight - orbit.Parent.Position).Direction());

        if (minAngle > maxAngle) maxAngle += Math.Tau;
        
        List<double> samplePointAngles = [];
        Utils.IterateAngleRange(minAngle, maxAngle, (maxAngle - minAngle) / 5, angle => { samplePointAngles.Add(angle); });
        List<Vector2> screenPoints = samplePointAngles
            .Select(x => camera.ConvertToScreenCoordinates(orbit.GetOrbitPositionFromWorldAngle(x) + orbit.Parent.Position))
            .ToList();

        if (screenPoints.Count < 5)
        {
            _onScreen = false;
            return;
        }

        if (screenPoints[0].Length() > 10000 || (screenPoints[0] - screenPoints[1]).Length() < 1)
        {
            _onScreen = false;
            return;
        }
        
        LinearEquationSystem<float> linearSystem = new LinearEquationSystem<float>(5);
        for (int i = 0; i < 5; i++)
        {
            Vector2 point = screenPoints[i];
            linearSystem.SetCoefficient(i * 5 + 0, point.X * point.X);
            linearSystem.SetCoefficient(i * 5 + 1, point.X * point.Y);
            linearSystem.SetCoefficient(i * 5 + 2, point.Y * point.Y);
            linearSystem.SetCoefficient(i * 5 + 3, point.X);
            linearSystem.SetCoefficient(i * 5 + 4, point.Y);
            linearSystem.SetConstant(i, 1);
        }
        float[] conicCoefficients = linearSystem.Solve();

        Color colour = _colour;
        graphics.DrawMesh(_orbitMesh, Matrix.Identity, new Dictionary<string, object>
        {
            { "Colour", colour.ToVector4() },
            { "Focus", Vector2.Zero },
            { "C1", conicCoefficients[0] },
            { "C2", conicCoefficients[1] },
            { "C3", conicCoefficients[2] },
            { "C4", conicCoefficients[3] },
            { "C5", conicCoefficients[4] },
            { "TexelSize", new Vector2(0.5f, 0.5f) }
        });
        
        //if (camera.MaximumRadiusSquared > SDecimal.Square(orbit.GetDistanceFromTrueAnomaly(0)))
        //DrawZoomedOutConic(orbit);
        //else DrawZoomedInConic(orbit);
        DrawSelectedOrbitPoint(orbit, _colour);
    }
}