using Microsoft.Xna.Framework;

namespace qQEngine;

public class CompoundOccluder
{
    public List<IOccluder> Occluders { get; set; } = new();
}