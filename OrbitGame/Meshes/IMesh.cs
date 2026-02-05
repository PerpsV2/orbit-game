using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public interface IMesh
{
    public void GenerateBuffers();

    public void Draw(GraphicsDevice graphics, Effect effect, Matrix transform, Dictionary<string, object> shaderParameters);
}