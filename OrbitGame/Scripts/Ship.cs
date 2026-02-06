using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

namespace OrbitGame;

public class Ship : Body, IGameDrawable
{
    private ScientificDecimal _maximumRadius;
    private OrbitMesh _orbitMesh;

    private Ship(
        string identifier,
        SpatialInfo spatialInfo,
        ScientificDecimal mass,
        Color colour,
        Planet parent,
        
        IMesh mesh,
        CompactCollider collider,
        Material material,
        OrbitMesh orbitMesh,
        ScientificDecimal maximumRadius
        )
        : base(identifier, spatialInfo, mass, colour, parent, mesh, collider, material)
    {
        _maximumRadius = maximumRadius;
        _orbitMesh = orbitMesh;
        Collider.CalculateInertia(mass);
    }

    public override void Draw(GraphicsDevice graphicsDevice, Camera camera)
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
            Mesh.Draw(graphicsDevice, transform, new()
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
        //DrawOrbitalPathLRL(graphicsDevice, camera, _orbitMesh);
    }

    public override void DrawCollider(GraphicsDevice graphicsDevice, Camera camera)
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
    
    public class ShipTemplate : KinematicObjectTemplate
    {
        private readonly IMesh _mesh;
        private readonly CompactCollider _collider;
        private readonly OrbitMesh _orbitMesh = new();
        private readonly ScientificDecimal _maximumRadius;
        private readonly Material _material;
        
        public ShipTemplate(SD_Vector2[] shipVertices, Material material)
        {
            _material = material;
            _mesh = new PolyMesh(shipVertices);
            _collider = new ConvexCollider(shipVertices);
            _maximumRadius = shipVertices.Select(x => x.Magnitude()).Max();
            _mesh.GenerateBuffers();
            _orbitMesh.GenerateBuffers();
        }

        public Ship CreateInstance(
            string identifier, 
            SpatialInfo spatialInfo,
            ScientificDecimal mass,
            Color colour,
            Planet parent
            )
        {
            Ship ship = new Ship(identifier, spatialInfo, mass, colour, parent, _mesh, _collider, _material, _orbitMesh,
                _maximumRadius);
            AddInstance(identifier, ship);
            return ship;
        }
    }
}