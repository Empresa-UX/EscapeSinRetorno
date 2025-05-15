using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSinRetorno.Source.World;

namespace EscapeSinRetorno.Source.Entities
{
	public class Player
	{
		private Texture2D _texture;
		private Vector2 _position;
		private Vector2 _velocity;
		private float _speed = 100f;

		private int _frameWidth = 128;
		private int _frameHeight = 128;

		private int _hitboxWidth = 32;
		private int _hitboxHeight = 32;

		private int _currentFrame = 0;
		private double _timer, _interval = 100;

		private SpriteEffects _flip = SpriteEffects.None;
		private bool _isMoving = false;

		private Texture2D _debugPixel; // Para dibujar el hitbox

		public int Width => _hitboxWidth;
		public int Height => _hitboxHeight;

		public Vector2 HitboxPosition => new Vector2(
			_position.X + (_frameWidth - _hitboxWidth) / 2,
			_position.Y + (_frameHeight - _hitboxHeight)
		);

		public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
		{
			_texture = content.Load<Texture2D>("Characters/Enchantress/Walk");
			_position = new Vector2(300, 300);

			_debugPixel = new Texture2D(graphicsDevice, 1, 1);
			_debugPixel.SetData(new[] { Color.White });
		}


        public void Update(GameTime gameTime, TileMap tileMap)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState ks = Keyboard.GetState();
            Vector2 input = Vector2.Zero;

            if (ks.IsKeyDown(Keys.D))
            {
                input.X += 1;
                _flip = SpriteEffects.None;
            }
            else if (ks.IsKeyDown(Keys.A))
            {
                input.X -= 1;
                _flip = SpriteEffects.FlipHorizontally;
            }

            if (ks.IsKeyDown(Keys.W))
                input.Y -= 1;
            else if (ks.IsKeyDown(Keys.S))
                input.Y += 1;

            _isMoving = input != Vector2.Zero;

            Vector2 newPosX = Vector2.Zero;
            Vector2 newPosY = Vector2.Zero;

            if (_isMoving)
            {
                input.Normalize();
                _velocity = input * _speed * deltaTime;

                newPosX = new Vector2(HitboxPosition.X + _velocity.X, HitboxPosition.Y);
                if (!tileMap.IsColliding(newPosX, Width, Height))
                    _position.X += _velocity.X;

                newPosY = new Vector2(HitboxPosition.X, HitboxPosition.Y + _velocity.Y);
                if (!tileMap.IsColliding(newPosY, Width, Height))
                    _position.Y += _velocity.Y;

                Animate(gameTime);
            }
            else
            {
                _velocity = Vector2.Zero;
                _currentFrame = 0;
            }
        }


        private void Animate(GameTime gameTime)
		{
			_timer += gameTime.ElapsedGameTime.TotalMilliseconds;
			if (_timer > _interval)
			{
				_currentFrame = (_currentFrame + 1) % 8; // Cambiar si tu sprite tiene otro número de frames
				_timer = 0;
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			if (_texture == null)
				return;

			// Dibuja al personaje
			Rectangle source = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
			spriteBatch.Draw(_texture, _position, source, Color.White, 0f, Vector2.Zero, 1f, _flip, 0f);

			// Dibuja el área de colisión para debug (transparente)
			Rectangle hitboxRect = new Rectangle(
				(int)HitboxPosition.X, (int)HitboxPosition.Y,
				_hitboxWidth, _hitboxHeight
			);
			spriteBatch.Draw(_debugPixel, hitboxRect, Color.Red * 0.4f); // podés quitar esta línea cuando no lo necesites
		}
	}
}





