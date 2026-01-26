using SkiaSharp;

namespace OrbitGame.Tests;

public class OriginBody_Tests
{
    private static readonly Planet TestBody1 = new (
        100, 
        new Vector2(-3, -4), 
        new Vector2(-3, -4), 
        0, new Material(0), SKColor.Empty, null, "TestBody1"
    );
    private static readonly Planet TestBody2 = new (
        100, 
        Vector2.Zero, 
        Vector2.Zero, 
        0, new Material(0), SKColor.Empty, null, "TestBody2"
    );
    private static readonly Planet TestBody3 = new (
        100, 
        new Vector2(3, 4), 
        new Vector2(3, 4), 
        0, new Material(0), SKColor.Empty, null, "TestBody3"
    );

    [Fact]
    public void OriginBody_ResetOriginMethod()
    {
        OriginBody.Body = TestBody3;
        OriginBody.ResetOrigin([TestBody1, TestBody2]);
        AssertExtensions.Equal(new Vector2(-6, -8), TestBody1.Position);
        AssertExtensions.Equal(new Vector2(-3, -4), TestBody2.Velocity);
    }
}