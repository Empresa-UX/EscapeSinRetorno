using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShadowSky.World;

namespace ShadowSky.Source.Player
{
    public class Player
    {
        public Vector2 Position { get; private set; }
        private readonly Texture2D _texture;
        private readonly int _width, _height;
        private readonly float _baseSpeed;
        public PlayerStats Stats { get; private set; }

        private bool _isCollapsed = false;
        private float _collapseTimer = 0f;
        private float _fadeAlpha = 0f;

        public Player(Texture2D texture, Vector2 startPos, int width, int height, float speed)
        {
            _texture = texture;
            Position = startPos;
            _width = width;
            _height = height;
            _baseSpeed = speed;
            Stats = new PlayerStats();
        }

        public void Update(KeyboardState input, TileMap tileMap, int mapWidth, int mapHeight, float deltaTime)
        {
            Stats.Update(deltaTime);
            ApplyStatPenalties(deltaTime);

            if (_isCollapsed)
            {
                _collapseTimer -= deltaTime;
                if (_collapseTimer <= 0f) _isCollapsed = false;
                return;
            }

            if (!Stats.CanMove || !Stats.CanAct)
                return;

            Vector2 direction = Vector2.Zero;

            bool invert = Stats.ControlsInverted;

            if (GetKey(input, Keys.W, Keys.Up, invert)) direction.Y -= 1;
            if (GetKey(input, Keys.S, Keys.Down, invert)) direction.Y += 1;
            if (GetKey(input, Keys.A, Keys.Left, invert)) direction.X -= 1;
            if (GetKey(input, Keys.D, Keys.Right, invert)) direction.X += 1;

            float speed = _baseSpeed * Stats.SpeedMultiplier;

            if (input.IsKeyDown(Keys.X) && Stats.CanSprint)
            {
                speed *= 1.5f;
                Stats.SpendStamina(deltaTime * 10);
            }

            if (direction != Vector2.Zero)
            {
                direction.Normalize();

                if (Stats.StunTimer > 0f)
                {
                    Stats.StunTimer -= deltaTime;
                    return;
                }

                if (Stats.IsDelirious && new System.Random().NextDouble() < 0.05)
                {
                    direction *= -1; // movimiento errático
                }

                Vector2 nextPos = Position + direction * speed * deltaTime;

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

        private bool GetKey(KeyboardState input, Keys main, Keys alt, bool inverted)
        {
            return inverted ? input.IsKeyDown(alt) : input.IsKeyDown(main) || input.IsKeyDown(alt);
        }
        public void DrawEffects(SpriteBatch spriteBatch, Texture2D fadeTexture, int screenWidth, int screenHeight)
        {
            if (_fadeAlpha > 0f)
            {
                spriteBatch.Draw(fadeTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * _fadeAlpha);
            }
        }

        private void ApplyStatPenalties(float dt)
        {
            if (!_isCollapsed && Stats.Fatigue >= 100)
            {
                _isCollapsed = true;
                _collapseTimer = 5f;
            }

            if (!_isCollapsed && Stats.Hunger <= 5 && Stats.Thirst <= 5 && Stats.Fatigue >= 90)
            {
                _isCollapsed = true;
                _collapseTimer = 6f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            Vector2 screenPos = Position - camera;
            Rectangle dest = new((int)screenPos.X - _width / 2, (int)screenPos.Y - _height / 2, _width, _height);
            spriteBatch.Draw(_texture, dest, Color.White);
        }
    }
}
