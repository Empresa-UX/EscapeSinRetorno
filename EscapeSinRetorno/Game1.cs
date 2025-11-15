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

        private bool _isInMenu = true;
        private bool _wasDead = false;              // trackea transición a pantalla de muerte
        private MenuState _menuState;

        private StatsHud _hud;
        private VignetteOverlay _overlay;
        private DeathScreen _deathScreen;
        private SpriteFont _hudFont;

        public static readonly System.Random Random = new System.Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            // Suscripciones correctas (NO override OnExiting)
            this.Exiting += OnGameExiting;                    // EventHandler<EventArgs>
            this.Window.ClientSizeChanged += OnClientSizeChanged; // reubicar UI si cambia tamaño
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _menuState = new MenuState(this);
            _menuState.LoadContent(Content, GraphicsDevice);

            _overlay = new VignetteOverlay(GraphicsDevice);

            // Usa tu fuente existente
            _hudFont = Content.Load<SpriteFont>("Fonts/MenuFont");
            _hud = new StatsHud(GraphicsDevice, _hudFont);

            // DeathScreen con acciones reales
            _deathScreen = new DeathScreen(
                GraphicsDevice,
                _hudFont,
                onRetry: StartGame,
                onMenu: ReturnToMenu
            );
        }

        // Reubica botones de DeathScreen si cambia el tamaño de la ventana
        private void OnClientSizeChanged(object sender, EventArgs e)
        {
            _deathScreen?.Resize(GraphicsDevice.Viewport, StartGame, ReturnToMenu);
        }

        // Limpieza opcional al salir del juego
        private void OnGameExiting(object sender, EventArgs e)
        {
            _hud?.Dispose();
            _spriteBatch?.Dispose();
            // Agregá aquí más Dispose/guardado de estado si lo necesitás.
        }

        public void StartGame()
        {
            _tileMap = new TileMap(tileSize: 16);
            _tileMap.LoadContent(Content);

            _player = new Player();
            _player.LoadContent(Content, GraphicsDevice);
            if (_tileMap.PlayerStartPosition.HasValue)
                _player.SetPosition(_tileMap.PlayerStartPosition.Value);

            _enemyManager = new EnemyManager();
            _enemyManager.SpawnFromMapData(_tileMap.EnemySpawns);
            _enemyManager.LoadContent(Content);
            Enemy.LoadDebugTexture(GraphicsDevice);

            _camera = new Camera2D(GraphicsDevice.Viewport);
            _camera.SetZoom(5.0f);

            _isInMenu = false;
            _wasDead = false;
            IsMouseVisible = false; // ocultar mouse en gameplay
        }

        public void ReturnToMenu()
        {
            _isInMenu = true;
            IsMouseVisible = true; // mostrar mouse en menú
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) Exit();
            InputManager.Update();

            if (_isInMenu)
            {
                _menuState.Update(gameTime);
                _menuState.HandleInput();
                base.Update(gameTime);
                return;
            }

            bool isDead = _player?.Stats?.IsDead == true;

            // Mostrar/ocultar mouse al entrar/salir de la pantalla de muerte
            if (isDead && !_wasDead) IsMouseVisible = true;
            if (!isDead && _wasDead) IsMouseVisible = false;
            _wasDead = isDead;

            if (isDead)
            {
                _deathScreen.Update(gameTime);
                base.Update(gameTime);
                return;
            }

            _enemyManager.Update(gameTime, _player, _tileMap);
            _player.Update(gameTime, _tileMap);
            _camera.Follow(_player.Position, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

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
                // Mundo
                _spriteBatch.Begin(transformMatrix: _camera.GetTransform());
                _tileMap.DrawBackground(_spriteBatch, Vector2.Zero, 1366, 768);
                _tileMap.Draw(_spriteBatch, Vector2.Zero);
                _enemyManager.Draw(_spriteBatch);
                _player.Draw(_spriteBatch);
                _spriteBatch.End();

                // Overlay + HUD + DeathScreen
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                _overlay.Draw(_spriteBatch, GraphicsDevice.Viewport, _player.CurrentFx);
                _hud.Draw(
                    _spriteBatch,
                    _player.Stats,
                    new Point(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
                );

                if (_player.Stats.IsDead)
                    _deathScreen.Draw(_spriteBatch, GraphicsDevice.Viewport);

                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hud?.Dispose();
                _spriteBatch?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}