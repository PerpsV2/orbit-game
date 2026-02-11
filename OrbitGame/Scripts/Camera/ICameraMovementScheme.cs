namespace OrbitGame;

public interface ICameraMovementScheme
{
    public SD_Vector2 GetAbsolutePosition();
    public double GetAbsoluteAngle();
    public void MovePerpendicular(ScientificDecimal distance);
    public void MoveParallel(ScientificDecimal distance);
}