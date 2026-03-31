using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Graphical portion of a game object.
/// </summary>
public interface IMesh
{
    /// <summary>
    /// Generate vertex and index buffers for the mesh.
    /// </summary>
    public void GenerateBuffers(GraphicsDevice graphicsDevice);
    
    /// <summary>
    /// Generates vertex and index buffers for the mesh only if they have not already been generated.
    /// </summary>
    /// <returns>Returns true if successful, false otherwise.</returns>
    public bool TryGenerateBuffers(GraphicsDevice graphicsDevice);

    /// <summary>
    /// Draw the mesh using the generated vertex and index buffers.
    /// </summary>
    /// <param name="graphicsDevice">Graphics device to be drawn on.</param>
    /// <param name="transform">Transform to be applied to the mesh.</param>
    /// <param name="shaderParameters">Dictionary with a shader parameter name as a key and its value as a value.</param>
    public void Draw(GraphicsDevice graphicsDevice, Matrix transform, Dictionary<string, object> shaderParameters);
}