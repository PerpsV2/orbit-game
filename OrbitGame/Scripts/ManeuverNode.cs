using System;

namespace OrbitGame;

public class ManeuverNode
{
    private KeplerOrbitPoint _point;
    private DVector2<SDecimal> _velocity;

    public KeplerOrbit GenerateAppliedKeplerOrbit()
    {
        return _point.Path.Orbit ?? throw new NullReferenceException();
    }
}