// Tile.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace EscapeSinRetorno.Source.World
{
    public class Tile
    {
        private readonly List<Texture2D> _layers;
        public Vector2 Position { get; }

        public Tile(List<Texture2D> layers, Vector2 pos)
        {
            _layers = layers;
            Position = pos;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            foreach (var tex in _layers)
                spriteBatch.Draw(tex, Position - camera, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
        }
    }
}