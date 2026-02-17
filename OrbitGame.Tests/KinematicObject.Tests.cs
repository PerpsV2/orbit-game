namespace OrbitGame.Tests;

public class KinematicObject_Tests
{
    private class TestTemplate : KinematicObject.KinematicObjectTemplate;

    private class TestKinematicObject(string identifier, SpatialInfo spatialInfo) 
        : KinematicObject(identifier, spatialInfo);
    
    private TestTemplate _testKinObjTemplate = new();

    [Fact]
    public void KinematicObject_ObjectToWorldSpaceMethod()
    {
        
    }

    [Fact]
    public void KinematicObject_WorldToObjectSpaceMethod()
    {
        
    }

    [Fact]
    public void KinematicObject_ObjectToObjectSpaceMethod()
    {
        
    }

    [Fact]
    public void KinematicObject_ResetOriginMethod()
    {
        
    }
}