using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Mesh for drawing a complete elliptic orbit.
/// </summary>
public class OrbitMesh : IMesh
{
    private static readonly int OrbitVertices = Options.OrbitVertices;
    private static VertexBuffer? _vertexBuffer;
    private static IndexBuffer? _indexBuffer;
    private static bool _buffersGenerated;

    public void GenerateBuffers(GraphicsDevice graphicsDevice)
    {
        var vertices = new VertexPositionColor[OrbitVertices + 1];
        for (int i = 0; i < OrbitVertices; i++)
        {
            double angle = i * Math.Tau / OrbitVertices;
            vertices[i] = new VertexPositionColor(new Vector3((float)Math.Cos(angle), -(float)Math.Sin(angle), 0), Color.White);
        }
        vertices[^1] = vertices[0];

        var indices = new int[vertices.Length];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;
         
        _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.None);
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
        effect ??= Effects.DefaultEffect ?? throw new NullReferenceException("Default effect not initialized");
        
        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;

        effect.Parameters["World"].SetValue(transform);
        foreach (var pair in shaderParameters)
            effect.Parameters[pair.Key].SetValue((dynamic)pair.Value);
        
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawInstancedPrimitives(
                PrimitiveType.LineStrip, 0, 0, OrbitVertices, _vertexBuffer.VertexCount
            );
        }
    }
}