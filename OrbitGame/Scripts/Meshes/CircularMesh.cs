using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Mesh for drawing a completely circular object.
/// </summary>
public class CircularMesh : IMesh
{
    private static VertexBuffer? _vertexBuffer;
    private static IndexBuffer? _indexBuffer;
    private static bool _buffersGenerated;

    public void GenerateBuffers(GraphicsDevice graphicsDevice)
    {
        VertexPositionTexture[] vertices = [
            new (new Vector3(1, 1, 0), new Vector2(1, 1)),
            new (new Vector3(1, -1, 0), new Vector2(1, -1)),
            new (new Vector3(-1, 1, 0), new Vector2(-1, 1)),
            new (new Vector3(-1, -1, 0), new Vector2(-1, -1)),
        ];

        int[] indices = [0, 1, 2, 2, 1, 3];
         
        _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.None);
        _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
         
        _vertexBuffer.SetData(vertices);
        _indexBuffer.SetData(indices);

        _buffersGenerated = true;
    }

    public bool TryGenerateBuffers(GraphicsDevice graphicsDevice)
    {
        if (_buffersGenerated) return false;
        GenerateBuffers(graphicsDevice);
        return true;
    }

    public void Draw(GraphicsDevice graphicsDevice, Matrix transform, Dictionary<string, object> shaderParameters, Effect? effect)
    {
        if (_vertexBuffer == null || _indexBuffer == null) 
            throw new NullReferenceException("Buffers not generated for this mesh");
        effect ??= Effects.CircleEffect ?? throw new NullReferenceException("Circle effect not initialized");
        
        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;

        effect.Parameters["World"].SetValue(transform);
        foreach (var pair in shaderParameters)
            effect.Parameters[pair.Key].SetValue((dynamic)pair.Value);

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawInstancedPrimitives(
                PrimitiveType.TriangleList, 0, 0, _indexBuffer.IndexCount / 3, _vertexBuffer.VertexCount
            );
        }
    }
}