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
    private SpaceHierarchy.KDTree _testHierarchy;
    private readonly TestKinematicObject _newObject;
    
    public Benchmarker()
    {
        KinematicObject.KinematicObjectTemplate.DestroyAll();
        _testHierarchy = new SpaceHierarchy.KDTree([
            _template.CreateTestInstance("Test Instance A", new SpatialInfo(position: new Vec2<SDecimal>(1, 2))),
            _template.CreateTestInstance("Test Instance B", new SpatialInfo(position: new Vec2<SDecimal>(6, 3))),
            _template.CreateTestInstance("Test Instance C", new SpatialInfo(position: new Vec2<SDecimal>(1, 6))),
            _template.CreateTestInstance("Test Instance D", new SpatialInfo(position: new Vec2<SDecimal>(3, 7))),
            _template.CreateTestInstance("Test Instance E", new SpatialInfo(position: new Vec2<SDecimal>(9, 9))),
        ]);
        _newObject = _template.CreateTestInstance("Test Instance F", new SpatialInfo(position: new Vec2<SDecimal>(3, 9)));
        points = new();
        for (int i = 0; i < 10000; ++i)
        {
            points.Add(_template.CreateTestInstance("Test Instance " + index, new SpatialInfo(
                position: new Vec2<SDecimal>(_rnd.NextDouble() * 100, _rnd.NextDouble() * 100)
            )));
            index++;
        }
    }

    private int index = 0;
    private Random _rnd = new();
    private List<KinematicObject> points;
    
    [Benchmark]
    public void Operation1()
    {
        
        _testHierarchy = new SpaceHierarchy.KDTree(points);
        /*_testHierarchy.AddKinematicObject(_newObject);
        _testHierarchy.RemoveKinematicObject("Test Instance F");*/
    }
}