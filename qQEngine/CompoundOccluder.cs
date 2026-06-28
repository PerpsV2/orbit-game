using Microsoft.Xna.Framework;

namespace qQEngine;

public class CompoundOccluder
{
    public bool IsEmitter { get; set; } = false;
    public Color Colour { get; set; } = Color.White;
    public List<CircularOccluder> Occluders { get; set; } = new();
}