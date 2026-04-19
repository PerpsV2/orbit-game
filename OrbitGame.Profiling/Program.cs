using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OrbitGame;
using OrbitGame.Profiling;

var summary = BenchmarkRunner.Run<Benchmarker>();

public class Benchmarker
{
    private class TestKinematicObject(string identifier, SpatialInfo spatialInfo)
        : KinematicObject(identifier, spatialInfo)
    {
        public class TestTemplate : KinematicObjectTemplate
        {
            public new static Dictionary<string, TestKinematicObject> AllInstances { get; } = new();
            
            protected override void AddInstance(string identifier, KinematicObject instance)
            {
                base.AddInstance(identifier, instance);
                if (!AllInstances.TryAdd(identifier, (TestKinematicObject)instance))
                    throw new ArgumentException($"KinematicObject with identifier '{identifier}' has already been added");
            }

            public override void DestroyInstance(string identifier)
            {
                base.DestroyInstance(identifier);
                AllInstances.Remove(identifier);
                Instances.Remove(identifier);
            }
            
            public TestKinematicObject CreateTestInstance(string identifier, SpatialInfo spatialInfo)
            {
                TestKinematicObject instance = new TestKinematicObject(identifier, spatialInfo);
                AddInstance(identifier, instance);
                return instance;
            }
        }
    }
    
    private readonly TestKinematicObject.TestTemplate _template = new();
    private readonly int _index;
    private readonly Random _rnd = new();
    private readonly List<KinematicObject> _points;
    private readonly Vec2<SDecimal> _queryPosition = new(50, 50);
    private readonly SDecimal _radius = 10;
    
    public Benchmarker()
    {
        KinematicObject.KinematicObjectTemplate.DestroyAll();
        _points = new();
        for (int i = 0; i < 10000; ++i)
        {
            _points.Add(_template.CreateTestInstance("Test Instance " + _index, new SpatialInfo(
                position: new Vec2<SDecimal>(_rnd.NextDouble() * 100, _rnd.NextDouble() * 100)
            )));
            _index++;
        }
    }
    
    [Benchmark]
    public void Operation1()
    {
        SpaceHierarchy.KDTree testHierarchy = new SpaceHierarchy.KDTree(_points);
        testHierarchy.GetObjectsInRadius(_queryPosition, 10);
        /*_testHierarchy.AddKinematicObject(_newObject);
        _testHierarchy.RemoveKinematicObject("Test Instance F");*/
    }
}