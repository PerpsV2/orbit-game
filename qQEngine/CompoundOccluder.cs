namespace qQEngine;

public class CompoundOccluder
{
    public bool IsEmitter { get; set; } = false;
    public List<CircularOccluder> Occluders { get; set; } = new();
}