using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Mesh which covers the area of a camera's screen.
/// </summary>
public class CameraMesh
{
    private static VertexBuffer? _vertexBuffer;
    private static IndexBuffer? _indexBuffer;
    
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

    public void SetBuffers(GraphicsDevice graphics, out VertexBuffer vertexBuffer, out IndexBuffer indexBuffer)
    {
        graphics.SetVertexBuffer(_vertexBuffer);
        vertexBuffer = _vertexBuffer!;
        graphics.Indices = _indexBuffer;
        indexBuffer = _indexBuffer!;
    }

    public RenderTarget2D GenerateOrbitRenderTarget(GraphicsDevice graphics, Dictionary<string, object> shaderParameters)
    {
        Console.WriteLine(" - Start - ");
        if (_vertexBuffer == null || _indexBuffer == null) 
            throw new NullReferenceException("Buffers not generated for this mesh");
        
        graphics.SetVertexBuffer(_vertexBuffer);
        graphics.Indices = _indexBuffer;
        
        Effect effect = Effects.OrbitEffect ?? throw new NullReferenceException("Effect not initialized yet");
        Effect renderTargetEffect = Effects.DefaultEffect ?? throw new NullReferenceException();
        
        Console.WriteLine(Effects.OrbitEffect.Parameters["Center"].GetValueVector2());
        
        effect.Parameters["World"].SetValue(Matrix.Identity);
        
        foreach (var pair in shaderParameters)
            effect.Parameters[pair.Key].SetValue((dynamic)pair.Value);
        
        effect.Parameters["TexelSize"].SetValue(new Vector2(1f / Options.ScreenSize.width, 1f / Options.ScreenSize.height));
        
        int effectPasses = effect.CurrentTechnique.Passes.Count;
        RenderTarget2D[] renderTargets = new RenderTarget2D[effectPasses];
        for (int i = 0; i < renderTargets.Length; i++)
            renderTargets[i] = new RenderTarget2D(graphics, Options.ScreenSize.width, Options.ScreenSize.height,
                false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

        for (int i = 0; i < effectPasses; i++)
        {
            graphics.SetRenderTarget(renderTargets[i]);
            graphics.Clear(Color.Transparent);
            effect.Parameters["SpriteTexture"].SetValue(renderTargets[Math.Max(i - 1, 0)]);
            effect.CurrentTechnique.Passes[i].Apply();
            graphics.DrawInstancedPrimitives(
                PrimitiveType.TriangleList, 0, 0, _vertexBuffer.VertexCount - 2, _vertexBuffer.VertexCount
            );
        }

        for (int i = 0; i < renderTargets.Length - 1; ++i) renderTargets[i].Dispose();
        return renderTargets[^1];
    }
}