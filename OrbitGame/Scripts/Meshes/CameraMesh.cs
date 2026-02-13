using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Mesh which covers the area of a camera's screen.
/// </summary>
public class CameraMesh : IMesh
{
    private static VertexBuffer? _vertexBuffer;
    private static IndexBuffer? _indexBuffer;

    private Effect? _effect;
    
    public void GenerateBuffers()
    {
        VertexPositionTexture[] vertices = [
            new (new Vector3(+Options.ScreenSize.width, +Options.ScreenSize.height, 0), new Vector2(1, 1)),
            new (new Vector3(+Options.ScreenSize.width, -Options.ScreenSize.height, 0), new Vector2(1, -1)),
            new (new Vector3(-Options.ScreenSize.width, +Options.ScreenSize.height, 0), new Vector2(-1, 1)),
            new (new Vector3(-Options.ScreenSize.width, -Options.ScreenSize.height, 0), new Vector2(-1, -1)),
        ];

        int[] indices = [0, 1, 2, 2, 1, 3];
         
        _vertexBuffer = new VertexBuffer(OrbitGame.Graphics, typeof(VertexPositionTexture), vertices.Length, BufferUsage.None);
        _indexBuffer = new IndexBuffer(OrbitGame.Graphics, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
         
        _vertexBuffer.SetData(vertices);
        _indexBuffer.SetData(indices);
    }

    public void Draw(GraphicsDevice graphics, Matrix transform, Dictionary<string, object> shaderParameters)
    {
        if (_vertexBuffer == null || _indexBuffer == null) 
            throw new NullReferenceException("Buffers not generated for this mesh");
        
        Effect effect = _effect ?? throw new NullReferenceException("Effect not initialized yet");
        
        graphics.SetVertexBuffer(_vertexBuffer);
        graphics.Indices = _indexBuffer;
        
        effect.Parameters["World"].SetValue(transform);
        foreach (var pair in shaderParameters)
            effect.Parameters[pair.Key].SetValue((dynamic)pair.Value);
        
        effect.Parameters["TexelSize"].SetValue(new Vector2(1f / Options.ScreenSize.width / 5, 1f / Options.ScreenSize.height / 5));
        
        RenderTarget2D renderTarget2D = new(graphics, Options.ScreenSize.width * 5, Options.ScreenSize.height * 5);
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            effect.Parameters["SpriteTexture"].SetValue(renderTarget2D);
            graphics.SetRenderTarget(renderTarget2D);
            graphics.DrawInstancedPrimitives(
                PrimitiveType.TriangleList, 0, 0, _vertexBuffer.VertexCount - 2, _vertexBuffer.VertexCount
            );
        }

        Effect renderTargetEffect = Effects.RenderTargetEffect ?? throw new NullReferenceException();
        renderTargetEffect.Parameters["SpriteTexture"].SetValue(renderTarget2D);
        graphics.SetRenderTarget(null);
        graphics.DrawInstancedPrimitives(
            PrimitiveType.TriangleList, 0, 0, _vertexBuffer.VertexCount - 2, _vertexBuffer.VertexCount
        );
    }

    public void Draw(GraphicsDevice graphics, Effect effect,
        Dictionary<string, object> shaderParameters)
    {
        _effect = effect;
        Draw(graphics, Matrix.Identity, shaderParameters);
    }
}