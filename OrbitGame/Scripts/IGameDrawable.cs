using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public interface IGameDrawable
{
    public void Draw(GraphicsDevice graphicsDevice, Camera camera);

    public void DrawCollider(GraphicsDevice graphicsDevice, Camera camera);
}