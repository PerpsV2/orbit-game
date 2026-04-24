using Microsoft.Xna.Framework;

namespace OrbitGame;

public class TestTerrainCollider
{
    private (double angle, SDecimal distance)[] _elevationPoints;
    
    public TestTerrainCollider((double, SDecimal)[] elevationPoints)
    {
        _elevationPoints = elevationPoints;
    }

    public void IntersectsWith(SpatialInfo reference, SpatialInfo incident)
    {
        Vec2Double relPos = (Vec2Double)(reference.Position - incident.Position);
        
        DrawDebug.Add(() =>
        {
            Camera cam = OrbitGame.Camera;
            IGraphicsHandler g = OrbitGame.Graphics;

            for (int i = 0; i < _elevationPoints.Length; ++i)
            {
                int nextIndex = (i + 1) % _elevationPoints.Length;
                g.SD_DrawPoint(cam, incident.Position + Vec2<SDecimal>.FromPolar(_elevationPoints[i].angle, _elevationPoints[i].distance), Color.Red);
                g.SD_DrawLine(cam,
                    incident.Position + Vec2<SDecimal>.FromPolar(_elevationPoints[i].angle, _elevationPoints[i].distance),
                    incident.Position + Vec2<SDecimal>.FromPolar(_elevationPoints[nextIndex].angle, _elevationPoints[nextIndex].distance),
                    Color.Red);
            }
        });
    }
}