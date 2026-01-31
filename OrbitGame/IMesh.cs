using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public interface IMesh
{
    public void GenerateBuffers(GraphicsDevice graphicsDevice);

    public void Draw(GraphicsDevice graphicsDevice, Effect effect, Matrix transform,
        Dictionary<string, object> shaderParameters);
}