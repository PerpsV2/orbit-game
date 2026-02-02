using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public class OrbitMesh : IMesh
{
    public static readonly int OrbitVertices = 400;
    private static VertexBuffer? _vertexBuffer;
    private static IndexBuffer? _indexBuffer;

    public OrbitMesh()
    {
        GenerateBuffers();
    }

    public void GenerateBuffers()
    {
        var vertices = new VertexPositionColor[OrbitVertices + 1];
        for (int i = 0; i < OrbitVertices; i++)
        {
            double angle = i * Math.Tau / OrbitVertices;
            vertices[i] = new VertexPositionColor(new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0), Color.White);
        }
        vertices[^1] = vertices[0];

        var indices = new int[vertices.Length];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;
         
        _vertexBuffer = new VertexBuffer(OrbitGame.Graphics, typeof(VertexPositionColor), vertices.Length, BufferUsage.None);
        _indexBuffer = new IndexBuffer(OrbitGame.Graphics, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
         
        _vertexBuffer.SetData(vertices);
        _indexBuffer.SetData(indices);
    }
    
    public void Draw(GraphicsDevice graphicsDevice, Effect effect, Matrix transform, Dictionary<string, object> shaderParameters)
    {
        if (_vertexBuffer == null || _indexBuffer == null) 
            throw new NullReferenceException("Buffers not generated for this mesh");
        
        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;
        
        effect.Parameters["world"].SetValue(transform);
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