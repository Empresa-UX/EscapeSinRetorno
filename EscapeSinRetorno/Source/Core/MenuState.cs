// File: Source/Core/MenuState.cs
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

        private string _ipInput = "127.0.0.1";
        private float _caretTimer = 0f;

        private MenuScreen _currentScreen = MenuScreen.Main;

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
            _titleFont = _font; // si luego quieres, se puede usar una fuente distinta para el título

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
            int buttonWidth = 360;
            int buttonHeight = 58;
            int spacing = 72;
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
            int buttonWidth = 360;
            int buttonHeight = 55;
            int spacing = 72; // más separado para que respire mejor
            int startY = 230;
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
                new Rectangle(x, startY + spacing * 2, buttonWidth, buttonHeight),
                "GRÁFICOS (PRÓXIMAMENTE)",
                () => { }));

            _buttons.Add(CreateButton(
                new Rectangle(x, startY + spacing * 3, buttonWidth, buttonHeight),
                "CONTROLES (PRÓXIMAMENTE)",
                () => { }));

            _buttons.Add(CreateButton(
                new Rectangle(x, startY + spacing * 4, buttonWidth, buttonHeight),
                "GAMEPLAY (PRÓXIMAMENTE)",
                () => { }));

            _buttons.Add(CreateButton(
                new Rectangle(x, startY + spacing * 5 + 10, buttonWidth, buttonHeight),
                "VOLVER",
                () => QueueScreen(MenuScreen.Main)));
        }

        private void BuildCreditsMenu()
        {
            int buttonWidth = 220;
            int buttonHeight = 50;
            int x = 640 - buttonWidth / 2;

            // Más margen respecto al panel de créditos
            int y = 640;

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
            _pulseTimer += dt;
            _fadeInTimer += dt;
            _backgroundOpacity = MathHelper.Min(_fadeInTimer * 0.8f, 1f);
            _caretTimer += dt;

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

            float scale = _currentScreen == MenuScreen.Main ? 2.6f : 2.1f;

            Vector2 size = _titleFont.MeasureString(title);
            Vector2 center = new Vector2(640, 110);
            Vector2 origin = size / 2f;

            DrawTitleHighlight(spriteBatch, center, size, scale);

            spriteBatch.DrawString(_titleFont,
                title,
                center + new Vector2(3, 3),
                _deepBlack * _backgroundOpacity,
                0f,
                origin,
                scale,
                SpriteEffects.None,
                0f);

            Color titleColor = Color.Lerp(_ghostWhite, _glowPurple, 0.35f) * _backgroundOpacity;

            spriteBatch.DrawString(_titleFont,
                title,
                center,
                titleColor,
                0f,
                origin,
                scale,
                SpriteEffects.None,
                0f);
        }

        private void DrawTitleHighlight(SpriteBatch spriteBatch, Vector2 center, Vector2 textSize, float scale)
        {
            if (_backgroundOpacity <= 0f) return;

            Vector2 size = textSize * scale;
            var bounds = new Rectangle(
                (int)(center.X - size.X / 2f),
                (int)(center.Y - size.Y / 2f),
                (int)size.X,
                (int)size.Y);

            float travelWidth = bounds.Width + 220f;
            float t = (_pulseTimer * 120f) % travelWidth - 110f;

            Vector2 hlCenter = new Vector2(bounds.X + t, bounds.Center.Y);
            float hlWidth = 140f;
            float hlHeight = bounds.Height * 2f;

            spriteBatch.Draw(
                _pixelTexture,
                hlCenter,
                null,
                new Color(255, 255, 255, 70) * _backgroundOpacity,
                -0.6f,
                new Vector2(0.5f, 0.5f),
                new Vector2(hlWidth, hlHeight),
                SpriteEffects.None,
                0f);
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
            // Panel más ancho y un poco más alto para dos columnas
            int panelWidth = 1200;
            int panelHeight = 450;

            // 🔼 Subido un poco hacia arriba
            int px = 640 - panelWidth / 2;
            int py = 150;

            var panelRect = new Rectangle(px, py, panelWidth, panelHeight);
            spriteBatch.Draw(_pixelTexture, panelRect, new Color(10, 5, 20) * 0.90f);

            // Borde
            int th = 2;
            spriteBatch.Draw(_pixelTexture, new Rectangle(px, py, panelWidth, th), _glowPurple * 0.6f);
            spriteBatch.Draw(_pixelTexture, new Rectangle(px, py + panelHeight - th, panelWidth, th), _glowPurple * 0.6f);
            spriteBatch.Draw(_pixelTexture, new Rectangle(px, py, th, panelHeight), _glowPurple * 0.6f);
            spriteBatch.Draw(_pixelTexture, new Rectangle(px + panelWidth - th, py, th, panelHeight), _glowPurple * 0.6f);

            // Centros de columnas
            float colLeftX = px + panelWidth * 0.25f;
            float colRightX = px + panelWidth * 0.75f;

            // 🔼 Filas un poco más arriba
            float row1Y = py + 80;
            float rowGap = 110f;

            float lineSpacing = _font.LineSpacing + 2;

            void DrawBlock(float centerX, float startY, string title, string[] lines)
            {
                float y = startY;

                Vector2 tSize = _font.MeasureString(title);
                Vector2 tPos = new Vector2(centerX - tSize.X / 2f, y);
                spriteBatch.DrawString(_font, title, tPos, _glowPurple * 0.95f);
                y += lineSpacing + 2;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        y += lineSpacing * 0.4f;
                        continue;
                    }

                    Vector2 s = _font.MeasureString(line);
                    Vector2 pos = new Vector2(centerX - s.X / 2f, y);
                    spriteBatch.DrawString(_font, line, pos, _ghostWhite * 0.9f);
                    y += lineSpacing;
                }
            }

            // BLOQUE 1 (fila 1, izquierda)
            DrawBlock(
                colLeftX,
                row1Y,
                "Desarrollo y Diseño Principal - Cristian Chejo",
                new[]
                {
            "Todo el concepto, programación,",
            "diseño de niveles detallado y gameplay."
                });

            // BLOQUE 2 (fila 1, derecha)
            DrawBlock(
                colRightX,
                row1Y,
                "Diseño Sonoro y Ambientación - Maiky Marupa",
                new[]
                {
            "Efectos de sonido, música ambiental",
            "y atmósfera auditiva."
                });

            // BLOQUE 3 (fila 2, izquierda)
            DrawBlock(
                colLeftX,
                row1Y + rowGap,
                "Arquitectura de Nivel y Diseño de Mapa - Mauricio Vargas",
                new[]
                {
            "Diseño estructural de mapas, level blocking",
            "y composición de escenarios."
                });

            // BLOQUE 4 (fila 2, derecha)
            DrawBlock(
                colRightX,
                row1Y + rowGap,
                "Dirección de Arte y Recursos Gráficos - Matías Sirpa",
                new[]
                {
            "Selección y adaptación de spritesheets",
            "para personajes, enemigos y entorno."
                });

            // BLOQUE 5 (fila 3, izquierda)
            DrawBlock(
                colLeftX,
                row1Y + rowGap * 2f,
                "Control de Calidad y Testing - Jefferson Nina",
                new[]
                {
            "Pruebas, detección de bugs",
            "y feedback de jugabilidad."
                });

            // BLOQUE 6 (fila 3, derecha)
            DrawBlock(
                colRightX,
                row1Y + rowGap * 2f,
                "Asistencia Técnica y Sistemas - Code Copilot (ChatGPT)",
                new[]
                {
            "Apoyo en resolución de problemas",
            "y sistemas complejos."
                });

            // 🔼 Líneas finales, un poco más arriba también
            string engineLine = "Motor del Juego: MonoGame";

            float bottomY = py + panelHeight - 70;

            Vector2 eSize = _font.MeasureString(engineLine);
            Vector2 ePos = new Vector2(640 - eSize.X / 2f, bottomY);
            spriteBatch.DrawString(_font, engineLine, ePos, _ghostWhite * 0.8f);
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

            int th = 2;
            spriteBatch.Draw(_pixelTexture, new Rectangle(box.X, box.Y, box.Width, th), _glowPurple * 0.7f);
            spriteBatch.Draw(_pixelTexture, new Rectangle(box.X, box.Y + box.Height - th, box.Width, th), _glowPurple * 0.7f);
            spriteBatch.Draw(_pixelTexture, new Rectangle(box.X, box.Y, th, box.Height), _glowPurple * 0.7f);
            spriteBatch.Draw(_pixelTexture, new Rectangle(box.X + box.Width - th, box.Y, th, box.Height), _glowPurple * 0.7f);

            spriteBatch.DrawString(_font, text, ipPos, _ghostWhite);
        }

        public void HandleInput() { }
    }
}
