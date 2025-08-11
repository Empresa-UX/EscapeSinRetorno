using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSinRetorno.Source.World;
using EscapeSinRetorno.Source.Entities;
using EscapeSinRetorno.Source.Entities.Enemies;
using EscapeSinRetorno.Source.Core;
using EscapeSinRetorno.Source.UI;

namespace EscapeSinRetorno
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private EnemyManager _enemyManager;
        private TileMap _tileMap;
        private Player _player;
        private Camera2D _camera;

        // Variables para controlar si estamos en menú o en juego
        private bool _isInMenu = true;
        private MenuState _menuState;

        public static readonly System.Random Random = new System.Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Cargar el menú primero
            _menuState = new MenuState(this);
            _menuState.LoadContent(Content, GraphicsDevice);
        }

        // Método público para que el menú pueda iniciar el juego
        public void StartGame()
        {
            if (!_isInMenu) return; // Ya está en juego

            // Cargar todo el contenido del juego
            _tileMap = new TileMap(tileSize: 16);
            _tileMap.LoadContent(Content);

            _player = new Player();
            _player.LoadContent(Content, GraphicsDevice);

            if (_tileMap.PlayerStartPosition.HasValue)
            {
                _player.SetPosition(_tileMap.PlayerStartPosition.Value);
            }

            _enemyManager = new EnemyManager();
            _enemyManager.SpawnFromMapData(_tileMap.EnemySpawns);
            _enemyManager.LoadContent(Content);

            Enemy.LoadDebugTexture(GraphicsDevice);

            _camera = new Camera2D(GraphicsDevice.Viewport);
            _camera.SetZoom(5.0f);

            _isInMenu = false;
            IsMouseVisible = false; // Ocultar mouse en el juego
        }

        // Método público para volver al menú
        public void ReturnToMenu()
        {
            _isInMenu = true;
            IsMouseVisible = true;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            InputManager.Update();

            if (_isInMenu)
            {
                _menuState.Update(gameTime);
                _menuState.HandleInput();
            }
            else
            {
                // Tu código original del juego
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    ReturnToMenu();
                    return;
                }

                _enemyManager.Update(gameTime, _player, _tileMap);
                _player.Update(gameTime, _tileMap);
                _camera.Follow(_player.Position , _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (_isInMenu)
            {
                _menuState.Draw(_spriteBatch);
            }
            else
            {
                // Tu código original de dibujo del juego
                _spriteBatch.Begin(transformMatrix: _camera.GetTransform());
                _tileMap.DrawBackground(_spriteBatch, Vector2.Zero, 1366, 768);
                _tileMap.Draw(_spriteBatch, Vector2.Zero);
                _enemyManager.Draw(_spriteBatch);
                _player.Draw(_spriteBatch);
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}