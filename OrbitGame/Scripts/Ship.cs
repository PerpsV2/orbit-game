using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Player-controllable celestial object.
/// </summary>
public class Ship : Body, IGameDrawable
{
    private readonly SDecimal _maximumRadius;
    
    private Vec2<SDecimal> ArtificialAcceleration { get; set; }
    
    public bool DrawOrbitalPath { get; set; }
    private bool _mouseDetectionEnabled = true;

    public bool MouseDetectionEnabled
    {
        get => _mouseDetectionEnabled;
        set
        {
            _mouseDetectionEnabled = value;
            OrbitPath.MouseDetectionEnabled = value;
        }
    }
    
    public bool MarkedForRemoval { get; set; }
    
    public Landing? LandingState { get; private set; }

    private Ship(
        string identifier,
        SpatialInfo spatialInfo,
        ObjectInfo objectInfo,
        SDecimal maximumRadius,
        SDecimal mass,
        Color colour,
        Planet parent,
        ShipTemplate template)
        : base(identifier, spatialInfo, objectInfo, mass, colour, parent, template)
    {
        _maximumRadius = maximumRadius;
        Collider.CalculateInertia(mass);
    }

    protected override void Body_UpdateFrame(object? e, EventArgs args)
    {
        base.Body_UpdateFrame(e, args);
        ArtificialAcceleration = Vec2<SDecimal>.Zero;
    }

    public void Draw()
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;
        
        Vector2 screenPosition = camera.ConvertToScreenCoordinates(Position);
        float screenDistance = camera.ConvertToScreenDistance(_maximumRadius);
        if (LandingState == null && DrawOrbitalPath) OrbitPath.Draw();
        if ((Position - camera.Position).MagnitudeSquared() - 4 * _maximumRadius * _maximumRadius > camera.MaximumRadiusSquared) 
            return;
        if (screenDistance > 1)
        {
            Vector2 scale = new((float)(Options.ScreenSize.height / camera.Height),
                (float)(Options.ScreenSize.width / camera.Width));
            Matrix transform = Matrix.CreateScale(new Vector3(scale.X, scale.Y, 1)) *
                               Matrix.CreateRotationZ(-(float)(Angle + camera.Angle)) *
                               Matrix.CreateTranslation(new Vector3(screenPosition.X, screenPosition.Y, 0));
            graphicsDevice.DrawMesh(Mesh, transform, new()
            {
                { "Colour", Colour.ToVector4() }
            });
        }
        if (screenDistance < 10)
        {
            double iconAngle = -Angle - camera.Angle;
            float alpha = Utils.Clamp(1 - camera.ConvertToScreenDistance(_maximumRadius) / 10, 0, 1);
            Color colour = Colour * alpha;
            graphicsDevice.DrawLine(
                screenPosition + (Vector2)Vec2<SDecimal>.RotatePoint(new(10, -5), iconAngle), 
                screenPosition + (Vector2)Vec2<SDecimal>.RotatePoint(new(10, 5), iconAngle), colour);
            graphicsDevice.DrawLine(
                screenPosition + (Vector2)Vec2<SDecimal>.RotatePoint(new(10, 0), iconAngle), 
                screenPosition + (Vector2)Vec2<SDecimal>.RotatePoint(new(-10, 0), iconAngle), colour);
            graphicsDevice.DrawLine(
                screenPosition + (Vector2)Vec2<SDecimal>.RotatePoint(new(5, -8), iconAngle), 
                screenPosition + (Vector2)Vec2<SDecimal>.RotatePoint(new(-10, 0), iconAngle), colour);
            graphicsDevice.DrawLine(
                screenPosition + (Vector2)Vec2<SDecimal>.RotatePoint(new(5, 8), iconAngle), 
                screenPosition + (Vector2)Vec2<SDecimal>.RotatePoint(new(-10, 0), iconAngle), colour);
        }
    }

    public void DrawCollider()
    {
        throw new NotImplementedException();
    }
    
    public override void GenerateOrbitPath(SDecimal time)
    {
        if (Parent == null)
            throw new NullReferenceException($"Ship \"{Identifier}\" has no parent");
        
        SDecimal? parentSOIRadius = Parent.OrbitPath.GetSphereOfInfluenceRadius();
        if (parentSOIRadius != null)
            if ((Position - Parent.Position).Magnitude() > parentSOIRadius)
                Parent = Parent.Parent ?? throw new ArgumentException("Parent with SOI has no parent itself.");
        
        foreach (Planet planet in KinematicObjectTemplate.AllInstances.Values.OfType<Planet>())
        {
            if (planet == Parent) continue;
            SDecimal? bodySOIRadius = planet.OrbitPath.GetSphereOfInfluenceRadius();
            if (bodySOIRadius != null)
                if ((Position - planet.Position).Magnitude() < bodySOIRadius)
                    Parent = planet;
        }
        
        base.GenerateOrbitPath(time);
    }

    public void UpdatePosition_Landed()
    {
        if (LandingState == null)
            throw new NullReferenceException("Ship is not landed");
        Landing landing = LandingState.Value;
        Position = landing.Parent.Position + Vec2<SDecimal>.RotatePoint(landing.RelativePosition, landing.Parent.Angle);
        Velocity = landing.Parent.Velocity;
        Angle = landing.Parent.Angle + landing.RelativeAngle;
    }
    
    public void SetLandingState(KinematicObject parent)
    {
        LandingState = new Landing(parent, Position - parent.Position, Angle - parent.Angle);
    }

    private void DisturbLandingState()
    {
        LandingState = null;
    }

    public void ApplyThrust(Vec2<SDecimal> thrust, Vec2<SDecimal> position)
    {
        thrust = Vec2<SDecimal>.RotatePoint(thrust, Angle);
        position = Vec2<SDecimal>.RotatePoint(position, Angle);
        SDecimal torque = Vec2<SDecimal>.Cross(thrust, position).Z;
        AngularAcceleration += (double)(torque / Mass);
        ArtificialAcceleration += thrust / Mass;
        DisturbLandingState();
    }

    public override Vec2<SDecimal> CalculateNetAcceleration()
    {
        return base.CalculateNetAcceleration() + ArtificialAcceleration;
    }
    
    public class ShipTemplate : BodyTemplate
    {
        public new static Dictionary<string, Ship> AllInstances { get; } = new();
        
        private readonly IMesh _mesh;
        private readonly CompactCollider _collider;
        private readonly OrbitMesh _orbitMesh = new();
        private readonly double _maximumRadius;
        private readonly Material _material;
        
        public ShipTemplate(Vec2Double[] shipVertices, Material material)
        {
            _material = material;
            _mesh = new PolyMesh(shipVertices);
            _collider = new ConvexCollider(shipVertices);
            _maximumRadius = shipVertices.Select(x => x.Magnitude()).Max();
        }

        public Ship CreateInstance(
            string identifier, 
            SpatialInfo spatialInfo,
            SDecimal mass,
            Color colour,
            Planet parent)
        {
            ObjectInfo objectInfo = new ObjectInfo(_mesh, _collider, _material) {
                OrbitMesh = _orbitMesh
            };
            Ship ship = new Ship(identifier, spatialInfo, objectInfo, _maximumRadius, mass, colour, parent, this);
            AddInstance(identifier, ship);
            return ship;
        }
        
        protected override void AddInstance(string identifier, KinematicObject instance)
        {
            base.AddInstance(identifier, instance);
            if (!AllInstances.TryAdd(identifier, (Ship)instance))
                throw new ArgumentException($"KinematicObject with identifier '{identifier}' has already been added");
        }
    }
}