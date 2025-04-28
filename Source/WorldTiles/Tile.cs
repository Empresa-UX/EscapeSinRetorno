using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadowSky.WorldTiles
{
    public abstract class Tile
    {
        public string Name { get; protected set; }
        public string TextureName { get; protected set; }
        public Texture2D Texture { get; set; } // La textura del tile

        public const int Size = 32; // Tamaño del tile

        public Tile()
        {
            
        }

        public virtual void Draw(int x, int y)
        {
            if (Texture != null)
            {
                Globals.SpriteBatch.Draw(Texture, new Rectangle(x, y, Size, Size), Color.White);
            }
        }
    }
}
