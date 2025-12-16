using SkiaSharp;

namespace OrbitGame;

public struct Material
{
    public SKColor Colour;
    public float RestitutionCoefficient;
    
    public Material(SKColor colour, float restitution)
    {
        Colour = colour;
        if (restitution < 0)
            throw new ArgumentOutOfRangeException(nameof(restitution), "Constant of restitution cannot be less than zero");
        RestitutionCoefficient = restitution;
    }
}