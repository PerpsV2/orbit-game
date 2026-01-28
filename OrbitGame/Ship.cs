using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

namespace OrbitGame;

public class Ship : Body
{
    private readonly SD_Vector2[] _mesh;
    
    public Ship(
        ScientificDecimal mass, SD_Vector2 position, SD_Vector2 velocity, Material material, Color colour, Planet parent,
        SD_Vector2[] mesh, string name)
        : base(mass, position, velocity, colour, name, parent)
    {
        LinkedList<int> colliderIndices = SD_Vector2.GetConvexHullIndices(mesh);
        _mesh = colliderIndices.Select(x => mesh[x]).ToArray();
        Collider = new ConvexCollider(_mesh, this, material);
    }

    public override void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        SD_Vector2[] polyPoints = _mesh.Select(ObjectToWorldSpace).ToArray();
        spriteBatch.GS_DrawPoly(camera, polyPoints, Colour);
        
        Vector2 screenPosition = camera.ConvertToScreenCoordinates(Position);
        spriteBatch.DrawLine(screenPosition + new Vector2(10, 0), screenPosition + new Vector2(0, 10), Colour);
        spriteBatch.DrawLine(screenPosition + new Vector2(0, 10), screenPosition + new Vector2(-10, 0), Colour);
        spriteBatch.DrawLine(screenPosition + new Vector2(-10, 0), screenPosition + new Vector2(0, -10), Colour);
        spriteBatch.DrawLine(screenPosition + new Vector2(0, -10), screenPosition + new Vector2(10, 0), Colour);
    }

    public override void DrawCollider(SpriteBatch canvas, Camera camera)
    {
        /*using SKPaint paint = new SKPaint();
        Color = Colour;
        paint.StrokeWidth = 4;

        SD_Vector2[] polyPoints = _mesh.Select(ObjectToWorldSpace).ToArray();
        canvas.GS_DrawPoly(camera, polyPoints, paint, false);*/
    }
    
    public void CalculateShipOrbit(List<Planet> planets)
    {
        if (Parent == null)
            throw new NullReferenceException($"Ship \"{Name}\" has no parent");
        
        ScientificDecimal? parentSOIRadius = Parent.Orbit?.SphereOfInfluenceRadius;
        if (parentSOIRadius != null)
            if ((Position - Parent.Position).Magnitude() > parentSOIRadius)
                Parent = Parent.Parent ?? throw new ArgumentException("Parent with SOI has no parent itself.");
        
        foreach (Planet planet in planets)
        {
            if (planet == Parent) continue;
            ScientificDecimal? bodySOIRadius = planet.Orbit?.SphereOfInfluenceRadius;
            if (bodySOIRadius != null)
                if ((Position - planet.Position).Magnitude() < bodySOIRadius)
                    Parent = planet;
        }
        
        Orbit = CalculateOrbit(false);
    }
}