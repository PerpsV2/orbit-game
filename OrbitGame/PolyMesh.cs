using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public class PolyMesh : IMesh
{
    private VertexPositionColor[] _vertices;
    private int[] _indices;
    
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;

    private Vector2[] _points;
    public SD_Vector2[] SD_Points;

    public PolyMesh(SD_Vector2[] points)
    {
        SD_Points = points;
        _points = points.Select(v => new Vector2((float)v.X, (float)v.Y)).ToArray();
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
    }

    public void Draw(GraphicsDevice graphicsDevice, Effect effect, Matrix transform, Dictionary<string, object> shaderParameters)
    {
        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;
        
        effect.Parameters["world"].SetValue(transform);
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