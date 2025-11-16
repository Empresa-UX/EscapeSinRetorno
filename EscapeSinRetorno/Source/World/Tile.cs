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

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_layers.Count > 0)
            {
                // Se dibuja en coordenadas de mundo.
                // La cámara se aplica vía transformMatrix en Game1.
                spriteBatch.Draw(
                    _layers[0],
                    Position,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    0f);
            }
        }
    }
}