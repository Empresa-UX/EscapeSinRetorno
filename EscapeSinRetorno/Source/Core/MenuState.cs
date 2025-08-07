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
        private Texture2D pixelTexture;
        private Game1 game;

        public MenuState(Game1 game)
        {
            this.game = game;
            buttons = new List<Button>();
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            // Crear pixel texture para botones
            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

            // Cargar fuente
            try
            {
                font = content.Load<SpriteFont>("Fonts/MenuFont");
            }
            catch
            {
                // Si no existe la fuente, crear una básica
                throw new Exception("No se encontró la fuente MenuFont. Asegúrate de crear el archivo Content/Fonts/MenuFont.spritefont");
            }

            CreateButtons();
        }

        private void CreateButtons()
        {
            int buttonWidth = 200;
            int buttonHeight = 50;
            int spacing = 70;
            int startY = 200;

            // Botón Nueva Partida
            var newGameButton = new Button(
                new Rectangle(540, startY, buttonWidth, buttonHeight),
                "Nueva Partida", font);
            newGameButton.OnClick += () => game.StartGame();
            buttons.Add(newGameButton);

            // Botón Test 1
            var test1Button = new Button(
                new Rectangle(540, startY + spacing, buttonWidth, buttonHeight),
                "Test 1", font);
            test1Button.OnClick += () => System.Console.WriteLine("Test 1 clickeado");
            buttons.Add(test1Button);

            // Botón Test 2
            var test2Button = new Button(
                new Rectangle(540, startY + spacing * 2, buttonWidth, buttonHeight),
                "Test 2", font);
            test2Button.OnClick += () => System.Console.WriteLine("Test 2 clickeado");
            buttons.Add(test2Button);

            // Botón Salir
            var exitButton = new Button(
                new Rectangle(540, startY + spacing * 3, buttonWidth, buttonHeight),
                "Salir", font);
            exitButton.OnClick += () => game.Exit();
            buttons.Add(exitButton);
        }

        public void Update(GameTime gameTime)
        {
            foreach (var button in buttons)
                button.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // Título
            string title = "Escape Sin Retorno";
            Vector2 titleSize = font.MeasureString(title);
            Vector2 titlePos = new Vector2(640 - titleSize.X / 2, 100);
            spriteBatch.DrawString(font, title, titlePos, Color.White);

            // Botones
            foreach (var button in buttons)
                button.Draw(spriteBatch, pixelTexture);

            spriteBatch.End();
        }

        public void HandleInput()
        {
            // El input ya se maneja en Update de los botones
        }
    }
}