using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

namespace OrbitGame;

public class Ship : Body, IGameDrawable
{
    public readonly IMesh Mesh;
        
    private Ship(
        string identifier,
        ScientificDecimal mass,
        Material material, 
        SD_Vector2 position,
        SD_Vector2 velocity, 
        Color colour, 
        Planet parent,
        SD_Vector2[] points,
        IMesh mesh)
        : base(identifier, mass, material, position, velocity, colour, parent)
    {
        Position += parent.Position;
        Velocity += parent.Velocity;
        Mesh = mesh;
        Collider = new ConvexCollider(points, this);
    }

    public override void Draw(GraphicsDevice graphicsDevice, Camera camera, Effect effect)
    {
        Vector2 scale = new((float)(Options.ScreenSize.height / camera.Height), (float)(Options.ScreenSize.width / camera.Width));
        Vector2 screenPosition = camera.ConvertToScreenCoordinates(Position);
        Matrix transform = Matrix.CreateScale(new Vector3(scale.X, scale.Y, 1)) *
                           Matrix.CreateRotationZ(-(float)(Angle + camera.Angle)) *
                           Matrix.CreateTranslation(new Vector3(screenPosition.X, screenPosition.Y, 0));
        Mesh.Draw(graphicsDevice, effect, transform, new()
        {
            {"colour", Colour.ToVector4()}
        });

        graphicsDevice.DrawLine(screenPosition, screenPosition + new Vector2(10, 0), Colour);
        graphicsDevice.DrawLine(screenPosition, screenPosition + new Vector2(0, 10), Colour);
        graphicsDevice.DrawLine(screenPosition, screenPosition + new Vector2(-10, 0), Colour);
        graphicsDevice.DrawLine(screenPosition, screenPosition + new Vector2(0, -10), Colour);
    }

    public override void DrawCollider(GraphicsDevice graphicsDevice, Camera camera, Effect effect)
    {
        throw new NotImplementedException();
    }
    
    public void CalculateShipOrbit(List<Planet> planets)
    {
        if (Parent == null)
            throw new NullReferenceException($"Ship \"{Identifier}\" has no parent");
        
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
    
    /// <summary>
    /// Optimize the creation of multiple similar ships by using the same object for multiple instance's properties
    /// </summary>
    public class ShipTemplate(SD_Vector2[] points, GraphicsDevice graphics, Material material, ScientificDecimal mass)
        : KinematicObjectTemplate(new PolyMesh(points), material)
    {
        private readonly Material _material = material;

        public Ship Instantiate(string identifier, SD_Vector2 position, SD_Vector2 velocity, Color colour, Planet parent)
        {
            Mesh.GenerateBuffers(graphics);
            Ship instance = new Ship(identifier, mass, _material, position, velocity, colour, parent, points, Mesh);
            Instances.Add(identifier, instance);
            return instance;
        }
    }
}