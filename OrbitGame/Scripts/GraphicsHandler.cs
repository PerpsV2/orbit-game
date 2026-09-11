using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using qQEngine;

namespace OrbitGame;

public class GraphicsHandler(SpriteBatch spriteBatch) : IGraphicsHandler
{
    private GraphicsDevice GraphicsDevice => spriteBatch.GraphicsDevice;
    
    private static readonly CircularMesh CircleMesh = new();
    private static readonly ScreenMesh ScreenMesh = new();
    
    public void DrawShadowMask(qQEngine.Camera camera, CircularLight light, qQEngine.Body occluderBody)
    {
        foreach (var occluder in occluderBody.Occluder.Occluders)
        {
            switch (occluder)
            {
                case CircularOccluder o:
                    Effect shadowEffect = Effects.ShadowEffect ?? throw new NullReferenceException("Effect not initialized yet");
                    DrawMesh(ScreenMesh, Matrix.Identity, new Dictionary<string, object>
                    {
                        { "LightCenter", camera.ConvertToScreenCoordinates(light.Position) },
                        { "LightRadius", camera.ConvertToScreenDistance(70) },
                        { "OccluderCenter", camera.ConvertToScreenCoordinates(occluderBody.Position + o.LocalPosition) },
                        { "OccluderRadius", camera.ConvertToScreenDistance(o.Radius) },
                        { "ScreenSize", new Vector2(Options.ScreenSize.width, Options.ScreenSize.height) }
                    }, shadowEffect);
                    break;
                
                case PolyOccluder o:
                    Effect polyShadowEffect = Effects.PolyShadowEffect ?? throw new NullReferenceException("Effect not initialized yet");
                    DrawMesh(ScreenMesh, Matrix.Identity, new Dictionary<string, object>
                    {
                        { "LightCenter", camera.ConvertToScreenCoordinates(light.Position) },
                        { "LightRadius", camera.ConvertToScreenDistance(70) },
                        { "OccluderVertices", o.Vertices.Select(x => camera.ConvertToScreenCoordinates(occluderBody.Position + x + o.LocalPosition)).ToArray() },
                        { "VerticesActiveCount", o.Vertices.Count },
                        { "ScreenSize", new Vector2(Options.ScreenSize.width, Options.ScreenSize.height) }
                    }, polyShadowEffect);
                    break;
            }
        }
    }

    public void DrawShadowMask2(qQEngine.Camera camera, CircularLight light, IEnumerable<qQEngine.Body> bodies)
    {
        List<Vector4> occluderLineData = new();
        List<Vector4> circularOccluderData = new();
        foreach (var body in bodies)
        {
            foreach (var occluder in body.Occluder.Occluders)
            {
                if (occluder is PolyOccluder o)
                {
                    for (int i = 0; i < o.Vertices.Count; i++)
                    {
                        int n = (i + 1) % o.Vertices.Count;
                        Vector2 segStart = camera.ConvertToScreenCoordinates(body.Position + o.LocalPosition + o.Vertices[i]);
                        Vector2 segEnd = camera.ConvertToScreenCoordinates(body.Position + o.LocalPosition + o.Vertices[n]);
                        occluderLineData.Add(new Vector4(segStart.X, segStart.Y, segEnd.X, segEnd.Y));
                    }
                }

                if (occluder is CircularOccluder c)
                {
                    Vector2 center = camera.ConvertToScreenCoordinates(body.Position + c.LocalPosition);
                    float radius = camera.ConvertToScreenDistance(c.Radius);
                    circularOccluderData.Add(new Vector4(center.X, center.Y, radius, 0));
                }
            }
        }

        Vector4[] occluderLineDataArray = occluderLineData.ToArray();
        Texture2D occluderLineDataTexture = new Texture2D(GraphicsDevice, occluderLineDataArray.Length, 1, false, SurfaceFormat.Vector4);
        occluderLineDataTexture.SetData(occluderLineDataArray);

        Vector4[] cOccluderDataArray = circularOccluderData.ToArray();
        Texture2D cOccluderDataTexture = new Texture2D(GraphicsDevice, cOccluderDataArray.Length, 1, false, SurfaceFormat.Vector4);
        cOccluderDataTexture.SetData(cOccluderDataArray);
        
        Effect globalShadowEffect = Effects.GlobalShadowEffect ?? throw new NullReferenceException("Effect not initialized yet");
        DrawMesh(ScreenMesh, Matrix.Identity, new Dictionary<string, object>
        {
            { "LightCenter", camera.ConvertToScreenCoordinates(light.Position) },
            { "LightRadius", camera.ConvertToScreenDistance(70) },
            { "OccluderTextureBuffer", occluderLineDataTexture},
            { "OccluderCount", occluderLineDataArray.Length },
            { "COccluderTextureBuffer", cOccluderDataTexture },
            { "COccluderCount", cOccluderDataArray.Length },
            { "ScreenSize", new Vector2(Options.ScreenSize.width, Options.ScreenSize.height) }
        }, globalShadowEffect);
    }

