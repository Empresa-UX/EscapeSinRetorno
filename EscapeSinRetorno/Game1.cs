using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSinRetorno.Source.World;
using EscapeSinRetorno.Source.Entities;
using EscapeSinRetorno.Source.Entities.Enemies;
using EscapeSinRetorno.Source.Core;
using EscapeSinRetorno.Source.UI;
using EscapeSinRetorno.Source.Multiplayer;
using EscapeSinRetorno.Source.Boot;
using EscapeSinRetorno.Source.Net;
using EscapeSinRetorno.Source.Chat;

namespace EscapeSinRetorno
{
    public class Game1 : Game
    {
        private readonly LaunchOptions _opts;

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

        private MultiplayerManager _mp = new MultiplayerManager();
        private GameNetMode _netMode = GameNetMode.Offline;

        // CHAT
        private ChatManager _chat;
        private ChatRenderer _chatRenderer;
        private Texture2D _chatPixel;
        private int _prevScrollValue;

        public static readonly Random Random = new Random();

        public Game1(LaunchOptions opts = null)
        {
            _opts = opts ?? LaunchOptions.Default();

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
            _deathScreen = new DeathScreen(GraphicsDevice, _hudFont, onRetry: StartOfflineGame, onMenu: ReturnToMenu);

            // Autostart opcional via args
            if (_opts.AutoStart)
            {
                if (_opts.AsClient) JoinByIpPort(_opts.Host, _opts.Port);
                else StartOfflineGame();
            }

            Window.TextInput += OnTextInput;

            // CHAT
            _chat = new ChatManager();

            _chatPixel = new Texture2D(GraphicsDevice, 1, 1);
            _chatPixel.SetData(new[] { Color.White });
           
            _chatRenderer = new ChatRenderer(_hudFont, _chatPixel);
            _chat.CommandRequested += OnChatCommandRequested;
            _chat.MessageSent += OnChatMessageSent;

            _prevScrollValue = Mouse.GetState().ScrollWheelValue;
        }

        private void OnClientSizeChanged(object sender, EventArgs e)
        {
            _deathScreen?.Resize(GraphicsDevice.Viewport, StartOfflineGame, ReturnToMenu);
        }

        private void OnGameExiting(object sender, EventArgs e)
        {
            try { _mp?.Stop(); } catch { }
            _hud?.Dispose();
            _spriteBatch?.Dispose();
        }

        // =========================
        // MENÚ
        // =========================
        public void StartOfflineGame()
        {
            _netMode = GameNetMode.Offline;
            _mp.Stop();
            BuildWorld();
        }

        public void StartLocalhostClient()
        {
            _netMode = GameNetMode.Client;
            _mp.StartClient(Content, host: "127.0.0.1", name: "Player", port: NetConfig.ServerPort);

            // 👇 NUEVO: escuchar mensajes de chat
            if (_mp.Client != null)
                _mp.Client.ChatReceived += OnNetChatReceived;

            BuildWorld();
        }

        public void JoinByIp(string host)
        {
            _netMode = GameNetMode.Client;
            _mp.StartClient(Content, host, name: "Player", port: NetConfig.ServerPort);

            if (_mp.Client != null)
                _mp.Client.ChatReceived += OnNetChatReceived;

            BuildWorld();
        }

        public void JoinByIpPort(string host, int port)
        {
            _netMode = GameNetMode.Client;
            _mp.StartClient(Content, host, name: "Player", port: port);

            if (_mp.Client != null)
                _mp.Client.ChatReceived += OnNetChatReceived;

            BuildWorld();
        }

        private void BuildWorld()
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
            _mp.Stop();
            _isInMenu = true;
            IsMouseVisible = true;
        }

        // =========================
        // UPDATE
        // =========================
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            InputManager.Update();

            // 👇 NUEVO: scroll con la rueda del mouse para el chat
            var ms = Mouse.GetState();
            int scrollDelta = ms.ScrollWheelValue - _prevScrollValue;
            _prevScrollValue = ms.ScrollWheelValue;

            if (_chat != null && (_chat.IsOpen || _chat.Messages.Count > 0) && scrollDelta != 0)
            {
                // En XNA: rueda arriba => delta positivo. Queremos que arriba = ver mensajes más viejos.
                int deltaLines = scrollDelta > 0 ? 1 : -1;
                _chatRenderer?.AdjustScroll(deltaLines);
            }

