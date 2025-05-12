using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShadowSky.World;
using ShadowSky.Source.Player;
using System;

namespace ShadowSky
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SpriteFont _debugFont;
        private Texture2D _fadeTexture;

        private Player _player;
        private TileMap _tileMap;

        private Vector2 _camera;

        private KeyboardState _currentKey;
        private KeyboardState _previousKey;

        private int _mapWidth, _mapHeight;
        private const int TileSize = 16;

        private Effect _blurEffect;
        private RenderTarget2D _sceneTarget;
        private RenderTarget2D _blurTarget;

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

            _debugFont = Content.Load<SpriteFont>("Fonts/DebugFont");
            _fadeTexture = new Texture2D(GraphicsDevice, 1, 1);
            _fadeTexture.SetData(new[] { Color.Black });

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

            _blurEffect = Content.Load<Effect>("Shaders/BlurEffect");

            int w = _graphics.PreferredBackBufferWidth;
            int h = _graphics.PreferredBackBufferHeight;
            _sceneTarget = new RenderTarget2D(GraphicsDevice, w, h);
            _blurTarget = new RenderTarget2D(GraphicsDevice, w, h);
        }

        protected override void Update(GameTime gameTime)
        {
            _previousKey = _currentKey;
            _currentKey = Keyboard.GetState();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_currentKey.IsKeyDown(Keys.F9) && !_previousKey.IsKeyDown(Keys.F9))
                _player.Stats.SetSanity(5);

            _player.Update(_currentKey, _tileMap, _mapWidth, _mapHeight, dt);

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
            // Log interno (Inicio)
            Console.WriteLine("[1] Inicio del método Draw");

            // Dibujar al RenderTarget intermedio
            GraphicsDevice.SetRenderTarget(_sceneTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            Console.WriteLine("[2] _sceneTarget configurado y limpiado");

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _tileMap.Draw(_spriteBatch, _camera, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _player.Draw(_spriteBatch, _camera);

            var messages = _player.Stats.GetStatusMessages();
            for (int i = 0; i < messages.Count; i++)
                _spriteBatch.DrawString(_debugFont, messages[i], new Vector2(10, 10 + i * 20), Color.Red);

            _spriteBatch.DrawString(_debugFont, "[DEBUG] TileMap y Player dibujados", new Vector2(10, 180), Color.Lime);
            _spriteBatch.End();
            Console.WriteLine("[4] SpriteBatch.End (dibujado de mensajes)");

            if (_player.Stats.VisionBlurred)
            {
                Console.WriteLine("[5] VisionBlurred = TRUE. Aplicando efecto...");

                // Mostrar feedback visual
                _spriteBatch.Begin();
                _spriteBatch.DrawString(_debugFont, "[DEBUG] VisionBlurred ACTIVO", new Vector2(10, 300), Color.Yellow);
                _spriteBatch.End();

                _blurEffect.Parameters["texelSize"].SetValue(new Vector2(1f / _sceneTarget.Width, 1f / _sceneTarget.Height));
                _blurEffect.Parameters["direction"].SetValue(new Vector2(1, 0));
                Console.WriteLine("[7] Parámetros del shader (horizontal) configurados");

                GraphicsDevice.SetRenderTarget(_blurTarget);
                GraphicsDevice.Clear(Color.Transparent);
                Console.WriteLine("[8] _blurTarget configurado para blur horizontal");

                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, _blurEffect);
                _spriteBatch.Draw(_sceneTarget, Vector2.Zero, Color.White);
                _spriteBatch.End();
                Console.WriteLine("[9] Blur horizontal aplicado");

                _blurEffect.Parameters["direction"].SetValue(new Vector2(0, 1));
                Console.WriteLine("[10] Parámetros del shader (vertical) configurados");

                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);
                Console.WriteLine("[11] RenderTarget restablecido a pantalla principal");

                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, _blurEffect);
                _spriteBatch.Draw(_blurTarget, Vector2.Zero, Color.White);
                _spriteBatch.End();
                Console.WriteLine("[12] Blur vertical aplicado y dibujado");

                // Feedback final en pantalla
                _spriteBatch.Begin();
                _spriteBatch.DrawString(_debugFont, "[DEBUG] Efecto Blur COMPLETADO", new Vector2(10, 330), Color.Cyan);
                _spriteBatch.End();

                _spriteBatch.Begin();
                _spriteBatch.DrawString(_debugFont, $"texelSize: {1f / _sceneTarget.Width:F4}, {1f / _sceneTarget.Height:F4}", new Vector2(10, 370), Color.Orange);
                _spriteBatch.End();

            }
            else
            {
                Console.WriteLine("[13] VisionBlurred = FALSE. Dibujando sin efectos...");
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);

                _spriteBatch.Begin();
                _spriteBatch.Draw(_sceneTarget, Vector2.Zero, Color.White);
                _spriteBatch.DrawString(_debugFont, "[DEBUG] Sin efecto blur", new Vector2(10, 300), Color.LightGray);
                _spriteBatch.End();
            }

            Console.WriteLine("[14] Llamando a DrawEffects del jugador...");
            _player.DrawEffects(_spriteBatch, _fadeTexture, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            Console.WriteLine("[15] Fin del método Draw");

            base.Draw(gameTime);
        }

    }
}
