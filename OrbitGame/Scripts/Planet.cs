using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace OrbitGame;

/// <summary>
/// Large celestial body which is simulated on rails using Keplerian physics rather than Newtonian.
/// </summary>
public class Planet : Body, IGameDrawable
{
    public readonly SDecimal Radius;
    private readonly int _terrainSeed;

    private readonly Random _rnd;

    private Planet(
        string identifier,
        SpatialInfo spatialInfo,
        ObjectInfo objectInfo,
        SDecimal mass,
        SDecimal radius,
        Color colour,
        Body? parent,
        int seed)
        : base(identifier, spatialInfo, objectInfo, mass, colour, parent)
    {
        Radius = radius;
        GenerateTerrainCollider();
        _terrainSeed = seed;
        _rnd = new Random(seed);
        GenerateOrbitPath(0);
        objectInfo.Collider.CalculateInertia(mass);
    }

    public void GenerateTerrainCollider()
    {
        Camera camera = OrbitGame.Camera;

        Utils.GetMinAngleRange(out double minAngle, out double maxAngle, 
            (camera.TopRight - Position).Direction(),
            (camera.TopLeft - Position).Direction(),
            (camera.BottomLeft - Position).Direction(),
            (camera.BottomRight - Position).Direction());

        if (maxAngle <= minAngle) maxAngle += Math.Tau;
        
        var elevationPoints = new List<(double, SDecimal)>();
        Utils.IterateAngleRange(minAngle, maxAngle, (maxAngle - minAngle) / 100, (_, angle) => {
            elevationPoints.Add((angle, Radius + GetElevationAtPoint(angle)));
        });
        Utils.IterateAngleRange(0, Math.Tau, Math.Tau / 100, (_, angle) => {
            elevationPoints.Add((angle, Radius + GetElevationAtPoint(angle)));
        });
        
        ObjectInfo.Collider = new TerrainCollider(elevationPoints.ToArray());
    }

    private void DrawTerrain()
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;

        Utils.GetMinAngleRange(out double minAngle, out double maxAngle, 
            (camera.TopRight - Position).Direction(),
            (camera.TopLeft - Position).Direction(),
            (camera.BottomLeft - Position).Direction(),
            (camera.BottomRight - Position).Direction());

        if (maxAngle <= minAngle) maxAngle += Math.Tau;
        
        List<Vector2> terrainMeshVertices = [camera.ConvertToScreenCoordinates(Position)];
        
        Utils.IterateAngleRange(minAngle, maxAngle, (maxAngle - minAngle) / 200, (_, angle) => {
            terrainMeshVertices.Add(
                camera.ConvertToScreenCoordinates(
                    Vec2<SDecimal>.FromPolar(angle, Radius + GetElevationAtPoint(angle)) + Position
                    )
                );
        });
        
        Utils.IterateAngleRange(maxAngle, minAngle, (minAngle + Math.Tau - maxAngle) / 100, (_, angle) => {
            terrainMeshVertices.Add(
                camera.ConvertToScreenCoordinates(
                    Vec2<SDecimal>.FromPolar(angle, Radius + GetElevationAtPoint(angle)) + Position
                )
            );
        });
        
