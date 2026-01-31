using System.Collections.Generic;

namespace OrbitGame;

public abstract class KinematicObjectTemplate
{
    public IMesh Mesh { get; set; }
    public Material Material { get; set; }
    protected readonly Dictionary<string, KinematicObject> Instances = new();

    protected KinematicObjectTemplate(IMesh mesh, Material material)
    {
        Mesh = mesh;
        Material = material;
    }

    public bool Destroy(string identifier)
        => Instances.Remove(identifier);

    public Dictionary<string, KinematicObject> GetInstances()
        => Instances;
}