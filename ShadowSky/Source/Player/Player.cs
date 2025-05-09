using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShadowSky.World;

namespace ShadowSky.Entities
{
    public class Player
    {
        public Vector2 Position { get; private set; }
        private readonly Texture2D _texture;
        private readonly int _width, _height;
        private readonly float _speed;

        public Player(Texture2D texture, Vector2 startPos, int width, int height, float speed)
        {
            _texture = texture;
            Position = startPos;
            _width = width;
            _height = height;
            _speed = speed;
        }

        public void Update(KeyboardState input, TileMap tileMap, int mapWidth, int mapHeight, float deltaTime)
        {
            Vector2 direction = Vector2.Zero;

            if (input.IsKeyDown(Keys.W)) direction.Y -= 1;
            if (input.IsKeyDown(Keys.S)) direction.Y += 1;
            if (input.IsKeyDown(Keys.A)) direction.X -= 1;
            if (input.IsKeyDown(Keys.D)) direction.X += 1;

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                Vector2 nextPos = Position + direction * _speed * deltaTime;

                int tileX = (int)(nextPos.X / tileMap.TileSize);
                int tileY = (int)(nextPos.Y / tileMap.TileSize);

                var type = tileMap.GetTileType(tileX, tileY);
                if (type != TileType.Stone && type != TileType.Water)
                {
                    Position = nextPos;
                }
            }

            Position = new Vector2(
                MathHelper.Clamp(Position.X, 0, mapWidth - 1),
                MathHelper.Clamp(Position.Y, 0, mapHeight - 1)
            );
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            Vector2 screenPos = Position - camera;
            Rectangle dest = new((int)screenPos.X - _width / 2, (int)screenPos.Y - _height / 2, _width, _height);
            spriteBatch.Draw(_texture, dest, Color.White);
        }
    }
}
