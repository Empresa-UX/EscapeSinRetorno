using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSinRetorno.Source.World;
using EscapeSinRetorno.Source.Entities;
using EscapeSinRetorno.Source.Entities.Enemies;
using EscapeSinRetorno.Source.Core;
using EscapeSinRetorno.Source.UI;
using EscapeSinRetorno.Source.Multiplayer;
using EscapeSinRetorno.Source.Net;

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
        private bool _wasDead = false;
        private MenuState _menuState;

        private StatsHud _hud;
        private VignetteOverlay _overlay;
        private DeathScreen _deathScreen;
        private SpriteFont _hudFont;

        private MultiplayerManager _mp;

        private Process _serverProc;

        private float _netSendTimer;
        private const float NET_SEND_DT = 1f / 20f;

        public static readonly System.Random Random = new System.Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            this.Exiting += OnGameExiting;
            this.Window.ClientSizeChanged += OnClientSizeChanged;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _menuState = new MenuState(this);
            _menuState.LoadContent(Content, GraphicsDevice);

            _overlay = new VignetteOverlay(GraphicsDevice);

            _hudFont = Content.Load<SpriteFont>("Fonts/MenuFont");
            _hud = new StatsHud(GraphicsDevice, _hudFont);

            _deathScreen = new DeathScreen(GraphicsDevice, _hudFont, onRetry: StartGame, onMenu: ReturnToMenu);

            _mp = new MultiplayerManager();
        }

        // ====== MODOS (helpers) ======

        public void StartSingleplayer()
        {
            StartGame(); // no llamar a EnableMultiplayer -> OFFLINE
        }

        public void StartLocalhostClient()
        {
            StartGame();
            EnableMultiplayer("127.0.0.1");
        }

        public void HostAndJoin()
        {
            if (TryStartLocalServer(7777))
            {
                StartGame();
                EnableMultiplayer("127.0.0.1");
            }
        }

        public void JoinByIp(string host)
        {
            StartGame();
            EnableMultiplayer(host);
        }

        // ====== Red ======

        public bool TryStartLocalServer(int port = 7777)
        {
            string[] candidates =
            {
                System.IO.Path.GetFullPath(@"..\..\..\..\ServerConsole\ServerConsole\bin\Debug\net9.0\ServerConsole.exe"),
                System.IO.Path.GetFullPath(@"..\ServerConsole\bin\Debug\net9.0\ServerConsole.exe"),
            };

            foreach (var path in candidates)
            {
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        _serverProc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = path,
                                Arguments = "",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            },
                            EnableRaisingEvents = true
                        };
                        _serverProc.Start();
                        return true;
                    }
                    catch { }
                }
            }
            // Si no se encontró, permitir seguir (server manual)
            return true;
        }

        public void EnableMultiplayer(string host)
        {
            _mp.StartClient(Content, host, name: "Player");
        }

        private void OnClientSizeChanged(object sender, EventArgs e)
        {
            _deathScreen?.Resize(GraphicsDevice.Viewport, StartGame, ReturnToMenu);
        }

        private void OnGameExiting(object sender, EventArgs e)
        {
            try
            {
                if (_serverProc != null && !_serverProc.HasExited)
                {
                    _serverProc.Kill(entireProcessTree: true);
                    _serverProc.Dispose();
                }
            }
            catch { }

            _hud?.Dispose();
            _spriteBatch?.Dispose();
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
            IsMouseVisible = false;
        }

        public void ReturnToMenu()
        {
            _isInMenu = true;
            IsMouseVisible = true;
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

            if (isDead && !_wasDead) IsMouseVisible = true;
            if (!isDead && _wasDead) IsMouseVisible = false;
            _wasDead = isDead;

            if (isDead)
            {
                _deathScreen.Update(gameTime);
                base.Update(gameTime);
                return;
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // MULTI (si está activo)
            if (_mp?.Client != null)
            {
                var ks = Keyboard.GetState();
                Vector2 dir = Vector2.Zero;
                if (ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D)) dir.X++;
                if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A)) dir.X--;
                if (ks.IsKeyDown(Keys.Up) || ks.IsKeyDown(Keys.W)) dir.Y--;
                if (ks.IsKeyDown(Keys.Down) || ks.IsKeyDown(Keys.S)) dir.Y++;

                bool run = ks.IsKeyDown(Keys.X);
                bool attack = ks.IsKeyDown(Keys.C);

                _netSendTimer += dt;
                if (_mp.Client.LocalId != 0 && _netSendTimer >= NET_SEND_DT)
                {
                    _netSendTimer = 0f;
                    _ = _mp.Client.SendInputAsync(dir, run, attack);
                }

                _mp.Update(gameTime);

                // (opcional) reconciliación suave aquí si la deseas, como en tu versión anterior
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
                _spriteBatch.Begin(transformMatrix: _camera.GetTransform());
                _tileMap.DrawBackground(_spriteBatch, Vector2.Zero, 1366, 768);
                _tileMap.Draw(_spriteBatch, Vector2.Zero);
                _enemyManager.Draw(_spriteBatch);
                _player.Draw(_spriteBatch);

                _mp?.Draw(_spriteBatch); // remotos

                _spriteBatch.End();

                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                _overlay.Draw(_spriteBatch, GraphicsDevice.Viewport, _player.CurrentFx);
                _hud.Draw(_spriteBatch, _player.Stats, new Point(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
                if (_player.Stats.IsDead) _deathScreen.Draw(_spriteBatch, GraphicsDevice.Viewport);
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (_serverProc != null && !_serverProc.HasExited)
                    {
                        _serverProc.Kill(entireProcessTree: true);
                        _serverProc.Dispose();
                    }
                }
                catch { }

                _hud?.Dispose();
                _spriteBatch?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
