using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

namespace OrbitGame;

public class Planet : Body
{
    public readonly ScientificDecimal Radius;
    
    public Planet(
        ScientificDecimal mass,
        SD_Vector2 position,
        SD_Vector2 velocity,
        ScientificDecimal radius,
        Material material,
        Color colour,
        Body? parent,
        string name
    )
        : base(mass, position, velocity, colour, name, parent)
    {
        Radius = radius;
        CircularCollider collider = new CircularCollider(Radius, this, material) {
            Fixed = true
        };
        Collider = collider;
    }

    public override void Draw(SpriteBatch spriteBatch, Camera camera)
    {
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
            
            spriteBatch.DrawPoly(camera, polyPoints, Colour);
        }
        
        // if the planet is too small to draw on screen, instead draw its approximate location with a marker
        else if (camera.Height >= Radius / Options.LocationApproximationRadiusZoomFraction)
        {
            Vector2 screenPosition = camera.ConvertToScreenCoordinates(Position);
            spriteBatch.DrawLine(screenPosition + new Vector2(10, 0), screenPosition + new Vector2(0, 10), Colour);
            spriteBatch.DrawLine(screenPosition + new Vector2(0, 10), screenPosition + new Vector2(-10, 0), Colour);
            spriteBatch.DrawLine(screenPosition + new Vector2(-10, 0), screenPosition + new Vector2(0, -10), Colour);
            spriteBatch.DrawLine(screenPosition + new Vector2(0, -10), screenPosition + new Vector2(10, 0), Colour);
        }
        
        // otherwise draw the planet as a circle
        else spriteBatch.GS_DrawCircle(camera, Position, Radius, Colour);
    }

    public override void DrawCollider(SpriteBatch canvas, Camera camera)
    {
        Draw(canvas, camera);
    }
    
    public void DrawSphereOfInfluence(SpriteBatch canvas, Camera camera)
    {
        if (Orbit == null) return;
        KeplerOrbit orbit = (KeplerOrbit)Orbit;
        
        if (orbit.SphereOfInfluenceRadius == null) return;
        ScientificDecimal sphereOfInfluenceRadius = (ScientificDecimal)orbit.SphereOfInfluenceRadius;
        
        // paint for spheres of influence
        Color colour = new Color((int)Colour.R, Colour.G, Colour.B, 1);
        
        canvas.GS_DrawCircle(camera, Position, sphereOfInfluenceRadius, colour);
    }
}