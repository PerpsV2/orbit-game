using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Interface for the graphical portion of objects (meshes).
/// </summary>
public interface IMesh
{
    /// <summary>
    /// Generate vertex and index buffers for the mesh.
    /// </summary>
    public void GenerateBuffers();

    /// <summary>
    /// Draw the mesh using the generated vertex and index buffers.
    /// </summary>
    /// <param name="graphics">Graphics device to be drawn on.</param>
    /// <param name="transform">Transform to be applied to the mesh.</param>
    /// <param name="shaderParameters">Collection of KeyValuePairs with the parameter name as the key.</param>
    public void Draw(GraphicsDevice graphics, Matrix transform, Dictionary<string, object> shaderParameters);
}