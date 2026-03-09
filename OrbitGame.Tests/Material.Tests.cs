using System;
using Xunit;

namespace OrbitGame.Tests;

public class Material_Tests
{
    [Fact]
    public void Material_Constructor()
    {
        // restitution tests
        Assert.Throws<ArgumentOutOfRangeException>(() => new Material(-1, 0, 0));
        
        // dynamic friction tests
        Assert.Throws<ArgumentOutOfRangeException>(() => new Material(0, 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Material(0, 0, 2));
        
        // static friction tests
        Assert.Throws<ArgumentOutOfRangeException>(() => new Material(0, 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Material(0, 2, 0));

        Material testMaterial = new Material(0, 0.5f, 0.5f);
    }
}