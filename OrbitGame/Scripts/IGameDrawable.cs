using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public interface IGameDrawable
{
    public void Draw(GraphicsDevice graphicsDevice, Camera camera, Effect effect);

    public void DrawCollider(GraphicsDevice graphicsDevice, Camera camera, Effect effect);
}