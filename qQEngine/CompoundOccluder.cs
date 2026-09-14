using Microsoft.Xna.Framework;

namespace qQEngine;

public class CompoundOccluder
{
    public List<IOccluder> OccluderPrimitives { get; set; } = new();
}