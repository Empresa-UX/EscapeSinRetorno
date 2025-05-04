using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShadowSky.World;
using System;
using System.Diagnostics;

namespace ShadowSky
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _playerTexture;
        private Vector2 _playerPosition;
        private Vector2 _camera;
        private float _playerSpeed = 150f;

        private TileMap _tileMap;

        private KeyboardState _currentKey;
        private KeyboardState _previousKey;

        private int _mapWidth, _mapHeight;
        private const int TileSize = 16;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // No borrar: _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            _playerPosition = new Vector2(85 * TileSize + TileSize / 2, 45 * TileSize + TileSize / 2);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _playerTexture = Content.Load<Texture2D>("Characters/player");

            _tileMap = new TileMap(TileSize);
            _tileMap.LoadContent(Content);

            _mapWidth = 255 * TileSize;
            _mapHeight = 144 * TileSize;
        }

        protected override void Update(GameTime gameTime)
        {
            _previousKey = _currentKey;
            _currentKey = Keyboard.GetState();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 movement = Vector2.Zero;

            if (_currentKey.IsKeyDown(Keys.W)) movement.Y -= 1;
            if (_currentKey.IsKeyDown(Keys.S)) movement.Y += 1;
            if (_currentKey.IsKeyDown(Keys.A)) movement.X -= 1;
            if (_currentKey.IsKeyDown(Keys.D)) movement.X += 1;

            if (movement != Vector2.Zero)
            {
                movement.Normalize();
                Vector2 potentialPos = _playerPosition + movement * _playerSpeed * dt;

                int tileX = (int)(potentialPos.X / TileSize);
                int tileY = (int)(potentialPos.Y / TileSize);

                if (_tileMap.GetTileType(tileX, tileY) != TileType.Stone)
                {
                    _playerPosition = potentialPos;
                }
            }

            // Clampeo del jugador dentro del mapa (opcional, por seguridad)
            _playerPosition.X = MathHelper.Clamp(_playerPosition.X, 0, _mapWidth - 1);
            _playerPosition.Y = MathHelper.Clamp(_playerPosition.Y, 0, _mapHeight - 1);

            // --- CÁLCULO DE CÁMARA ---
            float halfScreenW = _graphics.PreferredBackBufferWidth / 2f;
            float halfScreenH = _graphics.PreferredBackBufferHeight / 2f;

            // Calcula la posición ideal de la cámara centrada en el jugador
            float idealCamX = _playerPosition.X - halfScreenW;
            float idealCamY = _playerPosition.Y - halfScreenH;

            // Calcula los límites máximos de la cámara
            float maxCamX = _mapWidth - _graphics.PreferredBackBufferWidth;
            float maxCamY = _mapHeight - _graphics.PreferredBackBufferHeight;

            // Clampea para evitar mostrar fuera del mapa
            _camera.X = MathHelper.Clamp(idealCamX, 0, Math.Max(0, maxCamX));
            _camera.Y = MathHelper.Clamp(idealCamY, 0, Math.Max(0, maxCamY));

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _tileMap.Draw(
                _spriteBatch,
                _camera,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight
            );

            // Calcula la posición del jugador en pantalla, relativa a la cámara
            Vector2 screenPos = _playerPosition - _camera;

            Rectangle destination = new(
                (int)screenPos.X - 22,
                (int)screenPos.Y - 22,
                45, 45
            );

            _spriteBatch.Draw(_playerTexture, destination, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
