namespace OrbitGame;

public struct ObjectInfo(IMesh mesh, CompactCollider collider, Material material)
{
    public IMesh Mesh = mesh;
    public CompactCollider Collider = collider;
    public Material Material = material;
}