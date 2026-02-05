using System;

namespace OrbitGame;

public class Material
{
    public float RestitutionCoefficient;
    public float StaticFrictionCoefficient;
    public float DynamicFrictionCoefficient;
    
    public Material(float restitution, float staticFriction = 0f, float dynamicFriction = 0f)
    {
        if (restitution < 0)
            throw new ArgumentOutOfRangeException(nameof(restitution), "Coefficient of restitution cannot be less than zero");
        if (staticFriction < 0 || staticFriction > 1)
            throw new ArgumentOutOfRangeException(nameof(staticFriction),
                "Static friction cannot be negative or greater than 1");
        if (dynamicFriction < 0 || dynamicFriction > 1)
            throw new ArgumentOutOfRangeException(nameof(dynamicFriction),
                "Dynamic friction cannot be negative or greater than 1");
        RestitutionCoefficient = restitution;
        StaticFrictionCoefficient = staticFriction;
        DynamicFrictionCoefficient = dynamicFriction;
    }
}