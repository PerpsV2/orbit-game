using System;
using Xunit;
using Xunit.Abstractions;
using Interpolation = OrbitGame.InterpolationHandler<OrbitGame.PDecimal>.Interpolation;

namespace OrbitGame.Tests;

public class InterpolationHandler_Tests
{
    private readonly ITestOutputHelper _output;
    
    private PDecimal _testIndependent = 0;
    private PDecimal _testDependent = 0;

    private readonly PDecimal _testStartIndependent = 10;
    private readonly PDecimal _testEndIndependent = 20;
    private readonly PDecimal _testStartDependent = 5;
    private readonly PDecimal _testEndDependent = -5;

    private Interpolation _interpolation;
    
    public InterpolationHandler_Tests(ITestOutputHelper output)
    {
        _output = output;
        _interpolation = InterpolationHandler<PDecimal>.CreateInterpolation(
            new(() => _testIndependent, value => _testDependent = value),
            _testStartIndependent, _testEndIndependent, _testStartDependent, _testEndDependent
        );
    }

    [Fact]
    public void InterpolationHandler_CreateInterpolationMethod()
    {
        _interpolation = InterpolationHandler<PDecimal>.CreateInterpolation(
            new(() => _testIndependent, value => _testDependent = value),
            _testStartIndependent, _testEndIndependent, _testStartDependent, _testEndDependent
        );
        
        Assert.False(_interpolation.IsActive());
        Assert.False(_interpolation.IsEnded());

        Assert.Throws<ArgumentOutOfRangeException>(() => InterpolationHandler<PDecimal>.CreateInterpolation(
            new(() => _testIndependent, value => _testDependent = value),
            _testEndIndependent, _testStartIndependent, _testStartDependent, _testEndDependent
        ));
    }
    
    [Fact]
    public void InterpolationHandler_LinearInterpolateMethod()
    {
        _interpolation.Interpolator = InterpolationHandler<PDecimal>.LinearInterpolate;
        InterpolationHandler<PDecimal>.UpdateInterpolationValues();
        Assert.Equal(0, _testDependent);

        _testIndependent = 10;
        InterpolationHandler<PDecimal>.UpdateInterpolationValues();
        Assert.Equal(5, _testDependent);

        _testIndependent = 12.5;
        InterpolationHandler<PDecimal>.UpdateInterpolationValues();
        Assert.Equal(2.5, _testDependent);
        
        _testIndependent = 15;
        InterpolationHandler<PDecimal>.UpdateInterpolationValues();
        Assert.Equal(0, _testDependent);
        
        _testIndependent = 20;
        InterpolationHandler<PDecimal>.UpdateInterpolationValues();
        Assert.Equal(-5, _testDependent);
        
        _testIndependent = 25;
        InterpolationHandler<PDecimal>.UpdateInterpolationValues();
        Assert.Equal(-5, _testDependent);
    }

    [Fact]
    public void Interpolation_InterpolationStartEvent()
    {
        _testIndependent = 5;
        Assert.NotRaises<EventArgs>(
            handler => _interpolation.InterpolationStart += handler.Invoke,
            InterpolationHandler<PDecimal>.UpdateInterpolationValues
            );
        _testIndependent = 10;
        Assert.Raises<EventArgs>(
            handler => _interpolation.InterpolationStart += handler.Invoke,
            InterpolationHandler<PDecimal>.UpdateInterpolationValues
        );

        _testIndependent = 5;
        InterpolationHandler<PDecimal>.UpdateInterpolationValues();
        _testIndependent = 20;
        Assert.Raises<EventArgs>(
            handler => _interpolation.InterpolationStart += handler.Invoke,
            InterpolationHandler<PDecimal>.UpdateInterpolationValues
        );
    }
    
    [Fact]
    public void Interpolation_InterpolationEndEvent()
    {
        _testIndependent = 15;
        Assert.NotRaises<EventArgs>(
            handler => _interpolation.InterpolationEnd += handler.Invoke,
            InterpolationHandler<PDecimal>.UpdateInterpolationValues
        );
        _testIndependent = 20;
        Assert.Raises<EventArgs>(
            handler => _interpolation.InterpolationEnd += handler.Invoke,
            InterpolationHandler<PDecimal>.UpdateInterpolationValues
        );
    }
}