using Microsoft.Xna.Framework;

namespace qQEngine;

public interface ILight
{
    public SDecimal Luminosity { get; set; }
    public Color Colour { get; set; }
}