    public void DrawLighting(qQEngine.Camera camera, CircularLight light, RenderTarget2D shadowMask,
        RenderTarget2D occluderMask)
    {
        Effect effect = Effects.LightingEffect ?? throw new NullReferenceException("Effect not initialized yet");
        DrawMesh(ScreenMesh, Matrix.Identity, new Dictionary<string, object>
        {
            {"LightColour", light.Colour.ToVector4() * (float)light.Luminosity},
            {"LightCenter", camera.ConvertToScreenCoordinates(light.Position)},
            {"DistanceScale", 1 / camera.ConvertToScreenDistance(1)},
            {"ScreenSize", new Vector2(Options.ScreenSize.width, Options.ScreenSize.height)},
            {"ShadowMask", shadowMask},
            {"OccluderMask", occluderMask}
        }, effect);
    }

    public void DrawOccluder(qQEngine.Camera camera, qQEngine.Vec2Double position, IOccluder occluder, Color colour)
    {
        if (occluder is CircularOccluder o)
            DrawCircle(camera.ConvertToScreenCoordinates(position + o.LocalPosition), 
                camera.ConvertToScreenDistance(o.Radius), colour);
    }

    public void DrawScreenMesh(Dictionary<string, object> shaderParameters, Effect? effect)
    {
        DrawMesh(ScreenMesh, Matrix.Identity, shaderParameters, effect);
    }

    public void DrawCircle(Vector2 center, float radius, Color colour)
    {
        Matrix transform = Matrix.CreateScale(radius, radius, 1) *
                           Matrix.CreateTranslation(new Vector3(center.X, center.Y, 0));
        Effect effect = Effects.CircleEffect ?? throw new NullReferenceException("Effect not initialized yet");
        DrawMesh(CircleMesh, transform, new Dictionary<string, object>{{"Colour", colour.ToVector4()}}, effect);
    }

    public void DrawLighting(Vector2 lightOrigin, float distanceScale)
    {
        RenderTarget2D lightingMask = new RenderTarget2D(GraphicsDevice, Options.ScreenSize.width, Options.ScreenSize.height);
        GraphicsDevice.SetRenderTarget(lightingMask);
        spriteBatch.Begin();
        spriteBatch.End();
        Effect effect = Effects.LightingEffect ?? throw new NullReferenceException("Effect not initialized yet");
        DrawMesh(ScreenMesh, Matrix.Identity, new Dictionary<string, object>
        {
            {"Colour", Color.Red.ToVector4()},
            {"LightOrigin", lightOrigin},
            {"DistanceScale", distanceScale}
        }, effect);
    }

    public void DrawPoly(List<Vector2> points, Color colour)
    {
        if (points.Count < 3) return;
        
        var vertices = new VertexPositionTexture[points.Count];
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = new VertexPositionTexture(new Vector3(points[i].X, points[i].Y, 0), Vector2.Zero);
        
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