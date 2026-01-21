using System.Diagnostics;
using SkiaSharp;

namespace OrbitGame;

public delegate ScientificDecimal OrbitEquation(double angle);

public readonly struct Orbit(OrbitEquation equation, ScientificDecimal scale, ScientificDecimal eccentricity, double periapsis)
{
    public readonly OrbitEquation Equation = equation;
    public readonly ScientificDecimal Scale = scale;
    public readonly ScientificDecimal Eccentricity = eccentricity;
    public readonly double PeriapsisArgument = Utils.UnsignedMod(periapsis, Math.Tau);
    public readonly double ApoapsisArgument = Utils.UnsignedMod(periapsis + Math.PI, Math.Tau);
}

/// <summary>
/// A KinematicObject with information about shape and material and methods for physics.
/// </summary>
public abstract class Body(ScientificDecimal mass, Vector2 position, Vector2 velocity, SKColor colour, string name) 
    : KinematicObject(name, mass, position, velocity)
{
    public SKColor Colour = colour;
    public ICollider? Collider;
    
    public abstract void Draw(SKCanvas canvas, Camera camera);
    public abstract void DrawCollider(SKCanvas canvas, Camera camera);

    public void DrawOrbitalPathLRL(SKCanvas canvas, Camera camera, Body centralForce)
    {
        Orbit orbit = CalculateOrbit(centralForce);
        List<Vector2> orbitPoints = new List<Vector2>();
        Vector2 relCamPosition = camera.AbsolutePosition - centralForce.Position;
        Vector2 maxCamExtentVector = (Vector2)Vector3.Cross(relCamPosition.Normalize(), 
            new(0, 0, ScientificDecimal.Max(camera.Width, camera.Height)));
        double minAngle = Utils.UnsignedMod((relCamPosition + maxCamExtentVector).GetPrincipalAngle(), Math.Tau);
        double maxAngle = Utils.UnsignedMod((relCamPosition - maxCamExtentVector).GetPrincipalAngle(), Math.Tau);
        if (minAngle > maxAngle) maxAngle += Math.Tau;
        
        if (maxAngle - minAngle > Math.PI * 0.75f)
        {
            // drawing elliptic and parabolic orbits
            if (orbit.Eccentricity <= 1)
            {
                for (double a = 0; a <= Math.Tau; a += Math.Tau / Options.OrbitResolutionNumPoints)
                    orbitPoints.Add(centralForce.Position + Vector2.FromPolar(a, orbit.Equation(a)));
            }
            // for drawing hyperbolic orbits
            else
            {
                Console.WriteLine(orbit.Scale);
                double asymptoteAngle1 = Utils.UnsignedMod(Math.Acos((double)-(1 / orbit.Eccentricity)) - orbit.PeriapsisArgument, Math.Tau);
                // TODO: fix calculation for asymptote angle 2
                double asymptoteAngle2 = Utils.UnsignedMod(Math.Acos((double)-(1 / orbit.Eccentricity)) + orbit.PeriapsisArgument, Math.Tau);
                if (asymptoteAngle1 > asymptoteAngle2) asymptoteAngle2 += Math.Tau;
                
                double soiEscapeAngle = Utils.UnsignedMod(Math.Acos((double)(-(orbit.Eccentricity - 3) / (2 * orbit.Eccentricity))), Math.Tau);
                if (orbit.Scale.Negative) soiEscapeAngle = Utils.UnsignedMod(Math.Cos((double)(-(orbit.Eccentricity + 1) / (2 * orbit.Eccentricity))), Math.Tau);
                //if (Utils.UnsignedMod(-soiEscapeAngle, Math.Tau) < soiEscapeAngle)
                //    soiEscapeAngle = Utils.UnsignedMod(-soiEscapeAngle, Math.Tau);
                for (double a = asymptoteAngle1; a < asymptoteAngle2; a += (asymptoteAngle2 - asymptoteAngle1) / Options.OrbitResolutionNumPoints)
                {
                    //double a = 2 * i / (double)Options.OrbitResolutionNumPoints;
                    //if (a <= 1) a = Math.Pow(a, 2) * asymptoteAngle;
                    //else a = Math.Pow(2 - a, 2) * asymptoteAngle + asymptoteAngle;
                    //double trueAngle = asymptoteAngle - a;
                    //if (orbit.Scale.Negative) angleSwept = -Math.Tau + 2 * asymptoteAngle;
                    //double trueAngle = asymptoteAngle1 - angleSwept * (i / (double)Options.OrbitResolutionNumPoints);
                    //ScientificDecimal dist = orbit.Equation(trueAngle);
                    if (orbit.Equation(a) > 0)
                        orbitPoints.Add(centralForce.Position + Vector2.FromPolar(a, orbit.Equation(a)));
                }
                canvas.GS_DrawPoint(camera, Vector2.FromPolar(asymptoteAngle1, orbit.Equation(asymptoteAngle1)),
                    DebugCanvas.Yellow);
            }

            if (orbit.Eccentricity > 1)
            {
                canvas.GS_DrawPoint(camera, orbitPoints.Last(), DebugCanvas.Red);
                canvas.GS_DrawPoint(camera, orbitPoints.First(), DebugCanvas.Blue);
            }

            /*
            if (orbit.Eccentricity > 1)
            {
                canvas.GS_DrawPoint(camera, orbitPoints.Last(), DebugCanvas.Red);
                canvas.GS_DrawPoint(camera, orbitPoints.First(), DebugCanvas.Red);
                double asymptoteAngle = Math.Acos((double)(-1 / orbit.Eccentricity));
                ScientificDecimal soiDistance = 929000000;
                ScientificDecimal firstPointDistance = (orbitPoints.First() - centralForce.Position).Magnitude();
                ScientificDecimal lastPointDistance = (orbitPoints.Last() - centralForce.Position).Magnitude();
                orbitPoints.Add(orbitPoints.Last() + Vector2.FromPolar(-asymptoteAngle, soiDistance - lastPointDistance));
                //orbitPoints.Prepend(orbitPoints.First() - Vector2.FromPolar(asymptoteAngle, soiDistance - firstPointDistance));
            }
            */
        }
        else
        {
            for (double a = minAngle; a < maxAngle; a += (maxAngle - minAngle) / Options.OrbitResolutionNumPoints)
            {
                ScientificDecimal dist = orbit.Equation(a);
                if (dist > 0)
                    orbitPoints.Add(centralForce.Position + Vector2.FromPolar(a, dist));
            }
        }

        if (orbitPoints.Count > 0) canvas.GS_DrawPath(camera, orbitPoints, DebugCanvas.Purple);
    }
    
    private Vector2 CalculateGravitationalAcceleration(Body attractor)
    {
        Vector2 direction = Vector2.DirectionVectorBetween(Position, attractor.Position);
        ScientificDecimal distance = (Position - attractor.Position).Magnitude();
        ScientificDecimal magnitude = Constants.G * attractor.Mass / (distance * distance);
        return direction * magnitude;
    }

    public Vector2 SetNetGravitationalAcceleration(IEnumerable<Body> attractors)
    {
        Vector2 result = Vector2.Zero;
        return Acceleration = attractors
            .Where(x => x != this)
            .Aggregate(result, (sum, next) => 
                sum + CalculateGravitationalAcceleration(next));
    }

    public Orbit CalculateOrbit(Body centralForce)
    {
        Vector2 relVelocity = Velocity - centralForce.Velocity;
        Vector2 relPosition = Position - centralForce.Position;
        
        Vector2 momentum = relVelocity * Mass;
        Vector3 angularMomentum = Vector2.Cross(relPosition, momentum);
        Vector2 directionVector = relPosition.Normalize();
        ScientificDecimal forceStrength = Mass * centralForce.Mass * Constants.G;

        Vector2 lrlVector = Matrix3X3.Scale(-1, 1) * ((Vector2)Vector3.Cross(momentum, angularMomentum) -
                            directionVector * Mass * forceStrength);

        ScientificDecimal c = Mass * forceStrength / angularMomentum.Magnitude().Square();
        ScientificDecimal eccentricity = lrlVector.Magnitude() / (Mass * forceStrength).Abs();
        double periapsis = lrlVector.GetPrincipalAngle() + Math.PI;

        return new Orbit(angle => 1 / (c * (1 + eccentricity * Math.Cos(angle + periapsis))),
            c, eccentricity, periapsis);
    }
    
    public void NI_UpdatePosition(ScientificDecimal timeStep, NumericalIntegrator integrator)
    {
        switch (integrator)
        {
            case NumericalIntegrator.ExplicitEuler:
                Velocity += Acceleration * timeStep;
                Position += Velocity * timeStep;
                Angle += AngularVelocity * (double)timeStep;
                break;
            case NumericalIntegrator.ImplicitEuler:
                throw new NotImplementedException();
            case NumericalIntegrator.RungeKutta4:
                throw new NotImplementedException();
        }
    }
}