using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.World
{
    public class Tile
    {
        public TileType Type { get; }
        public Texture2D Texture { get; }
        public Vector2 Position { get; }

        public Tile(TileType type, Texture2D texture, Vector2 position)
        {
            Type = type;
            Texture = texture;
            Position = position;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }
    }
}
