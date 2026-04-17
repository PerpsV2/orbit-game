using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Mesh for drawing a polygonal object.
/// </summary>
public class PolyMesh : IMesh
{
    private VertexPositionColor[]? _vertices;
    private int[]? _indices;
    
    private VertexBuffer? _vertexBuffer;
    private IndexBuffer? _indexBuffer;

    private readonly Vector2[] _points;

    private bool _buffersGenerated;
    
    public PolyMesh(Vec2Double[] points)
    {
        _points = points.Select(v => new Vector2((float)v.X, -(float)v.Y)).ToArray();
    }

    public void GenerateBuffers(GraphicsDevice graphicsDevice)
    {
        if (_points.Length == 0) return;
        
        _vertices = new VertexPositionColor[_points.Length];
        for (int i = 0; i < _vertices.Length; i++)
            _vertices[i] = new VertexPositionColor(new Vector3(_points[i].X, _points[i].Y, 0), Color.White);
        
        _indices = new int[(_vertices.Length - 2) * 3];
        for (int i = 0; i < _vertices.Length - 2; i++)
        {
            _indices[i * 3] = 0;
            _indices[i * 3 + 1] = i + 1;
            _indices[i * 3 + 2] = (i + 2) % _vertices.Length;
        }

        _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), _vertices.Length, BufferUsage.None);
        _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, _indices.Length, BufferUsage.None);
        
        _vertexBuffer.SetData(_vertices);
        _indexBuffer.SetData(_indices);

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
        if (_vertexBuffer == null || _indexBuffer == null || _vertices == null || _indices == null) 
            throw new NullReferenceException("Buffers not generated for this mesh");
        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;
        
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
                PrimitiveType.TriangleList, 0, 0, _vertices.Length - 2, _vertices.Length
            );
        }
    }
}