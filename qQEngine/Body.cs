namespace qQEngine;

/// <summary>
/// Delegate for calculating instant acceleration given other spatial info.
/// </summary>
public delegate Vec2Double CalculateAccelerationMethod();

public class Body(string identifier, SpatialInfo spatialInfo) 
    : KinematicObject(identifier, spatialInfo)
{
    public SDecimal Mass { get; set; }
    public SDecimal Charge { get; set; }
    
    /// <summary>
    /// Calculate the gravitational acceleration caused by the mass of another body.
    /// </summary>
    private Vec2Double CalculateGravitationalAcceleration(Body effector)
    {
        Vec2Double direction = (effector.Position - Position).Normalize();
        double magnitude = (double)(Constants.G * effector.Mass / (Position - effector.Position).MagnitudeSquared());
        return direction * magnitude;
    }

    /// <summary>
    /// Calculate the net gravitational acceleration with all bodies given.
    /// </summary>
    private Vec2Double CalculateNetGravitationalAcceleration(IEnumerable<Body> effectors)
    {
        Vec2Double result = Vec2Double.Zero;
        foreach (var attractor in effectors)
        {
            if (attractor == this) continue;
            result += CalculateGravitationalAcceleration(attractor);
        }

        return result;
    }
    
    /// <summary>
    /// Calculate the electrostatic acceleration caused by the charges of another body
    /// </summary>
    private Vec2Double CalculateElectrostaticAcceleration(Body effector)
    {
        Vec2Double direction = (effector.Position - Position).Normalize();
        double magnitude = (double)(Constants.Ke * effector.Charge / (Position - effector.Position).MagnitudeSquared());
        return direction * magnitude;
    }
    
    /// <summary>
    /// Calculate the net gravitational acceleration with all bodies given.
    /// </summary>
    private Vec2Double CalculateNetElectrostaticAcceleration(IEnumerable<Body> effectors)
    {
        Vec2Double result = Vec2Double.Zero;
        foreach (var attractor in effectors)
        {
            if (attractor == this) continue;
            result += CalculateElectrostaticAcceleration(attractor);
        }

        return result;
    }
    
    /// <summary>
    /// Calculate the net acceleration from gravity and other sources.
    /// </summary>
    public virtual Vec2Double CalculateNetAcceleration(List<Body> effectors)
    {
        Body[] bodies = effectors.ToArray();
        return CalculateNetGravitationalAcceleration(effectors) + 
               CalculateNetElectrostaticAcceleration(effectors);
    }

    private void UpdatePosition_ExplicitEuler(double timeStep, CalculateAccelerationMethod calculateAcceleration)
    {
        Acceleration = calculateAcceleration();
        Velocity += Acceleration * timeStep;
        Position += Velocity * timeStep;
        AngularVelocity += AngularAcceleration * timeStep;
        Angle += AngularVelocity * timeStep;
    }

    private void UpdatePosition_ImplicitEuler(double timeStep, CalculateAccelerationMethod calculateAcceleration)
    {
        Acceleration = calculateAcceleration();
        Position += Velocity * timeStep;
        Velocity += Acceleration * timeStep;
        AngularVelocity += AngularAcceleration * timeStep;
        Angle += AngularVelocity * timeStep;
    }

    private void UpdatePosition_VelocityVerlet(double timeStep, CalculateAccelerationMethod calculateAcceleration)
    {
        Vec2Double acceleration1 = calculateAcceleration();
        Position += Velocity * timeStep + acceleration1 * 0.5f * timeStep * timeStep;
        Vec2Double acceleration2 = calculateAcceleration();
        Velocity += (acceleration1 + acceleration2) * 0.5f * timeStep;
        AngularVelocity += AngularAcceleration * timeStep;
        Angle += AngularVelocity * timeStep;
    }

    private void UpdatePosition_RungeKutta4(double timeStep, CalculateAccelerationMethod calculateAcceleration)
    {
        Vec2Double originalPosition = Position;
        Vec2Double originalVelocity = Velocity;

        Acceleration = calculateAcceleration();
        Vec2Double originalAcceleration = Acceleration;
        Vec2Double velocityK1 = originalAcceleration * timeStep;
        Vec2Double positionK1 = Velocity * timeStep;

        Position = originalPosition + positionK1 * 0.5f;
        Velocity = originalVelocity + velocityK1 * 0.5f;

        Acceleration = calculateAcceleration();
        Vec2Double velocityK2 = Acceleration * timeStep;
        Vec2Double positionK2 = Velocity * timeStep;

        Position = originalPosition + positionK2 * 0.5f;
        Velocity = originalVelocity + velocityK2 * 0.5f;

        Acceleration = calculateAcceleration();
        Vec2Double velocityK3 = Acceleration * timeStep;
        Vec2Double positionK3 = Velocity * timeStep;

        Position = originalPosition + positionK3;
        Velocity = originalVelocity + velocityK3;

        Acceleration = calculateAcceleration();
        Vec2Double velocityK4 = Acceleration * timeStep;
        Vec2Double positionK4 = Velocity * timeStep;

        Velocity = originalVelocity + (velocityK1 + velocityK2 * 2 + velocityK3 * 2 + velocityK4) * (1f / 6f);
        Position = originalPosition + (positionK1 + positionK2 * 2 + positionK3 * 2 + positionK4) * (1f / 6f);

        AngularVelocity += AngularAcceleration * timeStep;
        Angle += AngularVelocity * timeStep;
    }
    
    public void UpdatePosition_Integrator(
        double timeStep, 
        NumericalIntegrator integrator, 
        CalculateAccelerationMethod calculateAcceleration,
        uint integrationIterationAmount = 0)
    {
        timeStep /= integrationIterationAmount;
        for (int i = 0; i < integrationIterationAmount; ++i)
        {
            switch (integrator)
            {
                case NumericalIntegrator.ExplicitEuler: UpdatePosition_ExplicitEuler(timeStep, calculateAcceleration); break;
                case NumericalIntegrator.ImplicitEuler: UpdatePosition_ImplicitEuler(timeStep, calculateAcceleration); break;
                case NumericalIntegrator.VelocityVerlet: UpdatePosition_VelocityVerlet(timeStep, calculateAcceleration); break;
                case NumericalIntegrator.RungeKutta4: UpdatePosition_RungeKutta4(timeStep, calculateAcceleration); break;
                default: throw new ArgumentOutOfRangeException(nameof(integrator));
            }
        }
    }
}