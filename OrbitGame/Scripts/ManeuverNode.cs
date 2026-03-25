using System;

namespace OrbitGame;

public class ManeuverNode
{
    private KeplerOrbitPoint _point;
    private DVector2<SDecimal> _velocity;

    public KeplerOrbit GenerateAppliedKeplerOrbit()
    {
        return _point.ConicSection.Orbit ?? throw new NullReferenceException();
    }
}