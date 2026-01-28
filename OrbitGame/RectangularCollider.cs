using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

/// <summary>
/// Rectangular collider is unaffected by rotations
/// </summary>
public class RectangularCollider : CompactCollider, ICollider
{
    // Values are the signed ordinates of the vertex points of the collider
    private readonly ScientificDecimal _top;
    private readonly ScientificDecimal _right;
    private readonly ScientificDecimal _bottom;
    private readonly ScientificDecimal _left;

    public SD_Vector2 TopRight => new(_right, _top);
    public SD_Vector2 TopLeft => new(_left, _top);
    public SD_Vector2 BottomRight => new(_right, _bottom);
    public SD_Vector2 BottomLeft => new(_left, _bottom);
    
    public RectangularCollider(SD_Vector2 center,
        ScientificDecimal width,
        ScientificDecimal height, 
        KinematicObject parent,
        Material material) : base(parent, material)
    {
        _top = center.Y + height / 2;
        _right = center.X + width / 2;
        _bottom = center.Y - height / 2;
        _left = center.X - width / 2;
        Inertia = parent.Mass * (height * height + width * width) / 12;
    }
    
    public RectangularCollider(SD_Vector2 topRight, SD_Vector2 bottomLeft, KinematicObject parent, Material material)
        : this(
            (topRight + bottomLeft) / 2, 
            topRight.X - bottomLeft.X, 
            topRight.Y - bottomLeft.Y, 
            parent, material
        ) 
    { }

    public override RectangularCollider GetBoundingBox() => this;

    protected override bool NearsWith(ICollider collider)
    {
        if (collider is RectangularCollider rect)
            return Position.Y + _top >= rect.Position.Y + rect._bottom && 
                   Position.Y + _bottom <= rect.Position.Y + rect._top && 
                   Position.X + _right >= rect.Position.X + rect._left && 
                   Position.X + _left <= rect.Position.X + rect._right;
        // convert the other collider into a rectangular collider if it is not one
        else if (collider is CompactCollider compact)
            return NearsWith(compact.GetBoundingBox());
        else throw new ArgumentException("RectangularCollider cannot be near invalid collider type");
    }

    protected override PointCollision IntersectsWith(SD_Vector2 point)
    {
        if (IsEmpty()) return new(false);
        point -= Parent.Position;
        return new(point.Y <= _top && point.Y >= _bottom && point.X <= _right && point.X >= _left);
    }
    
    protected override PhysicsCollision? IntersectsWith(CircularCollider collider)
    {
        throw new NotImplementedException();
    }

    protected override PhysicsCollision? IntersectsWith(ConvexCollider collider)
        => ((PhysicsCollision?)collider.IntersectsWith(this))?.GetInverse() ?? null;

    protected override PhysicsCollision? IntersectsWith(RectangularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;

        HashSet<SD_Vector2> collisionManifold = new();
        SD_Vector2? minPenetrationVector = null;
        SD_Vector2[] penetrationVectors = new SD_Vector2[4];
        if (Position.Y + _bottom <= collider.Position.Y + collider._top && 
            Position.Y + _top >= collider.Position.Y + collider._bottom)
        {
            penetrationVectors[0] = new(0, collider.Position.Y + collider._top - Position.Y - _bottom);
            penetrationVectors[1] = new(0, Position.Y + _top - collider.Position.Y - collider._bottom);
            penetrationVectors[2] = new(collider.Position.X + collider._right - Position.X - _left, 0);
            penetrationVectors[3] = new(Position.X + _right - collider.Position.X - _left, 0);
            minPenetrationVector = penetrationVectors.MinBy(x => x.Magnitude());
        }

        if (minPenetrationVector == null) return null;

        (SD_Vector2 a, SD_Vector2 b)[] edges = [
            (TopLeft, TopRight),
            (BottomLeft, TopLeft),
            (BottomRight, BottomLeft),
            (TopRight, BottomRight)
        ];
        
        (SD_Vector2 a, SD_Vector2 b) incidentEdge = edges.MinBy(x =>
            Math.Abs((x.a - x.b).GetPrincipalAngle() - minPenetrationVector.Value.GetPrincipalAngle() - Math.PI / 2));
        collisionManifold.Add(incidentEdge.a);
        collisionManifold.Add(incidentEdge.b);

        return new PhysicsCollision(this, collider, collisionManifold, minPenetrationVector.Value);
    }

    public override bool IsEmpty() =>
        _top == _bottom || _left == _right;
}