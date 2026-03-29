using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public class PatchedConicPath
{
    private List<ManeuverNode> ManeuverNodes { get; } = [];
    public ConicPath[] Conics { get; }

    public PatchedConicPath(OrbitMesh mesh, Color colour)
    {
        Conics = new ConicPath[Options.PatchedConicDetail];
        for (int i = 0; i < Conics.Length; i++)
        {
            Conics[i] = new ConicPath(this, mesh, colour);
        }
    }
    
    public void Draw()
    {
        foreach (var orbitPath in Conics)
        {
            orbitPath.Draw();
        }
    }
    
    public void DrawCollider()
        => Draw();

    public SpatialInfo GetSpatialInfoAtTime(SDecimal time)
    {
        return Conics[0].Orbit?.GetSpatialInfoAtTime(time) ?? new SpatialInfo();
    }

    public SDecimal? GetSphereOfInfluenceRadius()
    {
        return Conics[0].Orbit?.SphereOfInfluenceRadius;
    }

    public bool IsEmpty()
    {
        return Conics[0].Orbit == null;
    }

    public void AddManeuverNode(KeplerOrbitPoint point, Vec2<SDecimal> deltaV, SDecimal currentTime)
    {
        ManeuverNodes.Add(new ManeuverNode(point, deltaV));
        int numManeuverNodes = ManeuverNodes.Count;
        point.ConicPath.EndAngle = point.TrueAnomaly;
        KeplerOrbit pointOrbit = point.ConicPath.Orbit ?? throw new NullReferenceException();
        SDecimal maneuverNodeTime = point.GetNextTime(currentTime);
        SpatialInfo nodeBodySpatialInfo = pointOrbit.GetSpatialInfoAtTime(maneuverNodeTime);
        nodeBodySpatialInfo.Velocity += deltaV;
        Conics[numManeuverNodes].Orbit = new KeplerOrbit(
            pointOrbit.Body, pointOrbit.Parent, nodeBodySpatialInfo, pointOrbit.Parent.SpatialInfo, currentTime
        );
        Conics[numManeuverNodes].StartAngle = point.TrueAnomaly;
        Conics[numManeuverNodes].EndAngle = point.TrueAnomaly + Math.Tau;
    }
}