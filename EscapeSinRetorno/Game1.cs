// File: Game1.cs
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

using EscapeSinRetorno.Source.World;
using EscapeSinRetorno.Source.Entities;
using EscapeSinRetorno.Source.Entities.Enemies;
using EscapeSinRetorno.Source.Core;
using EscapeSinRetorno.Source.UI;
using EscapeSinRetorno.Source.Multiplayer;
using EscapeSinRetorno.Source.Boot;
using EscapeSinRetorno.Source.Net;
using EscapeSinRetorno.Source.Chat;
using EscapeSinRetorno.Source.Inventory;
using EscapeSinRetorno.Source.Items;
using EscapeSinRetorno.Source.Systems.Stats;

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

        public bool FxEnabled { get; set; } = true;
        public bool DebugShowCollisions
        {
            get => _debugShowCollisions;
            set => _debugShowCollisions = value;
        }
        public double WorldTimeSeconds => _worldTimeSeconds;
        public DoorManager DoorManager => _doorManager;

        private StatsHud _hud;
        private VignetteOverlay _overlay;
        private DeathScreen _deathScreen;
        private SpriteFont _hudFont;

        private MultiplayerManager _mp = new MultiplayerManager();
        private GameNetMode _netMode = GameNetMode.Offline;

        private ChatManager _chat;
        private ChatRenderer _chatRenderer;
        private Texture2D _chatPixel;
        private int _prevScrollValue;

        private PlayerInventory _inventory;
        private InventoryRenderer _inventoryRenderer;

        private double _worldTimeSeconds = 0;
        private bool _debugShowCollisions = false;

        // ✔ nuevo sistema
        private ItemPickupManager _pickupManager;
        private KeyboardState _prevKeyboard;  // para pickups
        private KeyboardState _prevKb;        // para inventario/chat

        private DoorManager _doorManager = new DoorManager();

        public static readonly Random Random = new Random();

        // ========== AUDIO ==========
        Song menuMusic;
        Song gameMusic;
        SoundEffect doorOpenSound;
        SoundEffect playerHitSound;
        SoundEffect playerDeathSound;
        SoundEffect playerAttackSound;
        // ===========================

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

            // ==== Carga de audio ====
            menuMusic = Content.Load<Song>("Audio/menu");
            gameMusic = Content.Load<Song>("Audio/exploration");
            doorOpenSound = Content.Load<SoundEffect>("Audio/door-open");
            playerHitSound = Content.Load<SoundEffect>("Audio/damage-player");
            playerDeathSound = Content.Load<SoundEffect>("Audio/death");
            playerAttackSound = Content.Load<SoundEffect>("Audio/attack");
            // ========================

            _menuState = new MenuState(this);
            _menuState.LoadContent(Content, GraphicsDevice);

            _overlay = new VignetteOverlay(GraphicsDevice);
            _hudFont = Content.Load<SpriteFont>("Fonts/MenuFont");
            _hud = new StatsHud(GraphicsDevice, _hudFont);
            _deathScreen = new DeathScreen(GraphicsDevice, _hudFont, onRetry: StartOfflineGame, onMenu: ReturnToMenu);

            ItemDatabase.Load(Content);
            _inventoryRenderer = new InventoryRenderer(GraphicsDevice, _hudFont);

            _doorManager.LoadContent(Content, GraphicsDevice);

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

            ChatCommandBootstrap.RegisterAll();
            _chat.CommandRequested += OnChatCommandRequested;
            _chat.MessageSent += OnChatMessageSent;

            // ✔ nuevo manager de pickups
            _pickupManager = new ItemPickupManager();
            _prevKeyboard = Keyboard.GetState();
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
        // AUDIO
        // =========================
        private void PlayMusic(Song song)
        {
            if (song == null)
                return;

            if (MediaPlayer.Queue.ActiveSong != song)
            {
                MediaPlayer.Stop();
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 0.4f; // ajusta a gusto
                MediaPlayer.Play(song);
            }
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

            // Pasar SFX al player
            _player.SfxHit = playerHitSound;
            _player.SfxDeath = playerDeathSound;
            _player.SfxAttack = playerAttackSound;

            _inventory = new PlayerInventory();
            _player.Inventory = _inventory;

            // SEEDS
            _inventory.AddItem("potion_life", 5);
            _inventory.AddItem("watter_bottle", 3);
            _inventory.AddItem("meal", 4);
            _inventory.AddItem("main_key", 1);
            _inventory.AddItem("cyan_key", 1);

            if (_tileMap.PlayerStartPosition.HasValue)
                _player.SetPosition(_tileMap.PlayerStartPosition.Value);

            _enemyManager = new EnemyManager();
            _enemyManager.SpawnFromMapData(_tileMap.EnemySpawns);
            _enemyManager.LoadContent(Content);
            Enemy.LoadDebugTexture(GraphicsDevice);

            // conectar enemyManager al player
            _player.SetEnemyManager(_enemyManager);

            _doorManager.ClearDoors();
            _doorManager.SpawnFromMapData(_tileMap.DoorSpawns, _tileMap.TileSize);

            _camera = new Camera2D(GraphicsDevice.Viewport);
            _camera.SetZoom(5.0f);

            _pickupManager = new ItemPickupManager(); // reset pickups
            _prevKeyboard = Keyboard.GetState();

            _isInMenu = false;
            _wasDead = false;
            IsMouseVisible = false;

            // música del juego
            PlayMusic(gameMusic);
        }


        public void ReturnToMenu()
        {
            _mp.Stop();
            _isInMenu = true;
            IsMouseVisible = true;

            // música del menú
            PlayMusic(menuMusic);
        }


        // =========================
        // UPDATE
        // =========================
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            InputManager.Update();

            var ms = Mouse.GetState();
            int scrollDelta = ms.ScrollWheelValue - _prevScrollValue;
            _prevScrollValue = ms.ScrollWheelValue;

            var kb = Keyboard.GetState();

            // MENÚ
            if (_isInMenu)
            {
                // asegura que suene la música del menú
                PlayMusic(menuMusic);

                _menuState.Update(gameTime);
                _menuState.HandleInput();
                base.Update(gameTime);
                _prevKb = kb;
                return;
            }

            _worldTimeSeconds += gameTime.ElapsedGameTime.TotalSeconds;

            // INVENTARIO
            if (_chat != null && !_chat.IsOpen && _inventoryRenderer != null &&
                InputManager.IsKeyPressed(Keys.E))
            {
                _inventoryRenderer.Toggle();
            }

            if (_inventoryRenderer != null && _inventoryRenderer.IsOpen)
            {
                _inventoryRenderer.HandleInput(
                    _prevKb,
                    kb,
                    _inventory,
                    canUse: CanUseItemInInventory,
                    onUse: ApplyItemEffect,
                    onDrop: OnDropFromInventory
                );

                base.Update(gameTime);
                _prevKb = kb;
                return;
            }

            // CHAT
            if (_chat != null && (_chat.IsOpen || _chat.Messages.Count > 0) && scrollDelta != 0)
            {
                int deltaLines = scrollDelta > 0 ? 1 : -1;
                _chatRenderer?.AdjustScroll(deltaLines);
            }

            if (_chat != null)
            {
                if (!_chat.IsOpen && InputManager.IsKeyPressed(Keys.T))
                {
                    _chat.Open();
                    _chatRenderer?.ResetScroll();
                }

                _chat.Update(gameTime);

                if (_chat.IsOpen)
                {
                    base.Update(gameTime);
                    _prevKb = kb;
                    return;
                }
            }

            // MUERTE
            bool isDead = _player?.Stats?.IsDead == true;
            if (isDead && !_wasDead) IsMouseVisible = true;
            if (!isDead && _wasDead) IsMouseVisible = false;
            _wasDead = isDead;

            if (isDead)
            {
                _deathScreen.Update(gameTime);
                base.Update(gameTime);
                _prevKb = kb;
                return;
            }

            // MULTIJUGADOR CLIENTE INPUT
            if (_netMode == GameNetMode.Client && _mp.Enabled && _mp.Client != null)
            {
                var ks = kb;
                Vector2 dir = Vector2.Zero;
                if (ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D)) dir.X++;
                if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A)) dir.X--;
                if (ks.IsKeyDown(Keys.Up) || ks.IsKeyDown(Keys.W)) dir.Y--;
                if (ks.IsKeyDown(Keys.Down) || ks.IsKeyDown(Keys.S)) dir.Y++;

                bool run = ks.IsKeyDown(Keys.X);
                bool attack = ks.IsKeyDown(Keys.C);

                _ = _mp.Client.SendInputAsync(dir, run, attack);
                _mp.Update(gameTime);
            }

            // GAMEPLAY LOCAL
            _enemyManager.Update(gameTime, _player, _tileMap);
            _player.Update(gameTime, _tileMap, _doorManager);

            _doorManager.Update(gameTime);

            if (InputManager.IsKeyPressed(Keys.B))
            {
                var hb = _player.GetHitbox();

                bool openedDoor = _doorManager.TryOpenNearbyDoor(hb, _inventory, _chat);

                if (openedDoor)
                {
                    // sonido al abrir puerta
                    doorOpenSound?.Play();
                }
                else
                {
                    _enemyManager.TryInteractWithMageGuardian(hb, _inventory, _chat);
                }
            }

            // ✔ recoger ítems con R
            var curKeyboard = kb;
            _pickupManager.Update(_player, _prevKeyboard, curKeyboard);
            _prevKeyboard = curKeyboard;

            _camera.Follow(_player.Position,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight);

            base.Update(gameTime);
            _prevKb = kb;
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

                _doorManager.Draw(_spriteBatch);

                // pickups, enemigos, jugador...
                _pickupManager.Draw(_spriteBatch);
                _enemyManager.Draw(_spriteBatch);
                _player.Draw(_spriteBatch);
                if (_netMode == GameNetMode.Client && _mp.Enabled)
                    _mp.Draw(_spriteBatch);

                // DEBUG: mostrar colisiones como overlay
                if (_debugShowCollisions && _tileMap != null && _chatPixel != null)
                {
                    int ts = _tileMap.TileSize;
                    for (int y = 0; y < _tileMap.Height; y++)
                    {
                        for (int x = 0; x < _tileMap.Width; x++)
                        {
                            var pos = new Vector2(x * ts, y * ts);
                            if (_tileMap.IsColliding(pos, ts, ts))
                            {
                                var r = new Rectangle((int)pos.X, (int)pos.Y, ts, ts);
                                _spriteBatch.Draw(_chatPixel, r, Color.Red * 0.25f);
                            }
                        }
                    }
                }
                _spriteBatch.End();

                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                _overlay.Draw(_spriteBatch, vp, _player.CurrentFx);
                _hud.Draw(_spriteBatch, _player.Stats, new Point(vp.Width, vp.Height));
                if (_player.Stats.IsDead)
                    _deathScreen.Draw(_spriteBatch, vp);

                _chatRenderer.Draw(_spriteBatch, _chat);
                _inventoryRenderer.Draw(_spriteBatch, _inventory, vp);
                _spriteBatch.End();
            }

            if (Player.DebugDrawFPS)
            {
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

                _spriteBatch.DrawString(
                    _hudFont,
                    $"FPS: {(1 / gameTime.ElapsedGameTime.TotalSeconds):0}",
                    new Vector2(200, 10),
                    Color.Yellow
                );

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
            if (_netMode == GameNetMode.Client && _mp.Enabled && _mp.Client != null)
            {
                _ = _mp.Client.SendChatAsync(text);
            }
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


        // =========================
        // INVENTORY HOOKS
        // =========================
        private bool CanUseItemInInventory(EscapeSinRetorno.Source.Inventory.Item item)
        {
            if (item == null || _player?.Stats == null) return false;

            var stats = _player.Stats;
            switch (item.Type)
            {
                case ItemType.Potion:
                    return !stats.Health.IsFull;
                case ItemType.Water:
                    return !stats.Thirst.IsFull;
                case ItemType.Food:
                    return !stats.Hunger.IsFull;
                default:
                    return false;
            }
        }

        private void ApplyItemEffect(EscapeSinRetorno.Source.Inventory.Item item)
        {
            if (item == null || _player?.Stats == null) return;

            var s = _player.Stats;

            switch (item.Type)
            {
                case ItemType.Potion:
                    s.Heal(25f);
                    _chat?.AddSystemMessage("Usaste una poción de vida (+25 HP).");
                    break;
                case ItemType.Water:
                    s.Drink(30f);
                    s.Stamina.Add(20f);
                    _chat?.AddSystemMessage("Bebiste agua (+30 sed, +20 estamina).");
                    break;
                case ItemType.Food:
                    s.ConsumeFood(25f, 2f);
                    s.Heal(10f);
                    _chat?.AddSystemMessage("Comiste comida (+25 hambre, +2 cordura, +10 HP).");
                    break;
                default:
                    _chat?.AddSystemMessage($"No puedes usar: {item.Name}.");
                    break;
            }
        }

        private void OnDropFromInventory(EscapeSinRetorno.Source.Inventory.Item item, int amount)
        {
            if (item == null || amount <= 0 || _player == null) return;

            if (_netMode == GameNetMode.Client)
            {
                _chat?.AddSystemMessage("Soltar objetos en cliente aún no está implementado.");
                return;
            }

            SpawnItemOnGround(item.Id, amount, _player.Center);
            _chat?.AddSystemMessage($"Soltaste {amount}x {item.Name}.");

            var center = _player.Center;
            const int scatterRadius = 10;

            for (int i = 0; i < amount; i++)
            {
                var offset = new Vector2(
                    Random.Next(-scatterRadius, scatterRadius + 1),
                    Random.Next(-scatterRadius, scatterRadius + 1)
                );

                var pos = center + offset;

                var pickup = new ItemPickup(item.Id, pos);
                pickup.LoadContent(Content);

                // ✔ Agregar al nuevo sistema
                _pickupManager.AddPickup(pickup);
            }

            _chat?.AddSystemMessage($"Soltaste {amount}x {item.Name}.");
        }

        public void SpawnItemOnGround(string itemId, int amount, Vector2 center)
        {
            if (amount <= 0 || _pickupManager == null) return;

            const int scatterRadius = 10;

            for (int i = 0; i < amount; i++)
            {
                var offset = new Vector2(
                    Random.Next(-scatterRadius, scatterRadius + 1),
                    Random.Next(-scatterRadius, scatterRadius + 1)
                );

                var pos = center + offset;

                var pickup = new ItemPickup(itemId, pos);
                pickup.LoadContent(Content);
                _pickupManager.AddPickup(pickup);
            }
        }

        // Para /wipeinv
        public void ClearAllPickups()
        {
            _pickupManager?.ClearAll();
        }
    }
}
