namespace OrbitGame.Tests;

public class RectangularCollider_Tests
{
    private class TestKinematicObject(ScientificDecimal mass, Vector2 position, Vector2 velocity)
        : KinematicObject("", mass, position, velocity);
}