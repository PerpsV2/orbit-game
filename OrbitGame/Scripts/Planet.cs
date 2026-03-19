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
    private readonly OrbitMesh _orbitMesh;

    private Planet(
        string identifier,
        SpatialInfo spatialInfo,
        ObjectInfo objectInfo,
        OrbitMesh orbitMesh,
        SDecimal mass,
        SDecimal radius,
        Color colour,
        Body? parent)
        : base(identifier, spatialInfo, objectInfo, mass, colour, parent)
    {
        Radius = radius;
        _orbitMesh = orbitMesh;
        
        objectInfo.Collider.CalculateInertia(mass);
    }

    public void Draw()
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;
        
        DrawSphereOfInfluence();
        KeplerOrbitPath.Draw(_orbitMesh, Colour);

        if ((Position - camera.Position).MagnitudeSquared() - 4 * Radius * Radius > camera.MaximumRadiusSquared) return;
        
        // if the planet is too large to draw on screen as a circle, draw its intersection with the camera as a line
        if (camera.Height <= Radius / Options.SurfaceApproximationRadiusZoomFraction)
        {
            DVector2<SDecimal> screenPosition = camera.SD_ConvertToScreenCoordinates(Position);

            float h = Options.ScreenSize.height;
            float w = Options.ScreenSize.width;
            SDecimal p1 = screenPosition.Y;
            SDecimal p2 = screenPosition.X;
            SDecimal r = camera.SD_ConvertToScreenDistance(Radius);

            SDecimal topDiscriminant = 2 * h * p1 - p1 * p1 - h * h + r * r;
            SDecimal bottomDiscriminant = r * r - p1 * p1;
            SDecimal rightDiscriminant = 2 * w * p2 - p2 * p2 - w * w + r * r;
            SDecimal leftDiscriminant = r * r - p2 * p2;
            SDecimal radical;

            List<DVector2<SDecimal>> intersectionPoints = new();

            if (topDiscriminant >= 0)
            {
                radical = SDecimal.Sqrt(topDiscriminant);
                if (!(p2 - radical < 0 && p2 + radical < 0) && !(p2 - radical > w && p2 + radical > w))
                {
                    intersectionPoints.Add(new DVector2<SDecimal>(SDecimal.Clamp(p2 - radical, 0, w), h));
                    intersectionPoints.Add(new DVector2<SDecimal>(SDecimal.Clamp(p2 + radical, 0, w), h));
                }
            }

            if (rightDiscriminant >= 0)
            {
                radical = SDecimal.Sqrt(rightDiscriminant);
                if (!(p1 - radical < 0 && p1 + radical < 0) && !(p1 - radical > h && p1 + radical > h))
                {
                    intersectionPoints.Add(new DVector2<SDecimal>(w, SDecimal.Clamp(p1 + radical, 0, h)));
                    intersectionPoints.Add(new DVector2<SDecimal>(w, SDecimal.Clamp(p1 - radical, 0, h)));
                }
            }

            if (bottomDiscriminant >= 0)
            {
                radical = SDecimal.Sqrt(bottomDiscriminant);
                if (!(p2 - radical < 0 && p2 + radical < 0) && !(p2 - radical > w && p2 + radical > w))
                {
                    intersectionPoints.Add(new DVector2<SDecimal>(SDecimal.Clamp(p2 + radical, 0, w), 0));
                    intersectionPoints.Add(new DVector2<SDecimal>(SDecimal.Clamp(p2 - radical, 0, w), 0));
                }
            }

            if (leftDiscriminant >= 0)
            {
                radical = SDecimal.Sqrt(leftDiscriminant);
                if (!(p1 - radical < 0 && p1 + radical < 0) && !(p1 - radical > h && p1 + radical > h))
                {
                    intersectionPoints.Add(new DVector2<SDecimal>(0, SDecimal.Clamp(p1 - radical, 0, h)));
                    intersectionPoints.Add(new DVector2<SDecimal>(0, SDecimal.Clamp(p1 + radical, 0, h)));
                }
            }

            if (intersectionPoints.Count == 0) return;

            intersectionPoints = intersectionPoints.GroupBy(z => z).Select(z => z.First()).ToList();
            var polyPoints = intersectionPoints.Select(v => new Vector2((float)v.X, (float)v.Y)).ToList();

            graphicsDevice.DrawPoly(polyPoints, Colour);
        }
        
        // if the planet is too small to draw on screen, instead draw its approximate location with a marker
        else if (camera.Height >= Radius / Options.LocationApproximationRadiusZoomFraction)
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

        // otherwise draw the planet as a circle
        else
        {
            Vector2 screenCenter = camera.ConvertToScreenCoordinates(Position);
            float screenRadius = camera.ConvertToScreenDistance(Radius);
            Matrix transform = Matrix.CreateScale(screenRadius, screenRadius, 1) *
                               Matrix.CreateTranslation(new Vector3(screenCenter.X, screenCenter.Y, 0));
            graphicsDevice.DrawMesh(Mesh, transform, new()
            {
                { "Colour", Colour.ToVector4() }
            });
        }
    }

    public void DrawCollider()
    {
        Draw();
    }

    private void DrawSphereOfInfluence()
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;
        
        if (KeplerOrbitPath.Orbit == null) return;
        KeplerOrbit orbit = (KeplerOrbit)KeplerOrbitPath.Orbit;
        SDecimal sphereOfInfluenceRadius = orbit.SphereOfInfluenceRadius;

        // paint for spheres of influence
        Color soiColour = new Color(Colour.R, Colour.G, Colour.B) * Options.SOIAlpha;
        
        Vector2 screenCenter = camera.ConvertToScreenCoordinates(Position);
        float screenRadius = camera.ConvertToScreenDistance(sphereOfInfluenceRadius);
        Matrix transform = Matrix.CreateScale(screenRadius, screenRadius, 1) *
                           Matrix.CreateTranslation(new Vector3(screenCenter.X, screenCenter.Y, 0));
        graphicsDevice.DrawMesh(Mesh, transform, new()
        {
            {"Colour", soiColour.ToVector4()}
        });
    }

    public class PlanetTemplate(Material material) : KinematicObjectTemplate
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
            Body? parent)
        {
            CircularCollider collider = new CircularCollider(radius);
            ObjectInfo objectInfo = new ObjectInfo(_mesh, collider, _material);
            Planet planet = new Planet(identifier, spatialInfo, objectInfo, _orbitMesh, mass, radius, colour, parent);
            AddInstance(identifier, planet);
            return planet;
        }
    }
}