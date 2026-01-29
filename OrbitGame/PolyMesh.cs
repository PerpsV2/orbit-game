using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public class PolyMesh
{
    private VertexPositionColor[] _vertices;
    private int[] _indices;
    
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;

    private Color _colour;
    private GraphicsDevice _graphics;
    private double _radius;

    public PolyMesh(GraphicsDevice graphicsDevice, Color colour)
    {
        _colour = colour;
        _graphics = graphicsDevice;
    }

    private void SetBuffersPoly(Vector2[] points)
    {
        if (points.Length == 0) return;
        
        _vertices = new VertexPositionColor[points.Length];
        for (int i = 0; i < _vertices.Length; i++)
            _vertices[i] = new VertexPositionColor(new Vector3(points[i].X, points[i].Y, 0), Color.White);
        
        _indices = new int[(_vertices.Length - 2) * 3];
        for (int i = 0; i < _vertices.Length - 2; i++)
        {
            _indices[i * 3] = 0;
            _indices[i * 3 + 1] = i + 1;
            _indices[i * 3 + 2] = (i + 2) % _vertices.Length;
        }
        
        _radius = _vertices.Select(v => Math.Sqrt(v.Position.X * v.Position.X + v.Position.Y * v.Position.Y)).Max();
        
        _vertexBuffer = new VertexBuffer(_graphics, typeof(VertexPositionColor), _vertices.Length, BufferUsage.None);
        _indexBuffer = new IndexBuffer(_graphics, IndexElementSize.ThirtyTwoBits, _indices.Length, BufferUsage.None);
        
        _vertexBuffer.SetData(_vertices);
        _indexBuffer.SetData(_indices);
    }

    public void SetBuffersPoly(SD_Vector2[] points)
    {
        SetBuffersPoly(points.Select(v => new Vector2((float)v.X, (float)v.Y)).ToArray());
    }

    public void DrawMesh(Camera camera, KinematicObject kinObj)
    {
        _graphics.SetVertexBuffer(_vertexBuffer);
        _graphics.Indices = _indexBuffer;
        
        Effect effect = OrbitGame.CurrentEffect;
        Vector2 center = camera.ConvertToScreenCoordinates(kinObj.Position);
        Vector3 scale = new Vector3((float)(Options.ScreenSize.height / camera.Height),
            (float)(Options.ScreenSize.width / camera.Width), 1);
        if (_radius * scale.X < 0.5) return;
        effect.Parameters["world"].SetValue(Matrix.CreateScale(scale) * Matrix.CreateTranslation(new Vector3(center.X, center.Y, 0)));
        effect.Parameters["colour"].SetValue(_colour.ToVector4());
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphics.DrawInstancedPrimitives(
                PrimitiveType.TriangleList, 0, 0, _vertices.Length - 2, _vertices.Length
            );
        }
    }
}