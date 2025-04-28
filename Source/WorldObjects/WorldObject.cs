using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ShadowSky.Entities;

public abstract class WorldObject
{
    public Texture2D Texture { get; set; }
    public Vector2 Position { get; set; }
    public bool IsCollidable { get; set; }
    public string Name { get; set; }
    public string TextureName { get; set; }

    public WorldObject(Texture2D texture, Vector2 position, bool isCollidable)
    {
        Texture = texture;
        Position = position;
        IsCollidable = isCollidable;
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Texture, Position, Color.White);
    }

    public abstract void OnInteract(Player player);
}
