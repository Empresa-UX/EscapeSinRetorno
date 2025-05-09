using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShadowSky.World;
using ShadowSky.Entities;
using System;

namespace ShadowSky
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Player _player;
        private TileMap _tileMap;

        private Vector2 _camera;

        private int _mapWidth, _mapHeight;
        private const int TileSize = 16;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _tileMap = new TileMap(TileSize);
            _tileMap.LoadContent(Content);

            _mapWidth = 255 * TileSize;
            _mapHeight = 144 * TileSize;

            _player = new Player(
                Content.Load<Texture2D>("Characters/player"),
                new Vector2(85 * TileSize + TileSize / 2, 45 * TileSize + TileSize / 2),
                45, 45,
                150f
            );
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _player.Update(Keyboard.GetState(), _tileMap, _mapWidth, _mapHeight, dt);

            float halfW = _graphics.PreferredBackBufferWidth / 2f;
            float halfH = _graphics.PreferredBackBufferHeight / 2f;

            float idealX = _player.Position.X - halfW;
            float idealY = _player.Position.Y - halfH;

            float maxX = _mapWidth - _graphics.PreferredBackBufferWidth;
            float maxY = _mapHeight - _graphics.PreferredBackBufferHeight;

            _camera.X = MathHelper.Clamp(idealX, 0, Math.Max(0, maxX));
            _camera.Y = MathHelper.Clamp(idealY, 0, Math.Max(0, maxY));

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _tileMap.Draw(_spriteBatch, _camera, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _player.Draw(_spriteBatch, _camera);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
