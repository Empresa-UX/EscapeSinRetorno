using EscapeSinRetorno.Source.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace EscapeSinRetorno.Source.Core
{
    public class MenuState : IGameState
    {
        private List<Button> buttons;
        private SpriteFont font;
        private SpriteFont titleFont;
        private Texture2D pixelTexture;
        private Game1 game;

        private float pulseTimer = 0f;
        private float backgroundOpacity = 0f;
        private float fadeInTimer = 0f;

        private readonly Color darkPurple = new Color(25, 15, 35);
        private readonly Color deepBlack = new Color(10, 5, 15);
        private readonly Color glowPurple = new Color(120, 80, 150);
        private readonly Color bloodRed = new Color(120, 20, 30);
        private readonly Color ghostWhite = new Color(220, 220, 230);

        public MenuState(Game1 game)
        {
            this.game = game;
            buttons = new List<Button>();
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

            font = content.Load<SpriteFont>("Fonts/MenuFont");
            titleFont = font;

            CreateButtons();
        }

        private void CreateButtons()
        {
            int buttonWidth = 320;
            int buttonHeight = 55;
            int spacing = 70;
            int startY = 280;

            buttons.Clear();

            var offline = new MenuButton(
                new Rectangle(480, startY, buttonWidth, buttonHeight),
                "OFFLINE (SIN RED)", font, glowPurple, ghostWhite, bloodRed);
            offline.SetClickAction(() => { game.StartOfflineGame(); });
            buttons.Add(offline);

            var localhost = new MenuButton(
                new Rectangle(480, startY + spacing, buttonWidth, buttonHeight),
                "TEST LOCALHOST (CLIENTE)", font, darkPurple, ghostWhite, glowPurple);
            localhost.SetClickAction(() => { game.StartLocalhostClient(); });
            buttons.Add(localhost);

            var join = new MenuButton(
                new Rectangle(480, startY + spacing * 2, buttonWidth, buttonHeight),
                "UNIRSE (IP DEL HOST)", font, darkPurple, ghostWhite, glowPurple);
            join.SetClickAction(() =>
            {
                // TODO: reemplazar por tu TextInput
                string host = "192.168.0.10";
                game.JoinByIp(host);
            });
            buttons.Add(join);

            var exitButton = new MenuButton(
                new Rectangle(480, startY + spacing * 3, buttonWidth, buttonHeight),
                "SALIR", font, deepBlack, bloodRed, ghostWhite);
            exitButton.SetClickAction(() => game.Exit());
            buttons.Add(exitButton);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            pulseTimer += dt * 2f;
            fadeInTimer += dt;
            backgroundOpacity = MathHelper.Min(fadeInTimer * 0.8f, 1f);

            foreach (var button in buttons) button.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            DrawMenuBg(spriteBatch);
            DrawTitle(spriteBatch);
            foreach (var button in buttons) button.Draw(spriteBatch, pixelTexture);
            spriteBatch.End();
        }

        // RENOMBRADO para evitar ambigüedad
        private void DrawMenuBg(SpriteBatch spriteBatch)
        {
            int screenHeight = 720;
            int steps = 50;
            int stepH = screenHeight / steps;

            for (int i = 0; i < steps; i++)
            {
                float lerp = (float)i / steps;
                Color color = Color.Lerp(deepBlack, darkPurple, lerp * 0.6f) * backgroundOpacity;
                Rectangle rect = new Rectangle(0, i * stepH, 1280, stepH + 1);
                spriteBatch.Draw(pixelTexture, rect, color);
            }
        }

        private void DrawTitle(SpriteBatch spriteBatch)
        {
            string title = "ESCAPE SIN RETORNO";
            Vector2 size = titleFont.MeasureString(title);
            Vector2 pos = new Vector2(640 - size.X / 2, 80);
            float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.1f;
            Color titleColor = Color.Lerp(ghostWhite, glowPurple, (float)Math.Sin(pulseTimer * 0.5f) * 0.3f + 0.7f) * backgroundOpacity;

            spriteBatch.DrawString(titleFont, title, pos + new Vector2(3, 3), deepBlack * backgroundOpacity, 0f, Vector2.Zero, pulse, SpriteEffects.None, 0f);
            spriteBatch.DrawString(titleFont, title, pos, titleColor, 0f, Vector2.Zero, pulse, SpriteEffects.None, 0f);
        }

        public void HandleInput() { }
    }
}