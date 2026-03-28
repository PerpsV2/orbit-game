using System;

namespace OrbitGame;

public class ManeuverNode(KeplerOrbitPoint point, Vec2<SDecimal> velocity)
{
    private readonly KeplerOrbitPoint _point = point;
    private readonly Vec2<SDecimal> _velocity = velocity;
    public double TrueAnomaly => _point.TrueAnomaly;

    public KeplerOrbit GenerateAppliedKeplerOrbit()
    {
        return _point.ConicPath.Orbit ?? throw new NullReferenceException();
    }
}