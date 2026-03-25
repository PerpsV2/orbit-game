using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public class PatchedConicPath
{
    public List<ManeuverNode> ManeuverNodes { get; set; } = [];
    public readonly OrbitConicSection[] Conics;

    public PatchedConicPath(OrbitMesh mesh, Color colour)
    {
        Conics = new OrbitConicSection[Options.PatchedConicDetail];
        for (int i = 0; i < Conics.Length; i++)
        {
            Conics[i] = new OrbitConicSection(mesh, colour);
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
}