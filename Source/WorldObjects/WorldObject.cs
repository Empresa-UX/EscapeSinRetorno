using System;

public abstract class WorldObject
{
    public Texture2D Texture;
    public Vector2 Position;
    public bool IsCollidable;

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
