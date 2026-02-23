using System.Collections.Generic;

namespace OrbitGame;

/// <summary>
/// Class which sets the origin body to where most physics calculations would take place to prevent floating point errors.
/// </summary>
public static class OriginBody
{
    public static Body? Body = null;

    public static void ResetOrigin(List<KinematicObject> kinObjects)
    {
        if (Body == null) return;
        foreach (var obj in kinObjects)
        {
            if (obj == Body) continue;
            obj.ResetOrigin(Body.Position);
        }

        Body.Position = SD_Vector2.Zero;
    }
}