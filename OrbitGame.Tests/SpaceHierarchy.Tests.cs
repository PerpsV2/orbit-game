using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace OrbitGame.Tests;

public class SpaceHierarchy_Tests
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

    private class TestDerivedKinematicObject(string identifier, SpatialInfo spatialInfo)
        : TestKinematicObject(identifier, spatialInfo)
    {
        public class TestDerivedTemplate : KinematicObjectTemplate
        {
            public new static Dictionary<string, TestDerivedKinematicObject> AllInstances { get; } = new();
            
            protected override void AddInstance(string identifier, KinematicObject instance)
            {
                base.AddInstance(identifier, instance);
                if (!AllInstances.TryAdd(identifier, (TestDerivedKinematicObject)instance))
                    throw new ArgumentException($"KinematicObject with identifier '{identifier}' has already been added");
            }

            public override void DestroyInstance(string identifier)
            {
                base.DestroyInstance(identifier);
                AllInstances.Remove(identifier);
                Instances.Remove(identifier);
            }
            
            public TestDerivedKinematicObject CreateTestInstance(string identifier, SpatialInfo spatialInfo)
            {
                TestDerivedKinematicObject instance = new TestDerivedKinematicObject(identifier, spatialInfo);
                AddInstance(identifier, instance);
                return instance;
            }
        }
    }
    
    private readonly ITestOutputHelper _output;
    private readonly TestKinematicObject.TestTemplate _template = new();
    private readonly TestDerivedKinematicObject.TestDerivedTemplate _derivedTemplate = new();

    private readonly TestKinematicObject _testKinematicObject;
    
    private readonly SpaceHierarchy _randomHierarchy;
    private readonly SpaceHierarchy _testHierarchy;
    private readonly SpaceHierarchy.KDBranchNode _xNode;
    private readonly SpaceHierarchy.KDBranchNode _yNode;
    private readonly Random _rnd = new();
    
    public SpaceHierarchy_Tests(ITestOutputHelper output)
    {
        KinematicObject.KinematicObjectTemplate.DestroyAll();
        
        _output = output;
        List<KinematicObject> points = new();
        for (int i = 0; i < 10000; ++i)
        {
            points.Add(_template.CreateTestInstance("Test Instance " + i, new SpatialInfo(
                position: new Vec2<SDecimal>(_rnd.NextDouble() * 100, _rnd.NextDouble() * 100)
            )));
        }
        _randomHierarchy = new SpaceHierarchy(points);
        
        /* ----- Test kD tree layout -----
         * 
         *   0 1 2 3 4 5 6 7 8 9
         * 0 . . . . | . . . . .
         * 1 . . . . | . . . . .
         * 2 . # . . | . . . . .
         * 3 . . . . | . # . . .
         * 4 . . . . | . . . . .
         * 5 ________| . . . . .
         * 6 . # | . |__________
         * 7 . . | # | . . . . .
         * 8 . . | . | . . . . .
         * 9 . . | . | . . . . #
         */
        _testHierarchy = new SpaceHierarchy([
            _template.CreateTestInstance("Test Instance A", new SpatialInfo(position: new Vec2<SDecimal>(1, 2))),
            _template.CreateTestInstance("Test Instance B", new SpatialInfo(position: new Vec2<SDecimal>(6, 3))),
            _template.CreateTestInstance("Test Instance C", new SpatialInfo(position: new Vec2<SDecimal>(1, 6))),
            _template.CreateTestInstance("Test Instance D", new SpatialInfo(position: new Vec2<SDecimal>(3, 7))),
            _template.CreateTestInstance("Test Instance E", new SpatialInfo(position: new Vec2<SDecimal>(9, 9))),
        ]);
        
        TestKinematicObject nullTestInstance = _template.CreateTestInstance("Null Test Instance", new SpatialInfo());
        SpaceHierarchy.KDLeafNode leafNode = new(nullTestInstance);
        _xNode = new SpaceHierarchy.KDBranchNode(0, 5, leafNode, leafNode);
        _yNode = new SpaceHierarchy.KDBranchNode(1, 5, leafNode, leafNode);

        _testKinematicObject = _derivedTemplate.CreateTestInstance("Test Instance F", new SpatialInfo(position: new Vec2<SDecimal>(3, 9)));
    }

    [Fact]
    public void SpaceHierarchy_Constructor()
    {
        _output.WriteLine(_randomHierarchy.ToString());
        _output.WriteLine(_testHierarchy.ToString());
    }

    [Fact]
    public void SpaceHierarchy_AddObjectMethod()
    {
        _testHierarchy.AddObject(_testKinematicObject);
        Assert.Equal(6, _testHierarchy.GetObjects().Count);
        Assert.Equal(6, _testHierarchy.GetObjectsOfType<KinematicObject>().Count);
        Assert.Equal(6, _testHierarchy.GetObjectsOfType<TestKinematicObject>().Count);
        Assert.Equal(1, _testHierarchy.GetObjectsOfType<TestDerivedKinematicObject>().Count);
    }
    
    [Fact]
    public void SpaceHierarchy_RemoveObjectMethod()
    {
        _testHierarchy.RemoveObject("Test Instance E");
        Assert.Equal(4, _testHierarchy.GetObjects().Count);
        Assert.Equal(4, _testHierarchy.GetObjectsOfType<KinematicObject>().Count);
        Assert.Equal(4, _testHierarchy.GetObjectsOfType<TestKinematicObject>().Count);
    }

    [Fact]
    public void SpaceHierarchy_GetObjectsInRadiusMethod()
    {
        _testHierarchy.GetObjectsInRadius(new(1, 2), 4);
        _output.WriteLine(_testHierarchy.ToString());
    }

    [Fact]
    public void KDBranchNode_GetSideMethod()
    {
        Vec2<SDecimal> leftXNodePosition = new Vec2<SDecimal>(0, 5);
        Vec2<SDecimal> medianXNodePosition = new Vec2<SDecimal>(5, 0);
        Vec2<SDecimal> rightXNodePosition = new Vec2<SDecimal>(10, 0);
        
        Vec2<SDecimal> leftYNodePosition = new Vec2<SDecimal>(5, 0);
        Vec2<SDecimal> medianYNodePosition = new Vec2<SDecimal>(0, 5);
        Vec2<SDecimal> rightYNodePosition = new Vec2<SDecimal>(0, 10);
        
        Assert.Equal(-1, _xNode.GetSide(leftXNodePosition));
        Assert.Equal(0, _xNode.GetSide(medianXNodePosition));
        Assert.Equal(1, _xNode.GetSide(rightXNodePosition));
        
        Assert.Equal(-1, _yNode.GetSide(leftYNodePosition));
        Assert.Equal(0, _yNode.GetSide(medianYNodePosition));
        Assert.Equal(1, _yNode.GetSide(rightYNodePosition));
    }
}