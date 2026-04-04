using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        OrbitGame.UpdateFrame += PatchedConicPath_UpdateFrame;
    }

    public void Draw()
    {
        foreach (var orbitPath in Conics)
            orbitPath.Draw();

        foreach (var maneuverNode in ManeuverNodes)
        {
            maneuverNode.Draw();
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
        ManeuverNode maneuverNode = new ManeuverNode(point, deltaV);
        ManeuverNodes.Add(maneuverNode);
        KeplerOrbit newTrajectory = maneuverNode.GenerateAppliedKeplerOrbit(currentTime);
        
        int numManeuverNodes = ManeuverNodes.Count;
        point.ConicPath.EndAngle = point.TrueAnomaly;
        Conics[numManeuverNodes].Orbit = newTrajectory;
        Conics[numManeuverNodes].StartAngle = newTrajectory.GetTrueAnomalyFromWorldPosition(point.GetWorldPosition());
    }

    public void AddSOIChange()
    {
        
    }
    
    private void PatchedConicPath_UpdateFrame(object? sender, UpdateEventArgs e)
    {
        for (int i = 0; i < ManeuverNodes.Count; ++i)
        {
            KeplerOrbit newTrajectory = ManeuverNodes[i].GenerateAppliedKeplerOrbit(e.PhysicsTime);
            Conics[i + 1].Orbit = newTrajectory;
            Conics[i].EndAngle = ManeuverNodes[i].TrueAnomaly;
            Conics[i + 1].StartAngle = newTrajectory.GetTrueAnomalyFromWorldPosition(ManeuverNodes[i].Point.GetWorldPosition());
        }
    }
}