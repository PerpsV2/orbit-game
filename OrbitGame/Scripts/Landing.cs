namespace OrbitGame;

public record struct Landing(
    KinematicObject Parent,
    SD_Vector2 RelativePosition,
    double RelativeAngle
)
{
    public void ResetOrigin(SD_Vector2 origin)
    {
        RelativePosition -= origin;
    }
}