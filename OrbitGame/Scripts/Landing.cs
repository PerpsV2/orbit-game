namespace OrbitGame;

/// <summary>
/// Struct containing information about the landing of a ship on an object
/// </summary>
public record struct Landing
{
    public KinematicObject Parent { get; }

    public SD_Vector2 RelativePosition { get; private set; }

    public double RelativeAngle { get; private set; }
    
    public Landing(KinematicObject Parent,
        SD_Vector2 RelativePosition,
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