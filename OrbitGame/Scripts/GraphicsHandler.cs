using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public class GraphicsHandler(GraphicsDevice graphicsDevice) : IGraphicsHandler
{
    public void DrawPoly(List<Vector2> points, Color colour)
    {
        if (points.Count == 0) return;
        
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
            graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3
            );
        }
    }

    public void DrawLine(Vector2 start, Vector2 end, Color colour)
    {
        VertexPositionColor[] vertices = [new(new Vector3(start.X, start.Y, 0), Color.White), new(new Vector3(end.X, end.Y, 0), Color.White)];
        int[] indices = [0, 1];
        
        Effect effect = Effects.DefaultEffect ?? throw new NullReferenceException("Effect not initialized yet");
        effect.Parameters["World"].SetValue(Matrix.Identity);
        effect.Parameters["Colour"].SetValue(colour.ToVector4());
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.LineList, vertices, 0, vertices.Length, indices, 0, 1
            );
        }
    }

    public void DrawLineR(Vector2 start, Vector2 displacement, Color colour)
    {
        DrawLine(start, start + displacement, colour);
    }

    public void SD_DrawLine(Camera camera, SD_Vector2 start, SD_Vector2 end, Color colour)
    {
        Vector2 screenStart = camera.ConvertToScreenCoordinates(start);
        Vector2 screenEnd = camera.ConvertToScreenCoordinates(end);
        DrawLine(screenStart, screenEnd, colour);
    }

    public void SD_DrawLineR(Camera camera, SD_Vector2 start, SD_Vector2 displacement, Color colour)
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
            graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, 2
            );
        }
    }

    public void SD_DrawPoint(Camera camera, SD_Vector2 position, Color colour)
    {
        DrawPoint(camera.ConvertToScreenCoordinates(position), colour);
    }

    public void DrawMesh(IMesh mesh, Matrix transform, Dictionary<string, object> shaderParameters)
    {
        mesh.TryGenerateBuffers(graphicsDevice);
        mesh.Draw(graphicsDevice, transform, shaderParameters);
    }
}