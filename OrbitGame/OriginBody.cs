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
            if (body != Body) body.Position -= Body.Position;
        Body.Position = Vector2.Zero;
    }
}