namespace OrbitGame.Tests;

public class KinematicObject_Tests
{
    private class TestKinematicObject(ScientificDecimal mass, Vector2 position, Vector2 velocity, double angle)
        : KinematicObject("", mass, position, velocity, angle);

    private readonly TestKinematicObject _kinematicObject1 = new (
        0, new Vector2(5, 2), Vector2.Zero, Math.PI / 2
        );
    private readonly TestKinematicObject _kinematicObject2 = new (
        0, new Vector2(-3, -3), Vector2.Zero, Math.PI
    );
    
    [Fact]
    public void KinematicObject_ObjectToWorldSpaceMethod()
    {
        AssertExtensions.Equal(new Vector2(3, 5), _kinematicObject1.ObjectToWorldSpace(new Vector2(3, 2)));
    }
    
    [Fact]
    public void KinematicObject_WorldToObjectSpaceMethod()
    {
        AssertExtensions.Equal(new Vector2(3, 2), _kinematicObject1.WorldToObjectSpace(new Vector2(3, 5)));
    }

    [Fact]
    public void KinematicObject_ObjectToObjectSpaceMethod()
    {
        AssertExtensions.Equal(new Vector2(-6, -8), 
            _kinematicObject1.ObjectToObjectSpace(new Vector2(3, 2), _kinematicObject2));
    }
}