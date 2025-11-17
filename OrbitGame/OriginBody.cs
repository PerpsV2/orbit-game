namespace OrbitGame;

public class OriginBody : Body
{
    public static Body Body = new OriginBody();

    private OriginBody() : base(0, Vector2.Zero, Vector2.Zero, String.Empty) { }

    public static void ResetOrigin(IEnumerable<Body> bodies)
    {
        foreach (var body in bodies) body.Position -= Body.Position;
        Body.Position -= Body.Position;
    }
}