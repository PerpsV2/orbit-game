namespace OrbitGame;

/// <summary>
/// Struct containing information about the landing of a ship on an object
/// </summary>
public record struct Landing
{
    public KinematicObject Parent { get; }

    public Vec2Double RelativePosition { get; private set; }

    public double RelativeAngle { get; private set; }
    
    public Landing(KinematicObject Parent,
        Vec2Double RelativePosition,
        double RelativeAngle)
    {
        this.Parent = Parent;
        this.RelativePosition = RelativePosition;
        this.RelativeAngle = RelativeAngle;
        OriginBody.OnResetOrigin += Landing_OnResetOrigin;
    }

    /// <summary>
    /// Update the relative position after resetting the world origin.
    /// </summary>
    private void Landing_OnResetOrigin(object? obj, OriginBodyEventArgs e)
    {
        RelativePosition += e.PositionOffset;
    }
}