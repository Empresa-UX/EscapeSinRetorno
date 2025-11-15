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

        // Input IP
        private TextInput ipInput;

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

            ipInput = new TextInput(new Rectangle(480, 210, 320, 44), font, graphicsDevice)
            {
                Placeholder = "192.168.0.15  ó  1.2.3.4"
            };

            CreateButtons();
        }

        private void CreateButtons()
        {
            int buttonWidth = 320, buttonHeight = 55, spacing = 70, startY = 280;
            buttons.Clear();

            // Un jugador (OFFLINE)
            var spBtn = new MenuButton(new Rectangle(480, startY, buttonWidth, buttonHeight),
                "UN JUGADOR (OFFLINE)", font, glowPurple, ghostWhite, bloodRed);
            spBtn.SetClickAction(() => game.StartSingleplayer());
            buttons.Add(spBtn);

            // Test Localhost (cliente)
            var localBtn = new MenuButton(new Rectangle(480, startY + spacing, buttonWidth, buttonHeight),
                "TEST LOCALHOST (CLIENTE)", font, darkPurple, ghostWhite, glowPurple);
            localBtn.SetClickAction(() => game.StartLocalhostClient());
            buttons.Add(localBtn);

            // Multijugador: Hostear (levanta servidor en esta PC y se conecta)
            var hostBtn = new MenuButton(new Rectangle(480, startY + spacing * 2, buttonWidth, buttonHeight),
                "MULTIJUGADOR: HOSTEAR", font, glowPurple, ghostWhite, bloodRed);
            hostBtn.SetClickAction(() => game.HostAndJoin());
            buttons.Add(hostBtn);

            // Multijugador: Unirse (IP)
            var joinBtn = new MenuButton(new Rectangle(480, startY + spacing * 3, buttonWidth, buttonHeight),
                "MULTIJUGADOR: UNIRSE (IP)", font, darkPurple, ghostWhite, glowPurple);
            joinBtn.SetClickAction(() =>
            {
                string host = string.IsNullOrWhiteSpace(ipInput.Text) ? "127.0.0.1" : ipInput.Text;
                // soportar ip:port (el cliente usa puerto por defecto; puedes ampliar si deseas)
                if (host.Contains(":")) host = host.Split(':')[0];
                game.JoinByIp(host);
            });
            buttons.Add(joinBtn);

            // Salir
            var exitBtn = new MenuButton(new Rectangle(480, startY + spacing * 4, buttonWidth, buttonHeight),
                "SALIR", font, deepBlack, bloodRed, ghostWhite);
            exitBtn.SetClickAction(() => game.Exit());
            buttons.Add(exitBtn);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            pulseTimer += dt * 2f;
            fadeInTimer += dt;
            backgroundOpacity = MathHelper.Min(fadeInTimer * 0.8f, 1f);

            ipInput.Update(gameTime);
            foreach (var button in buttons) button.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            DrawGradientBackground(spriteBatch);
            DrawFogEffect(spriteBatch);
            DrawTitle(spriteBatch);
            DrawSubtitle(spriteBatch);

            // Etiqueta IP
            var lbl = "IP del Host (LAN/Internet):";
            var sz = font.MeasureString(lbl);
            spriteBatch.DrawString(font, lbl, new Vector2(640 - sz.X / 2, 180), ghostWhite * 0.9f);

            ipInput.Draw(spriteBatch);

            foreach (var button in buttons)
                button.Draw(spriteBatch, pixelTexture);

            spriteBatch.End();
        }

        private void DrawGradientBackground(SpriteBatch spriteBatch)
        {
            int screenHeight = 720;
            int gradientSteps = 50;
            int stepHeight = screenHeight / gradientSteps;

            for (int i = 0; i < gradientSteps; i++)
            {
                float lerp = (float)i / gradientSteps;
                Color color = Color.Lerp(deepBlack, darkPurple, lerp * 0.6f);
                color *= backgroundOpacity;
                Rectangle rect = new Rectangle(0, i * stepHeight, 1280, stepHeight + 1);
                spriteBatch.Draw(pixelTexture, rect, color);
            }
        }

        private void DrawFogEffect(SpriteBatch spriteBatch)
        {
            Color fogColor = glowPurple * 0.1f * backgroundOpacity;
            Rectangle topFog = new Rectangle(0, 0, 1280, 150);
            spriteBatch.Draw(pixelTexture, topFog, fogColor);
            Rectangle bottomFog = new Rectangle(0, 570, 1280, 150);
            spriteBatch.Draw(pixelTexture, bottomFog, fogColor);
        }

        private void DrawTitle(SpriteBatch spriteBatch)
        {
            string title = "ESCAPE SIN RETORNO";
            Vector2 titleSize = titleFont.MeasureString(title);
            Vector2 titlePos = new Vector2(640 - titleSize.X / 2, 80);
            float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.1f;
            Color titleColor = Color.Lerp(ghostWhite, glowPurple, (float)Math.Sin(pulseTimer * 0.5f) * 0.3f + 0.7f);
            titleColor *= backgroundOpacity;

            spriteBatch.DrawString(titleFont, title, titlePos + new Vector2(3, 3), deepBlack * backgroundOpacity, 0f, Vector2.Zero, pulse, SpriteEffects.None, 0f);
            spriteBatch.DrawString(titleFont, title, titlePos, titleColor, 0f, Vector2.Zero, pulse, SpriteEffects.None, 0f);
        }

        private void DrawSubtitle(SpriteBatch spriteBatch)
        {
            string subtitle = "~ Elige un modo: Offline / Localhost / Multijugador ~";
            Vector2 subtitleSize = font.MeasureString(subtitle);
            Vector2 subtitlePos = new Vector2(640 - subtitleSize.X / 2, 140);
            Color subtitleColor = bloodRed * 0.8f * backgroundOpacity;
            spriteBatch.DrawString(font, subtitle, subtitlePos, subtitleColor);
        }

        public void HandleInput() { }
    }
}
