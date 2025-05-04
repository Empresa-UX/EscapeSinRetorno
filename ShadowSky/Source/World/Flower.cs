using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.Source.World
{
    class Flower
    {
        public Texture2D Texture { get; }
        public Vector2 Position { get; }

        public Flower(Texture2D texture, Vector2 position)
        {
            Texture = texture;
            Position = position;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            spriteBatch.Draw(Texture, Position - camera, Color.White);
        }
    }
}
