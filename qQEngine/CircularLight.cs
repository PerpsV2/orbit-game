using Microsoft.Xna.Framework;

namespace qQEngine;

public class CircularLight(string identifier, SpatialInfo spatialInfo, SDecimal luminosity, Color colour, SDecimal radius)
    : KinematicObject(identifier, spatialInfo), ILight
{
    public SDecimal Luminosity { get; set; } = luminosity;
    public Color Colour { get; set; } = colour;
    public SDecimal Radius { get; set; } = radius;
}