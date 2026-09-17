using Microsoft.Xna.Framework;

namespace qQEngine;

public class CircularLight(string identifier, SpatialInfo spatialInfo, Color colour, SDecimal radius, SDecimal luminosity, double magnitude)
    : KinematicObject(identifier, spatialInfo), ILight
{
    public SDecimal Luminosity { get; set; } = luminosity;
    public double Magnitude { get; set; } = magnitude;
    public Color Colour { get; set; } = colour;
    public SDecimal Radius { get; set; } = radius;
}