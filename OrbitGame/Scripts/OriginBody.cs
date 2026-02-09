using System.Collections.Generic;

namespace OrbitGame;

/// <summary>
/// To prevent floating point errors, set the origin body to where most physics calculations would take place.
/// </summary>
public static class OriginBody
{
    public static Body? Body = null;

    public static void ResetOrigin(IEnumerable<Body> bodies)
    {
        if (Body == null) return;
        foreach (var body in bodies)
        {
            if (body == Body) continue;
            body.ResetOrigin(Body.Position);
        }

        Body.Position = SD_Vector2.Zero;
    }
}