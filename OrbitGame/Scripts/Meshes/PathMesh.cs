using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public class PathMesh : IMesh
{
    private VertexBuffer? _vertexBuffer;
    private IndexBuffer? _indexBuffer;
    private bool _buffersGenerated;

    private readonly Vector2[] _points;

    public PathMesh(Vector2[] points)
    {
        if (points.Length <= 1)
            throw new ArgumentOutOfRangeException(nameof(points), "Must have at least 2 points");
        _points = points;
    }

    public void GenerateBuffers(GraphicsDevice graphicsDevice)
    {
        VertexPositionColor[] vertices = _points.Select(v => new VertexPositionColor(new Vector3(v.X, v.Y, 0), Color.White)).ToArray(); 

        int[] indices = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            indices[i] = i;
         
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
                PrimitiveType.LineStrip, 0, 0, 1, 1
            );
        }
    }
}