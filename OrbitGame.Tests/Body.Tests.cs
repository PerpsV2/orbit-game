using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Xunit;
using Xunit.Abstractions;

namespace OrbitGame.Tests;

public class Body_Tests
{
    private class TestBody(
        string identifier,
        SpatialInfo spatialinfo,
        ObjectInfo objectInfo,
        SDecimal mass,
        Color colour,
        Body? parent) 
        : Body(identifier, spatialinfo, objectInfo, mass, colour, parent)
    {
        public class TestTemplate : BodyTemplate
        {
            public new static Dictionary<string, TestBody> AllInstances { get; } = new();
            
            public TestBody CreateTestInstance(string identifier, SpatialInfo spatialInfo, int mass, Body? parent)
            {
                TestBody instance = new TestBody(identifier, spatialInfo, 
                    new ObjectInfo { OrbitMesh = new OrbitMesh() }, 
                    mass, Color.White, parent);
                return instance;
            }
        }
    }

    private readonly ITestOutputHelper _output;

    private readonly TestBody _testBody1;
    private readonly TestBody _testBody2;

    public Body_Tests(ITestOutputHelper output)
    {
        
        
        _output = output;
        Constants.SetGravitationalConstant((SDecimal)1);
        var template = new TestBody.TestTemplate();
        _testBody2 = template.CreateTestInstance("Test Body 2", new SpatialInfo(
                position: new Vec2<SDecimal>(0, 0),
                velocity: new Vec2<SDecimal>(0, 0)),
            4, null);
        _testBody1 = template.CreateTestInstance("Test Body 1", new SpatialInfo(
                position: new Vec2<SDecimal>(1, 0),
                velocity: new Vec2<SDecimal>(0, 2),
                angularVelocity: 1),
            1, _testBody2);
    }

    [Fact]
    public void Body_CalculateNetAccelerationMethod()
    {
        Assert.Equal(new Vec2<SDecimal>(-4, 0), _testBody1.CalculateNetAcceleration());
        Assert.Equal(new Vec2<SDecimal>(1, 0), _testBody2.CalculateNetAcceleration());
    }

    [Fact]
    public void Body_GenerateKeplerianOrbitMethod()
    {
        _testBody1.GenerateOrbitPath(0);
        Assert.NotNull(_testBody1.OrbitPath.Conics[0].Orbit);
        KeplerOrbit orbit = _testBody1.OrbitPath.Conics[0].Orbit ?? throw new NullReferenceException();
        Assert.Equal(0, orbit.Periapsis);
        Assert.Equal(0, orbit.Eccentricity);
        Assert.Equal(1, orbit.SemiLatusRectum);
        Assert.Equal(Math.PI, orbit.Period);
    }

    [Fact]
    public void Body_UpdatePosition_IntegratorExplicitEulerMethod()
    {
        _testBody1.AngularAcceleration = 1;
        _testBody1.UpdatePosition_Integrator(1, NumericalIntegrator.ExplicitEuler, () => new Vec2<SDecimal>(1, 0), 1);
        Assert.Equal(new Vec2<SDecimal>(1, 2), _testBody1.Velocity);
        Assert.Equal(new Vec2<SDecimal>(2, 2), _testBody1.Position);
        Assert.Equal(2, _testBody1.AngularVelocity);
        Assert.Equal(2, _testBody1.Angle);
    }
    
    [Fact]
    public void Body_UpdatePosition_IntegratorImplicitEulerMethod()
    {
        _testBody1.AngularAcceleration = 1;
        _testBody1.UpdatePosition_Integrator(1, NumericalIntegrator.ImplicitEuler, () => new Vec2<SDecimal>(1, 0), 1);
        
        Assert.Equal(new Vec2<SDecimal>(1, 2), _testBody1.Velocity);
        Assert.Equal(new Vec2<SDecimal>(1, 2), _testBody1.Position);
        Assert.Equal(2, _testBody1.AngularVelocity);
        Assert.Equal(2, _testBody1.Angle);
    }
    
    [Fact]
    public void Body_UpdatePosition_IntegratorVelocityVerletMethod()
    {
        _testBody1.AngularAcceleration = 1;
        _testBody1.UpdatePosition_Integrator(1, NumericalIntegrator.VelocityVerlet, () => new Vec2<SDecimal>(1, 0), 1);
        
        Assert.Equal(new Vec2<SDecimal>(1, 2), _testBody1.Velocity);
        Assert.Equal(new Vec2<SDecimal>(1.5, 2), _testBody1.Position);
        Assert.Equal(2, _testBody1.AngularVelocity);
        Assert.Equal(2, _testBody1.Angle);
    }
    
    [Fact]
    public void Body_UpdatePosition_IntegratorRungeKutta4Method()
    {
        _testBody1.AngularAcceleration = 1;
        _testBody1.UpdatePosition_Integrator(1, NumericalIntegrator.RungeKutta4, () => new Vec2<SDecimal>(1, 0), 1);
        
        Assert.Equal(new Vec2<SDecimal>(1, 2), _testBody1.Velocity, 1e-6);
        Assert.Equal(new Vec2<SDecimal>(1.5, 2), _testBody1.Position, 1e-6);
        Assert.Equal(2, _testBody1.AngularVelocity);
        Assert.Equal(2, _testBody1.Angle);
    }

    [Fact]
    public void Body_UpdatePosition_KeplerMethod()
    {
        _testBody1.AngularAcceleration = 1;
        _testBody1.GenerateOrbitPath(0);
        _testBody1.UpdatePosition_Kepler(Math.PI / 2, 1);
        Assert.Equal(new Vec2<SDecimal>(0, -2), _testBody1.Velocity);
        Assert.Equal(new Vec2<SDecimal>(-1, 0), _testBody1.Position);
        Assert.Equal(2, _testBody1.AngularVelocity);
        Assert.Equal(2, _testBody1.Angle);
    }
}