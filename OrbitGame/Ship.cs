using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

namespace OrbitGame;

public class Ship : Body, IGameDrawable
{
    private ScientificDecimal _maximumRadius;

    private Ship(
        ShipTemplate template,
        string identifier,
        ScientificDecimal mass,
        SpatialInfo spatialInfo,
        Color colour,
        Planet parent)
        : base(template, identifier, mass, spatialInfo, colour, parent)
    {
        Collider.CalculateInertia(mass);
    }

    public override void Draw(GraphicsDevice graphicsDevice, Camera camera, Effect effect)
    {
        if (Position.X < camera.Left - _maximumRadius) return;
        if (Position.X > camera.Right + _maximumRadius) return;
        if (Position.Y > camera.Top + _maximumRadius) return;
        if (Position.Y < camera.Bottom - _maximumRadius) return;
        
        Vector2 screenPosition = camera.ConvertToScreenCoordinates(Position);
        if (camera.ConvertToScreenDistance(_maximumRadius) > 1)
        {
            Vector2 scale = new((float)(Options.ScreenSize.height / camera.Height),
                (float)(Options.ScreenSize.width / camera.Width));
            Matrix transform = Matrix.CreateScale(new Vector3(scale.X, scale.Y, 1)) *
                               Matrix.CreateRotationZ(-(float)(Angle + camera.Angle)) *
                               Matrix.CreateTranslation(new Vector3(screenPosition.X, screenPosition.Y, 0));
            Mesh.Draw(graphicsDevice, effect, transform, new()
            {
                { "colour", Colour.ToVector4() }
            });
        }
        else
        {
            graphicsDevice.DrawLine(screenPosition, screenPosition + new Vector2(10, 0), Colour);
            graphicsDevice.DrawLine(screenPosition, screenPosition + new Vector2(0, 10), Colour);
            graphicsDevice.DrawLine(screenPosition, screenPosition + new Vector2(-10, 0), Colour);
            graphicsDevice.DrawLine(screenPosition, screenPosition + new Vector2(0, -10), Colour);
        }
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
    public class ShipTemplate(SD_Vector2[] shipVertices, Material material)
        : KinematicObjectTemplate(new PolyMesh(shipVertices), new ConvexCollider(shipVertices), material)
    {
        public Ship CreateInstance(
            string identifier, 
            ScientificDecimal mass, 
            SD_Vector2 position, 
            SD_Vector2 velocity, 
            double angle, 
            double angularVelocity,
            Color colour,
            Planet parent
            )
        {
            SpatialInfo spatialInfo = new SpatialInfo(position, velocity, SD_Vector2.Zero, angle, angularVelocity);
            Ship ship = new Ship(this, identifier, mass, spatialInfo, colour, parent)
            {
                Angle = angle,
                AngularVelocity = angularVelocity,
                _maximumRadius = shipVertices.Select(x => x.Magnitude()).Max()
            };
            AddInstance(identifier, ship);
            return ship;
        }
    }
}