        graphicsDevice.DrawPoly(terrainMeshVertices, Colour);
    }

    public void DrawZoomedIn()
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;

        Vec2Double screenPosition = (Vec2Double)camera.SD_ConvertToScreenCoordinates(Position);

        float h = Options.ScreenSize.height;
        float w = Options.ScreenSize.width;
        double p1 = screenPosition.Y;
        double p2 = screenPosition.X;
        double r = camera.ConvertToScreenDistance(Radius);

        double topDiscriminant = 2 * h * p1 - p1 * p1 - h * h + r * r;
        double bottomDiscriminant = r * r - p1 * p1;
        double rightDiscriminant = 2 * w * p2 - p2 * p2 - w * w + r * r;
        double leftDiscriminant = r * r - p2 * p2;
        double radical;

        List<Vec2Double> intersectionPoints = new();

        if (topDiscriminant >= 0)
        {
            radical = Math.Sqrt(topDiscriminant);
            if (!(p2 - radical < 0 && p2 + radical < 0) && !(p2 - radical > w && p2 + radical > w))
            {
                intersectionPoints.Add(new Vec2Double(Math.Clamp(p2 - radical, 0, w), h));
                intersectionPoints.Add(new Vec2Double(Math.Clamp(p2 + radical, 0, w), h));
            }
        }

        if (rightDiscriminant >= 0)
        {
            radical = Math.Sqrt(rightDiscriminant);
            if (!(p1 - radical < 0 && p1 + radical < 0) && !(p1 - radical > h && p1 + radical > h))
            {
                intersectionPoints.Add(new Vec2Double(w, Math.Clamp(p1 + radical, 0, h)));
                intersectionPoints.Add(new Vec2Double(w, Math.Clamp(p1 - radical, 0, h)));
            }
        }

        if (bottomDiscriminant >= 0)
        {
            radical = Math.Sqrt(bottomDiscriminant);
            if (!(p2 - radical < 0 && p2 + radical < 0) && !(p2 - radical > w && p2 + radical > w))
            {
                intersectionPoints.Add(new Vec2Double(Math.Clamp(p2 + radical, 0, w), 0));
                intersectionPoints.Add(new Vec2Double(Math.Clamp(p2 - radical, 0, w), 0));
            }
        }

        if (leftDiscriminant >= 0)
        {
            radical = Math.Sqrt(leftDiscriminant);
            if (!(p1 - radical < 0 && p1 + radical < 0) && !(p1 - radical > h && p1 + radical > h))
            {
                intersectionPoints.Add(new Vec2Double(0, Math.Clamp(p1 - radical, 0, h)));
                intersectionPoints.Add(new Vec2Double(0, Math.Clamp(p1 + radical, 0, h)));
            }
        }

        if (intersectionPoints.Count == 0) return;
        
        intersectionPoints = intersectionPoints.GroupBy(z => z).Select(z => z.First()).ToList();
        var polyPoints = intersectionPoints.Select(v => new Vector2((float)v.X, (float)v.Y)).ToList();
        
        graphicsDevice.DrawPoly(polyPoints, Colour);
    }

    public void Draw()
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;
        
        DrawSphereOfInfluence();
        OrbitPath.Draw();

        if ((Position - camera.Position).MagnitudeSquared() - 4 * Radius * Radius > camera.MaximumRadiusSquared) return;

        // if the planet is too small to draw on screen, instead draw its approximate location with a marker
        if (camera.Height >= Radius / Options.LocationApproximationRadiusZoomFraction)
        {
            Vector2 screenPosition = camera.ConvertToScreenCoordinates(Position);
            graphicsDevice.DrawLine(screenPosition + new Vector2(10, 0), screenPosition + new Vector2(0, 10),
                Colour);
            graphicsDevice.DrawLine(screenPosition + new Vector2(0, 10), screenPosition + new Vector2(-10, 0),
                Colour);
            graphicsDevice.DrawLine(screenPosition + new Vector2(-10, 0), screenPosition + new Vector2(0, -10),
                Colour);
            graphicsDevice.DrawLine(screenPosition + new Vector2(0, -10), screenPosition + new Vector2(10, 0),
                Colour);
        }
        else DrawTerrain();

        // otherwise draw the planet as a circle
        // else
        // {
        //     Vector2 screenCenter = camera.ConvertToScreenCoordinates(Position);
        //     float screenRadius = camera.ConvertToScreenDistance(Radius);
        //     Matrix transform = Matrix.CreateScale(screenRadius, screenRadius, 1) *
        //                        Matrix.CreateTranslation(new Vector3(screenCenter.X, screenCenter.Y, 0));
        //     graphicsDevice.DrawMesh(Mesh, transform, new()
        //     {
        //         { "Colour", Colour.ToVector4() }
        //     });
        // }
    }

    public void DrawCollider()
    {
        Draw();
    }

    private void DrawSphereOfInfluence()
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;
        
        if (OrbitPath.IsEmpty()) return;

        // paint for spheres of influence
        Color soiColour = new Color(Colour.R, Colour.G, Colour.B) * Options.SOIAlpha;
        
        Vector2 screenCenter = camera.ConvertToScreenCoordinates(Position);
        float screenRadius = camera.ConvertToScreenDistance(OrbitPath.GetSphereOfInfluenceRadius() ?? 0);
        Matrix transform = Matrix.CreateScale(screenRadius, screenRadius, 1) *
                           Matrix.CreateTranslation(new Vector3(screenCenter.X, screenCenter.Y, 0));
        graphicsDevice.DrawMesh(Mesh, transform, new()
        {
            {"Colour", soiColour.ToVector4()}
        });
    }

    public sealed override void GenerateOrbitPath(SDecimal time)
        => base.GenerateOrbitPath(time);

    public SDecimal GetElevationAtPoint(double angle)
    {
        return 150 * Math.Cos(angle * 32000);
    }

    public class PlanetTemplate(Material material) : BodyTemplate
    {
        private readonly CircularMesh _mesh = new();
        private readonly OrbitMesh _orbitMesh = new();
        private readonly Material _material = material;

        public Planet CreateInstance(
            string identifier,
            SpatialInfo spatialInfo,
            SDecimal mass,
            SDecimal radius,
            Color colour,
            Body? parent,
            int seed)
        {
            CircularCollider collider = new CircularCollider((double)radius);
            ObjectInfo objectInfo = new ObjectInfo(_mesh, collider, _material) {
                OrbitMesh = _orbitMesh
            };
            Planet planet = new Planet(identifier, spatialInfo, objectInfo, mass, radius, colour, parent, seed);
            OrbitGame.Hierarchy.AddObject(planet);
            return planet;
        }
    }
}