// File: Game1.cs
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
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

        private KeyboardState _prevKb;

        private readonly List<ItemPickup> _itemPickups = new();

        private DoorManager _doorManager = new DoorManager();

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

            ItemDatabase.Load(Content);
            _inventoryRenderer = new InventoryRenderer(GraphicsDevice, _hudFont);

            _doorManager.LoadContent(Content);

            if (_opts.AutoStart)
            {
                if (_opts.AsClient) JoinByIpPort(_opts.Host, _opts.Port);
                else StartOfflineGame();
            }

            Window.TextInput += OnTextInput;

            _chat = new ChatManager();
            _chatPixel = new Texture2D(GraphicsDevice, 1, 1);
            _chatPixel.SetData(new[] { Color.White });
            _chatRenderer = new ChatRenderer(_hudFont, _chatPixel);
            _chat.CommandRequested += OnChatCommandRequested;
            _chat.MessageSent += OnChatMessageSent;

            _prevScrollValue = Mouse.GetState().ScrollWheelValue;
            _prevKb = Keyboard.GetState();
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

            _inventory = new PlayerInventory();
            _player.Inventory = _inventory;

            _itemPickups.Clear();

            // Seeds de prueba
            _inventory.AddItem("potion_life", 5);
            _inventory.AddItem("watter_bottle", 3);
            _inventory.AddItem("meal", 4);
            _inventory.AddItem("main_key", 1);

            if (_tileMap.PlayerStartPosition.HasValue)
                _player.SetPosition(_tileMap.PlayerStartPosition.Value);

            _enemyManager = new EnemyManager();
            _enemyManager.SpawnFromMapData(_tileMap.EnemySpawns);
            _enemyManager.LoadContent(Content);
            Enemy.LoadDebugTexture(GraphicsDevice);

            _doorManager.ClearDoors();
            _doorManager.SpawnFromMapData(_tileMap.DoorSpawns, _tileMap.TileSize);

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

            var ms = Mouse.GetState();
            int scrollDelta = ms.ScrollWheelValue - _prevScrollValue;
            _prevScrollValue = ms.ScrollWheelValue;

            var kb = Keyboard.GetState();

            if (_isInMenu)
            {
                _menuState.Update(gameTime);
                _menuState.HandleInput();
                base.Update(gameTime);
                _prevKb = kb;
                return;
            }

            // INVENTARIO (E)
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

            // MULTIJUGADOR CLIENTE
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
                _doorManager.TryOpenNearbyDoor(_player.GetHitbox(), _inventory, _chat);
            }

            UpdatePickups();

            _camera.Follow(_player.Position,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight);

            base.Update(gameTime);
            _prevKb = kb;
        }

        private void UpdatePickups()
        {
            if (_player == null || _inventory == null) return;

            var hb = _player.GetHitbox();

            for (int i = _itemPickups.Count - 1; i >= 0; i--)
            {
                var p = _itemPickups[i];
                if (hb.Intersects(p.Bounds))
                {
                    if (_inventory.AddItem(p.ItemId, 1))
                    {
                        if (ItemDatabase.Items.TryGetValue(p.ItemId, out var def))
                            _chat?.AddSystemMessage($"Recogiste {def.Name}.");

                        _itemPickups.RemoveAt(i);
                    }
                    else
                    {
                        _chat?.AddSystemMessage("Inventario lleno.");
                    }
                }
            }
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

                foreach (var p in _itemPickups)
                    p.Draw(_spriteBatch);

                _enemyManager.Draw(_spriteBatch);
                _player.Draw(_spriteBatch);
                if (_netMode == GameNetMode.Client && _mp.Enabled)
                    _mp.Draw(_spriteBatch);
                _spriteBatch.End();

                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                _overlay.Draw(_spriteBatch, vp, _player.CurrentFx);
                _hud.Draw(_spriteBatch, _player.Stats, new Point(vp.Width, vp.Height));
                if (_player.Stats.IsDead)
                    _deathScreen.Draw(_spriteBatch, vp);

                _chatRenderer.Draw(_spriteBatch, _chat);
                _inventoryRenderer.Draw(_spriteBatch, _inventory, GraphicsDevice.Viewport);
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
                _itemPickups.Add(pickup);
            }

            _chat?.AddSystemMessage($"Soltaste {amount}x {item.Name}.");
        }
    }
}
