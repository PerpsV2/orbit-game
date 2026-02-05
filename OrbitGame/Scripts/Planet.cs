using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public class Planet : Body, IGameDrawable
{
    public readonly ScientificDecimal Radius;
    private OrbitMesh _orbitMesh;

    private Planet(
        PlanetTemplate template,
        string identifier,
        ScientificDecimal mass,
        SpatialInfo spatialInfo,
        Color colour,
        Body? parent,
        ScientificDecimal radius,
        CompactCollider collider
    )
        : base(template, identifier, mass, spatialInfo, colour, parent, null, collider)
    {
        Radius = radius;
        collider.CalculateInertia(mass);
    }

    public override void Draw(GraphicsDevice graphicsDevice, Camera camera)
    {
        DrawSphereOfInfluence(graphicsDevice, camera);
        DrawOrbitalPathLRL(graphicsDevice, camera, _orbitMesh);
        
        if (Position.X < camera.Left - Radius) return;
        if (Position.X > camera.Right + Radius) return;
        if (Position.Y > camera.Top + Radius) return;
        if (Position.Y < camera.Bottom - Radius) return;
        
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
            graphicsDevice.DrawLine(screenPosition + new Vector2(10, 0), screenPosition + new Vector2(0, 10), Colour);
            graphicsDevice.DrawLine(screenPosition + new Vector2(0, 10), screenPosition + new Vector2(-10, 0), Colour);
            graphicsDevice.DrawLine(screenPosition + new Vector2(-10, 0), screenPosition + new Vector2(0, -10), Colour);
            graphicsDevice.DrawLine(screenPosition + new Vector2(0, -10), screenPosition + new Vector2(10, 0), Colour);
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
                {"colour", Colour.ToVector4()}
            });
        }
    }

    public override void DrawCollider(GraphicsDevice graphicsDevice, Camera camera)
    {
        Draw(graphicsDevice, camera);
    }

    private void DrawSphereOfInfluence(GraphicsDevice graphicsDevice, Camera camera)
    {
        if (Orbit == null) return;
        KeplerOrbit orbit = (KeplerOrbit)Orbit;

        if (orbit.SphereOfInfluenceRadius == null) return;
        ScientificDecimal sphereOfInfluenceRadius = (ScientificDecimal)orbit.SphereOfInfluenceRadius;

        // paint for spheres of influence
        Color soiColour = new Color((int)Colour.R, Colour.G, Colour.B, 1);
        
        Vector2 screenCenter = camera.ConvertToScreenCoordinates(Position);
        float screenRadius = camera.ConvertToScreenDistance(sphereOfInfluenceRadius);
        Matrix transform = Matrix.CreateScale(screenRadius, screenRadius, 1) *
                           Matrix.CreateTranslation(new Vector3(screenCenter.X, screenCenter.Y, 0));
        Mesh.Draw(graphicsDevice, transform, new()
        {
            {"colour", soiColour.ToVector4()}
        });

        //canvas.GS_DrawCircle(camera, Position, sphereOfInfluenceRadius, colour);
    }

    public class PlanetTemplate(Material material) 
        : KinematicObjectTemplate(new CircularMesh(), null, material)
    {
        private readonly OrbitMesh _orbitMesh = new();
        
        public Planet CreateInstance(
            string identifier,
            ScientificDecimal mass,
            SD_Vector2 position, 
            SD_Vector2 velocity, 
            double angle,
            double angularVelocity,
            Color colour,
            Body? parent,
            ScientificDecimal radius
            )
        {
            SpatialInfo spatialInfo = new SpatialInfo(position, velocity, SD_Vector2.Zero, angle, angularVelocity);
            CircularCollider collider = new CircularCollider(radius);
            Planet planet = new Planet(this, identifier, mass, spatialInfo, colour, parent, radius, collider) {
                    Angle = angle,
                    AngularVelocity = angularVelocity,
                    _orbitMesh = _orbitMesh
                };
            AddInstance(identifier, planet);
            return planet;
        }
    }
}