            // CHAT
            // CHAT: solo en juego, no en menú principal
            if (!_isInMenu)
            {
                // Abrir chat solo si está cerrado
                if (!_chat.IsOpen && InputManager.IsKeyPressed(Keys.T))
                {
                    _chat.Open();
                    _chatRenderer.ResetScroll(); // 👈 ir al final
                }

                _chat.Update(gameTime);

                // Si el chat está abierto, bloqueamos el resto del input de juego
                if (_chat.IsOpen)
                {
                    base.Update(gameTime);
                    return;
                }
            }



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

            // Multiplayer client input forwarding
            if (_netMode == GameNetMode.Client && _mp.Enabled && _mp.Client != null)
            {
                var ks = Keyboard.GetState();
                Vector2 dir = Vector2.Zero;
                if (ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D)) dir.X++;
                if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A)) dir.X--;
                if (ks.IsKeyDown(Keys.Up) || ks.IsKeyDown(Keys.W)) dir.Y--;
                if (ks.IsKeyDown(Keys.Down) || ks.IsKeyDown(Keys.S)) dir.Y--;

                bool run = ks.IsKeyDown(Keys.X);
                bool attack = ks.IsKeyDown(Keys.C);

                _ = _mp.Client.SendInputAsync(dir, run, attack);
                _mp.Update(gameTime);
            }

            _enemyManager.Update(gameTime, _player, _tileMap);
            _player.Update(gameTime, _tileMap);
            _camera.Follow(_player.Position, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            base.Update(gameTime);
        }

        // =========================
        // DRAW
        // =========================
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (_isInMenu)
            {
                _menuState.Draw(_spriteBatch);
            }
            else
            {
                var camPos = _camera.GetPosition();
                var vp = GraphicsDevice.Viewport;

                _spriteBatch.Begin(transformMatrix: _camera.GetTransform());
                _tileMap.DrawBackground(_spriteBatch, camPos, vp.Width, vp.Height);
                _tileMap.Draw(_spriteBatch, camPos);
                _enemyManager.Draw(_spriteBatch);
                _player.Draw(_spriteBatch);
                if (_netMode == GameNetMode.Client && _mp.Enabled)
                    _mp.Draw(_spriteBatch);
                _spriteBatch.End();

                // HUD + Overlay + Chat
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                _overlay.Draw(_spriteBatch, vp, _player.CurrentFx);
                _hud.Draw(_spriteBatch, _player.Stats, new Point(vp.Width, vp.Height));
                if (_player.Stats.IsDead)
                    _deathScreen.Draw(_spriteBatch, vp);

                _chatRenderer.Draw(_spriteBatch, _chat);
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { _mp?.Stop(); } catch { }
                _hud?.Dispose();
                _spriteBatch?.Dispose();
            }
            base.Dispose(disposing);
        }

        public void ToggleFullscreen()
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _graphics.ApplyChanges();
        }

        // =========================
        // CHAT COMMANDS
        // =========================
        private void OnChatCommandRequested(string cmd, string[] args)
        {
            ChatCommandExecutor.Execute(
                cmd,
                args,
                this,
                _player,
                _tileMap,
                _enemyManager,
                _chat);
        }

        private bool TeleportPlayerRandom()
        {
            if (_player == null || _tileMap == null) return false;

            const int MaxTries = 200;

            for (int i = 0; i < MaxTries; i++)
            {
                int tx = Random.Next(0, _tileMap.Width);
                int ty = Random.Next(0, _tileMap.Height);

                var worldPos = new Vector2(
                    tx * _tileMap.TileSize,
                    ty * _tileMap.TileSize
                );

                if (_player.IsPositionFree(_tileMap, worldPos))
                {
                    _player.SetPosition(worldPos);
                    return true;
                }
            }

            return false;
        }

        private void OnChatMessageSent(string text)
        {
            // Si estoy en cliente, mandar el mensaje al servidor
            if (_netMode == GameNetMode.Client && _mp.Enabled && _mp.Client != null)
            {
                _ = _mp.Client.SendChatAsync(text);
            }
            // En offline no hace nada extra (el mensaje ya se agregó localmente)
        }

        private void OnNetChatReceived(int fromId, string msg)
        {
            if (_chat == null) return;

            string label;
            if (_netMode == GameNetMode.Client && _mp.LocalId == fromId)
                label = "Tú";
            else
                label = $"Player {fromId}";

            _chat.AddPlayerMessage($"[{label}] {msg}");
        }

        private void OnTextInput(object sender, TextInputEventArgs e)
        {
            if (_chat != null && _chat.IsOpen)
                _chat.ReceiveTextInput(e);
        }

    }
}
