using Microsoft.Xna.Framework;

namespace qQEngine;

public class CircularLight(string identifier, SpatialInfo spatialInfo, double luminosity, Color colour) 
    : KinematicObject(identifier, spatialInfo)
{
    public double Luminosity { get; set; } = luminosity;
    public Color Colour { get; set; } = colour;
}