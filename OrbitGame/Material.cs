using SkiaSharp;

namespace OrbitGame;

public struct Material
{
    public float RestitutionCoefficient;
    
    public Material(float restitution)
    {
        if (restitution < 0)
            throw new ArgumentOutOfRangeException(nameof(restitution), "Constant of restitution cannot be less than zero");
        RestitutionCoefficient = restitution;
    }
}