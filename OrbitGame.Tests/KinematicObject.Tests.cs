namespace OrbitGame.Tests;

public class KinematicObject_Tests
{
    private class TestKinematicObject(string identifier, SpatialInfo spatialInfo)
        : KinematicObject(identifier, spatialInfo)
    {
        public class TestTemplate : KinematicObjectTemplate
        {
            public TestKinematicObject CreateTestInstance(string identifier, SpatialInfo spatialInfo)
            {
                TestKinematicObject instance = new TestKinematicObject(identifier, spatialInfo);
                AddInstance(identifier, instance);
                return instance;
            }
        }
    }
    
    private readonly TestKinematicObject.TestTemplate _testKinObjTemplate = new();
    private readonly TestKinematicObject _testKinematicObject1;
    private readonly TestKinematicObject _testKinematicObject2;
    private readonly SD_Vector2 _testVector = new(5, 5);
    
    public KinematicObject_Tests()
    {
        _testKinematicObject1 = _testKinObjTemplate.CreateTestInstance("Object 1", new(
            position: new SD_Vector2(5, 0),
            angle: Math.PI / 2
        ));

        _testKinematicObject2 = _testKinObjTemplate.CreateTestInstance("Object 2", new(
            position: new SD_Vector2(0, -5),
            angle: -Math.PI / 2
        ));
    }

    [Fact]
    public void KinematicObject_ObjectToWorldSpaceMethod()
    {
        Assert.Equal(new SD_Vector2(0, 5), _testKinematicObject1.ObjectToWorldSpace(_testVector));
    }

    [Fact]
    public void KinematicObject_WorldToObjectSpaceMethod()
    {
        Assert.Equal(new SD_Vector2(5, 0), _testKinematicObject1.WorldToObjectSpace(_testVector));
    }

    [Fact]
    public void KinematicObject_ObjectToObjectSpaceMethod()
    {
        Assert.Equal(new SD_Vector2(-10, 0), _testKinematicObject1.ObjectToObjectSpace(_testVector, _testKinematicObject2));
    }

    [Fact]
    public void KinematicObject_ResetOriginMethod()
    {
        _testKinematicObject1.ResetOrigin(_testVector);
        Assert.Equal(new SD_Vector2(0, -5), _testKinematicObject1.Position);
    }
}