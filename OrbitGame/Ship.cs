using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

namespace OrbitGame;

public class Ship : Body
{
    private readonly PolyMesh _polyMesh;
    
    public Ship(
        ScientificDecimal mass, SD_Vector2 position, SD_Vector2 velocity, Material material, Color colour, Planet parent,
        SD_Vector2[] mesh, string name)
        : base(mass, position, velocity, colour, name, parent)
    {
        LinkedList<int> colliderIndices = SD_Vector2.GetConvexHullIndices(mesh);
        SD_Vector2[] meshPoints = colliderIndices.Select(x => mesh[x]).ToArray();
        _polyMesh = new PolyMesh(OrbitGame.Graphics, colour);
        _polyMesh.SetBuffersPoly(meshPoints);
        Collider = new ConvexCollider(meshPoints, this, material);
    }

    public override void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        _polyMesh.DrawMesh(camera, this);

        Vector2 screenPosition = camera.ConvertToScreenCoordinates(Position);
        spriteBatch.DrawLine(screenPosition, screenPosition + new Vector2(10, 0), Colour);
        spriteBatch.DrawLine(screenPosition, screenPosition + new Vector2(0, 10), Colour);
        spriteBatch.DrawLine(screenPosition, screenPosition + new Vector2(-10, 0), Colour);
        spriteBatch.DrawLine(screenPosition, screenPosition + new Vector2(0, -10), Colour);
    }

    public override void DrawCollider(SpriteBatch canvas, Camera camera)
    {
        throw new NotImplementedException();
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