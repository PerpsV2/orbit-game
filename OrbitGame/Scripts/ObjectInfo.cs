namespace OrbitGame;

/// <summary>
/// Class containing behavioural properties of an object.
/// </summary>
/// <param name="mesh">Visual portion of an object</param>
/// <param name="collider">Responsible for detecting and resolving overlaps between objects</param>
/// <param name="material">Information used to resolve collisions between objects</param>
public struct ObjectInfo(IMesh mesh, CompactCollider collider, Material material)
{
    public IMesh Mesh = mesh;
    public CompactCollider Collider = collider;
    public Material Material = material;
}