using EscapeSinRetorno.Source.Inventory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Items
{
    public class ItemPickup
    {
        public string ItemId;
        public Vector2 Position;
        private Texture2D _texture;

        private const int Size = 16;

        public ItemPickup(string id, Vector2 pos)
        {
            ItemId = id;
            Position = pos;
        }

        public void LoadContent(ContentManager content)
        {
            if (ItemDatabase.Items.TryGetValue(ItemId, out var item) && item.Icon != null)
                _texture = item.Icon;
        }

        public Rectangle Bounds => new Rectangle(
            (int)Position.X, (int)Position.Y, Size, Size
        );

        public void Draw(SpriteBatch sb)
        {
            if (_texture != null)
                sb.Draw(_texture, Bounds, Color.White);
        }
    }
}
