using EscapeSinRetorno.Source.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace EscapeSinRetorno.Source.Core
{
    public class MenuState : IGameState
    {
        private List<Button> buttons;
        private SpriteFont font;
        private SpriteFont titleFont;
        private Texture2D pixelTexture;
        private Game1 game;

        // Efectos visuales
        private float pulseTimer = 0f;
        private float backgroundOpacity = 0f;
        private float fadeInTimer = 0f;

        // Colores del tema oscuro
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

            try
            {
                font = content.Load<SpriteFont>("Fonts/MenuFont");
                titleFont = font; // Usamos la misma fuente por ahora
            }
            catch
            {
                throw new Exception("No se encontró la fuente MenuFont.");
            }

            CreateButtons();
        }
        private void CreateButtons()
        {
            int buttonWidth = 280;
            int buttonHeight = 55;
            int spacing = 80;
            int startY = 300;

            buttons.Clear();

            // Botón Nueva Partida - Destacado
            var newGameButton = new MenuButton(
                new Rectangle(500, startY, buttonWidth, buttonHeight),
                "NUEVA PARTIDA", font, glowPurple, ghostWhite, bloodRed);
            newGameButton.SetClickAction(() =>
            {
                backgroundOpacity = 0f;
                game.StartGame();
            });
            buttons.Add(newGameButton);

            // Botón Test 1
            var test1Button = new MenuButton(
                new Rectangle(500, startY + spacing, buttonWidth, buttonHeight),
                "MODO DEBUG", font, darkPurple, ghostWhite, glowPurple);
            test1Button.SetClickAction(() => System.Console.WriteLine("🔍 Modo Debug activado"));
            buttons.Add(test1Button);

            // Botón Test 2
            var test2Button = new MenuButton(
                new Rectangle(500, startY + spacing * 2, buttonWidth, buttonHeight),
                "CONFIGURACIÓN", font, darkPurple, ghostWhite, glowPurple);
            test2Button.SetClickAction(() => System.Console.WriteLine("⚙️ Configuración abierta"));
            buttons.Add(test2Button);

            // Botón Salir - Peligroso
            var exitButton = new MenuButton(
                new Rectangle(500, startY + spacing * 3, buttonWidth, buttonHeight),
                "ESCAPAR", font, deepBlack, bloodRed, ghostWhite);
            exitButton.SetClickAction(() => game.Exit());
            buttons.Add(exitButton);
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Efectos de animación
            pulseTimer += deltaTime * 2f;
            fadeInTimer += deltaTime;
            backgroundOpacity = MathHelper.Min(fadeInTimer * 0.8f, 1f);

            foreach (var button in buttons)
                button.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // Fondo degradado oscuro
            DrawGradientBackground(spriteBatch);

            // Efecto de niebla sutil
            DrawFogEffect(spriteBatch);

            // Título principal con efecto de pulso
            DrawTitle(spriteBatch);

            // Subtítulo ominoso
            DrawSubtitle(spriteBatch);

            // Botones
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
            // Efecto de niebla en los bordes
            Color fogColor = glowPurple * 0.1f * backgroundOpacity;

            // Niebla superior
            Rectangle topFog = new Rectangle(0, 0, 1280, 150);
            spriteBatch.Draw(pixelTexture, topFog, fogColor);

            // Niebla inferior
            Rectangle bottomFog = new Rectangle(0, 570, 1280, 150);
            spriteBatch.Draw(pixelTexture, bottomFog, fogColor);
        }

        private void DrawTitle(SpriteBatch spriteBatch)
        {
            string title = "ESCAPE SIN RETORNO";
            Vector2 titleSize = titleFont.MeasureString(title);
            Vector2 titlePos = new Vector2(640 - titleSize.X / 2, 80);

            // Efecto de pulso sutil
            float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.1f;
            Color titleColor = Color.Lerp(ghostWhite, glowPurple, (float)Math.Sin(pulseTimer * 0.5f) * 0.3f + 0.7f);
            titleColor *= backgroundOpacity;

            // Sombra del título
            spriteBatch.DrawString(titleFont, title, titlePos + new Vector2(3, 3), deepBlack * backgroundOpacity, 0f, Vector2.Zero, pulse, SpriteEffects.None, 0f);
            // Título principal
            spriteBatch.DrawString(titleFont, title, titlePos, titleColor, 0f, Vector2.Zero, pulse, SpriteEffects.None, 0f);
        }

        private void DrawSubtitle(SpriteBatch spriteBatch)
        {
            string subtitle = "~ No hay vuelta atrás ~";
            Vector2 subtitleSize = font.MeasureString(subtitle);
            Vector2 subtitlePos = new Vector2(640 - subtitleSize.X / 2, 180);

            Color subtitleColor = bloodRed * 0.8f * backgroundOpacity;
            spriteBatch.DrawString(font, subtitle, subtitlePos, subtitleColor);
        }

        public void HandleInput()
        {
            // El input se maneja en los botones
        }


    }
}