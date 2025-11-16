using EscapeSinRetorno.Source.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace EscapeSinRetorno.Source.Core
{
    public class MenuState : IGameState
    {
        private enum MenuScreen
        {
            Main,
            Multiplayer,
            Options,
            Credits,
            IpEntry
        }

        private readonly Game1 _game;
        private readonly List<Button> _buttons = new();

        private SpriteFont _font;
        private SpriteFont _titleFont;
        private Texture2D _pixelTexture;

        private float _pulseTimer = 0f;
        private float _backgroundOpacity = 0f;
        private float _fadeInTimer = 0f;

        // IP input
        private string _ipInput = "127.0.0.1";
        private float _caretTimer = 0f;

        private MenuScreen _currentScreen = MenuScreen.Main;

        // NUEVO SISTEMA PARA EVITAR EL ERROR
        private MenuScreen _pendingScreenChange = MenuScreen.Main;
        private bool _hasPendingScreenChange = false;

        private readonly Color _darkPurple = new Color(25, 15, 35);
        private readonly Color _deepBlack = new Color(10, 5, 15);
        private readonly Color _glowPurple = new Color(120, 80, 150);
        private readonly Color _bloodRed = new Color(120, 20, 30);
        private readonly Color _ghostWhite = new Color(220, 220, 230);

        public MenuState(Game1 game)
        {
            _game = game;
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            _font = content.Load<SpriteFont>("Fonts/MenuFont");
            _titleFont = _font;

            RebuildButtons();
        }

        private void QueueScreen(MenuScreen screen)
        {
            _pendingScreenChange = screen;
            _hasPendingScreenChange = true;
        }

        private void ApplyPendingScreen()
        {
            if (!_hasPendingScreenChange) return;

            _currentScreen = _pendingScreenChange;
            _hasPendingScreenChange = false;
            RebuildButtons();
        }

        private void RebuildButtons()
        {
            _buttons.Clear();

            switch (_currentScreen)
            {
                case MenuScreen.Main:
                    BuildMainMenu(); break;
                case MenuScreen.Multiplayer:
                    BuildMultiplayerMenu(); break;
                case MenuScreen.Options:
                    BuildOptionsMenu(); break;
                case MenuScreen.Credits:
                    BuildCreditsMenu(); break;
                case MenuScreen.IpEntry:
                    BuildIpEntryMenu(); break;
            }
        }

        // --------------------------------------------------------------------
        // MAIN MENU
        // --------------------------------------------------------------------
        private void BuildMainMenu()
        {
            int buttonWidth = 340;
            int buttonHeight = 55;
            int spacing = 70;
            int startY = 260;
            int x = 640 - buttonWidth / 2;

            _buttons.Add(CreateButton(
                new Rectangle(x, startY, buttonWidth, buttonHeight),
                "NUEVA PARTIDA (OFFLINE)",
                () => _game.StartOfflineGame()));

            _buttons.Add(CreateButton(
                new Rectangle(x, startY + spacing, buttonWidth, buttonHeight),
                "MULTIJUGADOR",
                () => QueueScreen(MenuScreen.Multiplayer)));

            _buttons.Add(CreateButton(
                new Rectangle(x, startY + spacing * 2, buttonWidth, buttonHeight),
                "OPCIONES",
                () => QueueScreen(MenuScreen.Options)));

            _buttons.Add(CreateButton(
                new Rectangle(x, startY + spacing * 3, buttonWidth, buttonHeight),
                "CRÉDITOS",
                () => QueueScreen(MenuScreen.Credits)));

            _buttons.Add(CreateButton(
                new Rectangle(x, startY + spacing * 4, buttonWidth, buttonHeight),
                "SALIR",
                () => _game.Exit()));
        }

        private void BuildMultiplayerMenu()
        {
            int buttonWidth = 380;
            int buttonHeight = 55;
            int spacing = 70;
            int startY = 260;
            int x = 640 - buttonWidth / 2;

            _buttons.Add(CreateButton(
                new Rectangle(x, startY, buttonWidth, buttonHeight),
                "CLIENTE LOCALHOST (TEST)",
                () => _game.StartLocalhostClient()));

            _buttons.Add(CreateButton(
                new Rectangle(x, startY + spacing, buttonWidth, buttonHeight),
                "UNIRSE POR IP",
                () => QueueScreen(MenuScreen.IpEntry)));

            _buttons.Add(CreateButton(
                new Rectangle(x, startY + spacing * 3, buttonWidth, buttonHeight),
                "VOLVER",
                () => QueueScreen(MenuScreen.Main)));
        }

        private void BuildOptionsMenu()
        {
            int buttonWidth = 320;
            int buttonHeight = 55;
            int spacing = 70;
            int startY = 260;
            int x = 640 - buttonWidth / 2;

            _buttons.Add(CreateButton(
                new Rectangle(x, startY, buttonWidth, buttonHeight),
                "PANTALLA COMPLETA ON/OFF",
                () => _game.ToggleFullscreen()));

            _buttons.Add(CreateButton(
                new Rectangle(x, startY + spacing, buttonWidth, buttonHeight),
                "VOLUMEN (PRÓXIMAMENTE)",
                () => { }));

            _buttons.Add(CreateButton(
                new Rectangle(x, startY + spacing * 3, buttonWidth, buttonHeight),
                "VOLVER",
                () => QueueScreen(MenuScreen.Main)));
        }

        private void BuildCreditsMenu()
        {
            int buttonWidth = 220;
            int buttonHeight = 50;
            int x = 640 - buttonWidth / 2;
            int y = 520;

            _buttons.Add(CreateButton(
                new Rectangle(x, y, buttonWidth, buttonHeight),
                "VOLVER",
                () => QueueScreen(MenuScreen.Main)));
        }

        private void BuildIpEntryMenu()
        {
            int buttonWidth = 260;
            int buttonHeight = 50;
            int spacing = 60;
            int startY = 360;
            int x = 640 - buttonWidth / 2;

            _buttons.Add(CreateButton(
                new Rectangle(x, startY, buttonWidth, buttonHeight),
                "CONECTAR",
                () =>
                {
                    if (!string.IsNullOrWhiteSpace(_ipInput))
                        _game.JoinByIp(_ipInput.Trim());
                }));

            _buttons.Add(CreateButton(
                new Rectangle(x, startY + spacing, buttonWidth, buttonHeight),
                "VOLVER",
                () => QueueScreen(MenuScreen.Multiplayer)));
        }

        private MenuButton CreateButton(Rectangle bounds, string text, Action onClick)
        {
            var btn = new MenuButton(bounds, text, _font, _darkPurple, _ghostWhite, _glowPurple);
            btn.SetClickAction(onClick);
            return btn;
        }

        // --------------------------------------------------------------------
        // UPDATE
        // --------------------------------------------------------------------
        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _pulseTimer += dt * 2f;
            _fadeInTimer += dt;
            _backgroundOpacity = MathHelper.Min(_fadeInTimer * 0.8f, 1f);
            _caretTimer += dt;

            // ESC navigation
            if (InputManager.IsKeyPressed(Keys.Escape))
            {
                if (_currentScreen == MenuScreen.Main)
                    _game.Exit();
                else if (_currentScreen == MenuScreen.IpEntry)
                    QueueScreen(MenuScreen.Multiplayer);
                else
                    QueueScreen(MenuScreen.Main);
            }

            if (_currentScreen == MenuScreen.IpEntry)
                UpdateIpInput();

            foreach (var button in _buttons)
                button.Update();

            ApplyPendingScreen();
        }

        private void UpdateIpInput()
        {
            if (InputManager.IsKeyPressed(Keys.Back) && _ipInput.Length > 0)
                _ipInput = _ipInput[..^1];

            if (InputManager.IsKeyPressed(Keys.Enter))
            {
                if (!string.IsNullOrWhiteSpace(_ipInput))
                    _game.JoinByIp(_ipInput.Trim());
            }

            TryAppendDigit(Keys.D0, '0'); TryAppendDigit(Keys.D1, '1');
            TryAppendDigit(Keys.D2, '2'); TryAppendDigit(Keys.D3, '3');
            TryAppendDigit(Keys.D4, '4'); TryAppendDigit(Keys.D5, '5');
            TryAppendDigit(Keys.D6, '6'); TryAppendDigit(Keys.D7, '7');
            TryAppendDigit(Keys.D8, '8'); TryAppendDigit(Keys.D9, '9');

            TryAppendDigit(Keys.NumPad0, '0'); TryAppendDigit(Keys.NumPad1, '1');
            TryAppendDigit(Keys.NumPad2, '2'); TryAppendDigit(Keys.NumPad3, '3');
            TryAppendDigit(Keys.NumPad4, '4'); TryAppendDigit(Keys.NumPad5, '5');
            TryAppendDigit(Keys.NumPad6, '6'); TryAppendDigit(Keys.NumPad7, '7');
            TryAppendDigit(Keys.NumPad8, '8'); TryAppendDigit(Keys.NumPad9, '9');

            if (InputManager.IsKeyPressed(Keys.OemPeriod) || InputManager.IsKeyPressed(Keys.Decimal))
                AppendChar('.');

            if (InputManager.IsKeyPressed(Keys.OemMinus) || InputManager.IsKeyPressed(Keys.Subtract))
                AppendChar('-');
        }

        private void TryAppendDigit(Keys key, char c)
        {
            if (InputManager.IsKeyPressed(key))
                AppendChar(c);
        }

        private void AppendChar(char c)
        {
            if (_ipInput.Length < 32)
                _ipInput += c;
        }

        // --------------------------------------------------------------------
        // DRAW
        // --------------------------------------------------------------------
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            DrawMenuBackground(spriteBatch);
            DrawTitle(spriteBatch);
            DrawScreenSpecific(spriteBatch);

            foreach (var button in _buttons)
                button.Draw(spriteBatch, _pixelTexture);

            spriteBatch.End();
        }

        private void DrawMenuBackground(SpriteBatch spriteBatch)
        {
            int screenHeight = 720;
            int steps = 50;
            int stepH = screenHeight / steps;

            for (int i = 0; i < steps; i++)
            {
                float lerp = (float)i / steps;
                Color color = Color.Lerp(_deepBlack, _darkPurple, lerp * 0.6f) * _backgroundOpacity;
                Rectangle rect = new Rectangle(0, i * stepH, 1280, stepH + 1);
                spriteBatch.Draw(_pixelTexture, rect, color);
            }
        }

        private void DrawTitle(SpriteBatch spriteBatch)
        {
            string title = _currentScreen switch
            {
                MenuScreen.Multiplayer => "MULTIJUGADOR",
                MenuScreen.Options => "OPCIONES",
                MenuScreen.Credits => "CRÉDITOS",
                MenuScreen.IpEntry => "UNIRSE POR IP",
                _ => "ESCAPE SIN RETORNO"
            };

            Vector2 size = _titleFont.MeasureString(title);
            Vector2 pos = new Vector2(640 - size.X / 2, 80);

            float pulse = 1f + (float)Math.Sin(_pulseTimer) * 0.1f;
            Color titleColor = Color.Lerp(
                _ghostWhite,
                _glowPurple,
                (float)Math.Sin(_pulseTimer * 0.5f) * 0.3f + 0.7f
            ) * _backgroundOpacity;

            spriteBatch.DrawString(_titleFont, title, pos + new Vector2(3, 3),
                _deepBlack * _backgroundOpacity, 0f, Vector2.Zero, pulse, SpriteEffects.None, 0f);

            spriteBatch.DrawString(_titleFont, title, pos,
                titleColor, 0f, Vector2.Zero, pulse, SpriteEffects.None, 0f);
        }

        private void DrawScreenSpecific(SpriteBatch spriteBatch)
        {
            switch (_currentScreen)
            {
                case MenuScreen.Credits: DrawCredits(spriteBatch); break;
                case MenuScreen.IpEntry: DrawIpEntry(spriteBatch); break;
            }
        }

        private void DrawCredits(SpriteBatch spriteBatch)
        {
            string[] lines =
            {
                "Escape Sin Retorno",
                "",
                "Programación y diseño:",
                "  Chejo",
                "",
                "Asistente técnico:",
                "  Code Copilot (ChatGPT)",
                "",
                "Gracias por jugar."
            };

            float y = 200;

            foreach (var line in lines)
            {
                Vector2 size = _font.MeasureString(line);
                Vector2 pos = new Vector2(640 - size.X / 2, y);
                spriteBatch.DrawString(_font, line, pos, _ghostWhite * 0.9f);
                y += size.Y + 4;
            }
        }

        private void DrawIpEntry(SpriteBatch spriteBatch)
        {
            string label = "Dirección IP del host:";
            Vector2 labelSize = _font.MeasureString(label);
            Vector2 labelPos = new Vector2(640 - labelSize.X / 2, 250);
            spriteBatch.DrawString(_font, label, labelPos, _ghostWhite);

            string text = _ipInput;
            bool showCaret = ((int)(_caretTimer * 2f) % 2) == 0;
            if (showCaret) text += "_";

            Vector2 ipSize = _font.MeasureString(text);
            Vector2 ipPos = new Vector2(640 - ipSize.X / 2, 280);

            Rectangle box = new Rectangle(
                (int)(ipPos.X - 12),
                (int)(ipPos.Y - 6),
                (int)(ipSize.X + 24),
                (int)(ipSize.Y + 12));

            spriteBatch.Draw(_pixelTexture, box, new Color(20, 10, 30) * 0.9f);
            spriteBatch.DrawString(_font, text, ipPos, _ghostWhite);
        }

        public void HandleInput() { }
    }
}
