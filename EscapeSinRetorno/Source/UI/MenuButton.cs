using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EscapeSinRetorno.Source.UI
{
    public class MenuButton : Button
    {
        private Color normalColor;
        private Color hoverColor;
        private Color textColor;
        private float hoverIntensity = 0f;
        private bool wasHovered = false;
        private Action clickAction; // ← Nueva variable para manejar el click

        public MenuButton(Rectangle bounds, string text, SpriteFont font, Color normalColor, Color textColor, Color hoverColor)
            : base(bounds, text, font)
        {
            this.normalColor = normalColor;
            this.textColor = textColor;
            this.hoverColor = hoverColor;
        }

        // Método para asignar la acción de click
        public void SetClickAction(Action action)
        {
            clickAction = action;
            OnClick += action; // Aquí sí podemos usar +=
        }

        public override void Update()
        {
            Point mousePos = InputManager.MousePosition;
            bool isCurrentlyHovered = bounds.Contains(mousePos);

            // Animación suave de hover
            if (isCurrentlyHovered)
            {
                hoverIntensity = MathHelper.Min(hoverIntensity + 0.1f, 1f);
            }
            else
            {
                hoverIntensity = MathHelper.Max(hoverIntensity - 0.08f, 0f);
            }

            wasHovered = isCurrentlyHovered;

            if (isCurrentlyHovered && InputManager.IsLeftMouseButtonPressed())
            {
                clickAction?.Invoke(); // Usar nuestra acción directamente
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            // Color del botón con transición suave
            Color currentBg = Color.Lerp(normalColor, hoverColor, hoverIntensity);

            // Borde exterior sutil
            Rectangle outerBorder = new Rectangle(bounds.X - 2, bounds.Y - 2, bounds.Width + 4, bounds.Height + 4);
            spriteBatch.Draw(pixelTexture, outerBorder, new Color(60, 40, 80) * 0.8f);

            // Fondo del botón
            spriteBatch.Draw(pixelTexture, bounds, currentBg);

            // Efecto de brillo en hover
            if (hoverIntensity > 0.1f)
            {
                Color glowColor = hoverColor * 0.3f * hoverIntensity;
                Rectangle glowRect = new Rectangle(bounds.X - 1, bounds.Y - 1, bounds.Width + 2, bounds.Height + 2);
                spriteBatch.Draw(pixelTexture, glowRect, glowColor);
            }

            // Texto del botón
            Vector2 textSize = font.MeasureString(text);
            Vector2 textPos = new Vector2(
                bounds.X + (bounds.Width - textSize.X) / 2,
                bounds.Y + (bounds.Height - textSize.Y) / 2
            );

            Color currentTextColor = Color.Lerp(textColor, Color.White, hoverIntensity * 0.4f);
            spriteBatch.DrawString(font, text, textPos, currentTextColor);
        }
    }
}