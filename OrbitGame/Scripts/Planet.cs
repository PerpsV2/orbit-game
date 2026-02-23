using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public class Planet : Body, IGameDrawable
{
    public readonly ScientificDecimal Radius;
    private readonly OrbitMesh _orbitMesh;

    private Planet(
        string identifier,
        SpatialInfo spatialInfo,
        ObjectInfo objectInfo,
        OrbitMesh orbitMesh,
        ScientificDecimal mass,
        ScientificDecimal radius,
        Color colour,
        Body? parent)
        : base(identifier, spatialInfo, objectInfo, mass, colour, parent)
    {
        Radius = radius;
        _orbitMesh = orbitMesh;
        
        objectInfo.Collider.CalculateInertia(mass);
    }

    public void Draw(GraphicsDevice graphicsDevice, Camera camera)
    {
        DrawSphereOfInfluence(graphicsDevice, camera);
        OrbitPath.DrawOrbitalPath(graphicsDevice, camera, _orbitMesh, Colour);

        if ((Position - camera.Position).MagnitudeSquared() - 4 * Radius.Square() > camera.MaximumRadiusSquared) return;
        
        // if the planet is too large to draw on screen as a circle, draw its intersection with the camera as a line
        if (camera.Height <= Radius / Options.SurfaceApproximationRadiusZoomFraction)
        {
            SD_Vector2 screenPosition = camera.SD_ConvertToScreenCoordinates(Position);

            float h = Options.ScreenSize.height;
            float w = Options.ScreenSize.width;
            ScientificDecimal p1 = screenPosition.Y;
            ScientificDecimal p2 = screenPosition.X;
            ScientificDecimal r = camera.SD_ConvertToScreenDistance(Radius);

            ScientificDecimal topDiscriminant = 2 * h * p1 - p1 * p1 - h * h + r * r;
            ScientificDecimal bottomDiscriminant = r * r - p1 * p1;
            ScientificDecimal rightDiscriminant = 2 * w * p2 - p2 * p2 - w * w + r * r;
            ScientificDecimal leftDiscriminant = r * r - p2 * p2;
            ScientificDecimal radical;

            List<SD_Vector2> intersectionPoints = new();

            if (topDiscriminant >= 0)
            {
                radical = topDiscriminant.Sqrt();
                if (!(p2 - radical < 0 && p2 + radical < 0) && !(p2 - radical > w && p2 + radical > w))
                {
                    intersectionPoints.Add(new SD_Vector2((p2 - radical).Clamp(0, w), h));
                    intersectionPoints.Add(new SD_Vector2((p2 + radical).Clamp(0, w), h));
                }
            }

            if (rightDiscriminant >= 0)
            {
                radical = rightDiscriminant.Sqrt();
                if (!(p1 - radical < 0 && p1 + radical < 0) && !(p1 - radical > h && p1 + radical > h))
                {
                    intersectionPoints.Add(new SD_Vector2(w, (p1 + radical).Clamp(0, h)));
                    intersectionPoints.Add(new SD_Vector2(w, (p1 - radical).Clamp(0, h)));
                }
            }

            if (bottomDiscriminant >= 0)
            {
                radical = bottomDiscriminant.Sqrt();
                if (!(p2 - radical < 0 && p2 + radical < 0) && !(p2 - radical > w && p2 + radical > w))
                {
                    intersectionPoints.Add(new SD_Vector2((p2 + radical).Clamp(0, w), 0));
                    intersectionPoints.Add(new SD_Vector2((p2 - radical).Clamp(0, w), 0));
                }
            }

            if (leftDiscriminant >= 0)
            {
                radical = leftDiscriminant.Sqrt();
                if (!(p1 - radical < 0 && p1 + radical < 0) && !(p1 - radical > h && p1 + radical > h))
                {
                    intersectionPoints.Add(new SD_Vector2(0, (p1 - radical).Clamp(0, h)));
                    intersectionPoints.Add(new SD_Vector2(0, (p1 + radical).Clamp(0, h)));
                }
            }

            if (intersectionPoints.Count == 0) return;

            intersectionPoints = intersectionPoints.GroupBy(z => z).Select(z => z.First()).ToList();
            var polyPoints = intersectionPoints.Select(v => new Vector2((float)v.X, (float)v.Y)).ToList();

            Utils.DrawPoly(graphicsDevice, polyPoints, Colour);
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
            Mesh.Draw(graphicsDevice, transform, new()
            {
                { "Colour", Colour.ToVector4() }
            });
        }
    }

    public void DrawCollider(GraphicsDevice graphicsDevice, Camera camera)
    {
        Draw(graphicsDevice, camera);
    }

    private void DrawSphereOfInfluence(GraphicsDevice graphicsDevice, Camera camera)
    {
        if (OrbitPath.Orbit == null) return;
        KeplerOrbit orbit = (KeplerOrbit)OrbitPath.Orbit;

        if (orbit.SphereOfInfluenceRadius == null) return;
        ScientificDecimal sphereOfInfluenceRadius = (ScientificDecimal)orbit.SphereOfInfluenceRadius;

        // paint for spheres of influence
        Color soiColour = new Color(Colour.R, Colour.G, Colour.B) * Options.SOIAlpha;
        
        Vector2 screenCenter = camera.ConvertToScreenCoordinates(Position);
        float screenRadius = camera.ConvertToScreenDistance(sphereOfInfluenceRadius);
        Matrix transform = Matrix.CreateScale(screenRadius, screenRadius, 1) *
                           Matrix.CreateTranslation(new Vector3(screenCenter.X, screenCenter.Y, 0));
        Mesh.Draw(graphicsDevice, transform, new()
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
            ScientificDecimal mass,
            ScientificDecimal radius,
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