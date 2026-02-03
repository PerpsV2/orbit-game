using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

/// <summary>
/// Rectangular collider is unaffected by rotations
/// </summary>
public class RectangularCollider : CompactCollider
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
    
    public RectangularCollider(SD_Vector2 center, ScientificDecimal width, ScientificDecimal height)
    {
        _top = center.Y + height / 2;
        _right = center.X + width / 2;
        _bottom = center.Y - height / 2;
        _left = center.X - width / 2;
        // parent.Mass * (height * height + width * width) / 12;
    }
    
    public RectangularCollider(SD_Vector2 topRight, SD_Vector2 bottomLeft)
        : this(
            (topRight + bottomLeft) / 2, 
            topRight.X - bottomLeft.X, 
            topRight.Y - bottomLeft.Y
        ) 
    { }
    
    public override void CalculateInertia(ScientificDecimal mass)
    {
        ScientificDecimal height = _top - _bottom;
        ScientificDecimal width = _right - _left;
        Inertia = mass * (height * height + width * width) / 12;
    }

    public override RectangularCollider GetBoundingBox() => this;

    public override bool NearsWith(CompactCollider collider, SD_Vector2 relPosition, double relAngle)
    {
        if (collider is RectangularCollider rect)
            return _top >= relPosition.Y + rect._bottom && _bottom <= relPosition.Y + rect._top && 
                   _right >= relPosition.X + rect._left && _left <= relPosition.X + rect._right;
        // convert the other collider into a rectangular collider if it is not one
        return NearsWith(collider.GetBoundingBox(), relPosition, relAngle);
    }

    public override PointCollision IntersectsWith(SD_Vector2 point, SpatialInfo spatial)
    {
        if (IsEmpty()) return new(false);
        point -= spatial.Position;
        return new(point.Y <= _top && point.Y >= _bottom && point.X <= _right && point.X >= _left);
    }
    
    protected override PhysicsCollision? IntersectsWith(CircularCollider collider, SpatialInfo referenceSpatial, SpatialInfo incidentSpatial)
    {
        throw new NotImplementedException();
    }

    protected override PhysicsCollision? IntersectsWith(ConvexCollider collider, SpatialInfo referenceSpatial, SpatialInfo incidentSpatial)
        => throw new NotImplementedException(); // collider.IntersectsWith(this, -relPosition, -relAngle)?.GetInverse() ?? null;

    protected override PhysicsCollision? IntersectsWith(RectangularCollider collider, SpatialInfo referenceSpatial, SpatialInfo incidentSpatial)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;

        HashSet<SD_Vector2> collisionManifold = new();
        SD_Vector2? minPenetrationVector = null;
        SD_Vector2[] penetrationVectors = new SD_Vector2[4];
        if (referenceSpatial.Position.Y + _bottom <= incidentSpatial.Position.Y + collider._top && 
            referenceSpatial.Position.Y + _top >= incidentSpatial.Position.Y + collider._bottom)
        {
            penetrationVectors[0] = new(0, incidentSpatial.Position.Y + collider._top - referenceSpatial.Position.Y - _bottom);
            penetrationVectors[1] = new(0, referenceSpatial.Position.Y + _top - incidentSpatial.Position.Y - collider._bottom);
            penetrationVectors[2] = new(incidentSpatial.Position.X + collider._right - referenceSpatial.Position.X - _left, 0);
            penetrationVectors[3] = new(referenceSpatial.Position.X + _right - incidentSpatial.Position.X - _left, 0);
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

        return new PhysicsCollision(referenceSpatial, incidentSpatial, collisionManifold, minPenetrationVector.Value);
    }

    public override bool IsEmpty() =>
        _top == _bottom || _left == _right;
}