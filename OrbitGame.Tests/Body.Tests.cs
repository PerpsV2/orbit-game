using SkiaSharp;

namespace OrbitGame.Tests;

public class Body_Tests
{
    private static readonly Planet TestBody1 = new (
        100, 
        new Vector2(-3, -4), 
        Vector2.Zero, 
        0, new Material(0), SKColor.Empty, null, "TestBody1"
    );
    private static readonly Planet TestBody2 = new (
        1000, 
        Vector2.Zero, 
        Vector2.Zero, 
        0, new Material(0), SKColor.Empty, null, "TestBody2"
    );
    private static readonly Planet TestBody3 = new (
        100, 
        new Vector2(3, 4), 
        Vector2.Zero, 
        0, new Material(0), SKColor.Empty, null, "TestBody3"
    );
    
    private static readonly Planet TestSun = new Planet(
        new ScientificDecimal(1.989m, 30),
        Vector2.Zero,
        Vector2.Zero,
        new ScientificDecimal(6.96340m, 8),
        new Material(0f),
        new SKColor(255, 255, 255, 255), null,
        "Sun"
    );
    private static readonly Planet TestEarth = new Planet(
        new ScientificDecimal(5.9722m, 24),
        new Vector2(
        new ScientificDecimal(-8.5613233m, 8),
        new ScientificDecimal(1.4688537m, 11)
            ),
        new Vector2(
        new ScientificDecimal(-3.0223357m, 4),
        new ScientificDecimal(-1.8447646m, 3)
            ),
        new ScientificDecimal(6.378m, 6),
        new Material(0.2f), new SKColor(100, 200, 255, 255), TestSun, "Earth"
    );
    
    [Fact]
    public void Body_SetNetGravitationalAccelerationMethod()
    {
        AssertExtensions.Equal(Vector2.Zero, TestBody2.SetNetGravitationalAcceleration([TestBody1, TestBody3]));
        double expectedDirection = Math.Atan2(3, 4);
        ScientificDecimal expectedMagnitude = Constants.G * 40;
        AssertExtensions.Equal(new(Math.Cos(expectedDirection) * expectedMagnitude, 
                Math.Sin(expectedDirection) * expectedMagnitude), 
            TestBody2.SetNetGravitationalAcceleration([TestBody1, TestBody2]));
    }

    [Fact]
    public void Body_OrbitProperty()
    {
        Assert.NotNull(TestEarth.Orbit);
        
        AssertExtensions.Equal(new KeplerOrbit(
                TestEarth, TestSun, 
                0.056972123043665647d,
                2.9458771004221047d,
                new ScientificDecimal(1.4856309391947249834250796170m, 11),
                true
            ), TestEarth.Orbit.Value);
    }

    [Fact]
    public void Body_Kepler_UpdatePositionMethod()
    {
        TestEarth.Kepler_UpdatePosition(1000000000);
        
        AssertExtensions.Equal(new Vector2(
                new ScientificDecimal(1.110230674539574452157059913m, 11),
                new ScientificDecimal(1.0613331993880793170037810314m, 11)
            ),
            TestEarth.Position);
        AssertExtensions.Equal(new Vector2(
                new ScientificDecimal(-2.0957987189206352652439078378m, 4),
                new ScientificDecimal(1.9968079424661837661257906279m, 4)
            ),
            TestEarth.Velocity);
    }
}