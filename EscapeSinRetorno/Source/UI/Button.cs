using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EscapeSinRetorno.Source.UI
{
    public class Button
    {
        protected Rectangle bounds;
        protected SpriteFont font;
        protected string text;
        protected Color backgroundColor;
        protected Color textColor;
        protected Color hoverColor;
        protected bool isHovered;

        public event Action OnClick; // ← Ya está bien, debe ser public

        public Button(Rectangle bounds, string text, SpriteFont font)
        {
            this.bounds = bounds;
            this.text = text;
            this.font = font;
            this.backgroundColor = Color.DarkGray;
            this.textColor = Color.White;
            this.hoverColor = Color.Gray;
        }

        public virtual void Update()
        {
            Point mousePos = InputManager.MousePosition;
            isHovered = bounds.Contains(mousePos);

            if (isHovered && InputManager.IsLeftMouseButtonPressed())
            {
                OnClick?.Invoke();
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            Color currentBg = isHovered ? hoverColor : backgroundColor;
            spriteBatch.Draw(pixelTexture, bounds, currentBg);

            Vector2 textSize = font.MeasureString(text);
            Vector2 textPos = new Vector2(
                bounds.X + (bounds.Width - textSize.X) / 2,
                bounds.Y + (bounds.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(font, text, textPos, textColor);
        }
    }
}