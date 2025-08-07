using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Core
{
    public interface IGameState
    {
        void LoadContent(ContentManager content, GraphicsDevice graphicsDevice);
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
        void HandleInput();
    }
}