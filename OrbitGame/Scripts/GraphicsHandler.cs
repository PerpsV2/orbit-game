using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public class GraphicsHandler(SpriteBatch spriteBatch) : IGraphicsHandler
{
    private GraphicsDevice GraphicsDevice => spriteBatch.GraphicsDevice;
    
    private static readonly CircularMesh CircleMesh = new ();
    public void DrawBody(qQEngine.Camera camera, qQEngine.Body body, Color colour)
    {
        foreach (var occluder in body.Occluder.Occluders)
        {
            DrawCircle(camera.ConvertToScreenCoordinates(occluder.LocalPosition) + 
                       camera.ConvertToScreenCoordinates(body.Position), 
                camera.ConvertToScreenDistance(occluder.Radius), colour);
        }
    }

    public void DrawCircle(Vector2 center, float radius, Color colour)
    {
        Matrix transform = Matrix.CreateScale(radius, radius, 1) *
                           Matrix.CreateTranslation(new Vector3(center.X, center.Y, 0));
        Effect effect = Effects.CircleEffect ?? throw new NullReferenceException("Effect not initialized yet");;
        DrawMesh(CircleMesh, transform, new Dictionary<string, object>{{"Colour", colour.ToVector4()}}, effect);
    }

    public void DrawPoly(List<Vector2> points, Color colour)
    {
        if (points.Count < 3) return;
        
        var vertices = new VertexPositionColor[points.Count];
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = new VertexPositionColor(new Vector3(points[i].X, points[i].Y, 0), Color.White);
        
        var indices = new int[(vertices.Length - 2) * 3];
        for (int i = 0; i < vertices.Length - 2; i++)
        {
            indices[i * 3] = 0;
            indices[i * 3 + 1] = i + 1;
            indices[i * 3 + 2] = (i + 2) % vertices.Length;
        }

        Effect effect = Effects.DefaultEffect ?? throw new NullReferenceException("Effect not initialized yet");
        effect.Parameters["World"].SetValue(Matrix.Identity);
        effect.Parameters["Colour"].SetValue(colour.ToVector4());
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3
            );
        }
    }

    public void DrawPath(IEnumerable<Vector2> points, Color colour)
    {
        VertexPositionColor[] vertices = points.Select(x => new VertexPositionColor(new Vector3(x.X, x.Y, 0), Color.White)).ToArray();
        int[] indices = new int[vertices.Length * 2];
        for (int i = 0; i < vertices.Length - 1; ++i)
        {
            indices[2 * i] = i;
            indices[2 * i + 1] = i + 1;
        }
        
        Effect effect = Effects.DefaultEffect ?? throw new NullReferenceException("Effect not initialized yet");
        effect.Parameters["World"].SetValue(Matrix.Identity);
        effect.Parameters["Colour"].SetValue(colour.ToVector4());
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.LineList, vertices, 0, vertices.Length, indices, 0, vertices.Length - 1
            );
        }
    }

    public void SD_DrawPath(Camera camera, IEnumerable<Vec2<SDecimal>> points, Color colour)
        => DrawPath(points.Select(camera.ConvertToScreenCoordinates).ToArray(), colour);

    public void DrawLine(Vector2 start, Vector2 end, Color colour)
    {
        Vector2 lineVector =  end - start;
        Matrix transform = Matrix.CreateScale(lineVector.X, lineVector.Y, 1) *
                           Matrix.CreateTranslation(start.X, start.Y, 0);
        LineMesh.Mesh.TryGenerateBuffers(GraphicsDevice);
        LineMesh.Mesh.Draw(GraphicsDevice, transform, new() {{"Colour", colour.ToVector4() }}, Effects.DefaultEffect);
    }

    public void DrawLineR(Vector2 start, Vector2 displacement, Color colour)
    {
        DrawLine(start, start + displacement, colour);
    }

    public void SD_DrawLine(Camera camera, Vec2<SDecimal> start, Vec2<SDecimal> end, Color colour)
    {
        Vector2 screenStart = camera.ConvertToScreenCoordinates(start);
        Vector2 screenEnd = camera.ConvertToScreenCoordinates(end);
        DrawLine(screenStart, screenEnd, colour);
    }

    public void SD_DrawLineR(Camera camera, Vec2<SDecimal> start, Vec2<SDecimal> displacement, Color colour)
    {
        SD_DrawLine(camera, start, start + displacement, colour);
    }

    public void DrawPoint(Vector2 position, Color colour)
    {
        VertexPositionColor[] vertices = [
            new(new Vector3(0, 5, 0), colour), 
            new(new Vector3(5, 0, 0), colour),
            new(new Vector3(0, -5, 0), colour), 
            new(new Vector3(-5, 0, 0), colour)];
        int[] indices = [0, 1, 3, 3, 1, 2];
        
        Effect effect = Effects.DefaultEffect ?? throw new NullReferenceException("Effect not initialized yet");;
        effect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(position.X, position.Y, 0)));
        effect.Parameters["Colour"].SetValue(colour.ToVector4());
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, 2
            );
        }
    }

    public void SD_DrawPoint(Camera camera, Vec2<SDecimal> position, Color colour)
    {
        DrawPoint(camera.ConvertToScreenCoordinates(position), colour);
    }

    public void DrawMesh(IMesh mesh, Matrix transform, Dictionary<string, object> shaderParameters, Effect? effect)
    {
        mesh.TryGenerateBuffers(GraphicsDevice);
        mesh.Draw(GraphicsDevice, transform, shaderParameters, effect);
    }

    public void DrawMesh(IMesh mesh, Vector2 scale, Vector2 position, Vector2 rotation, Color colour)
    {
        mesh.TryGenerateBuffers(GraphicsDevice);
        throw new NotImplementedException();
    }

    public void DrawText(SpriteFont spriteFont, string text, Vector2 position, Color colour)
    {
        spriteBatch.DrawString(spriteFont, text, position, colour);
    }

    public void DrawText(SpriteFont spriteFont, string text, Vector2 position, Vector2 scale, float rotation, Color colour)
    {
        spriteBatch.DrawString(spriteFont, text, position, colour, rotation, Vector2.Zero, scale, SpriteEffects.None, 0);
    }
}