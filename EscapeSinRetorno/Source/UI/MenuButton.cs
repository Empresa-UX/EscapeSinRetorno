using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EscapeSinRetorno.Source.UI
{
    public class MenuButton : Button
    {
        private Color _normalBg;
        private Color _hoverBg;
        private Color _textColor;

        private float _hoverIntensity = 0f;
        private bool _wasHovered = false;
        private Action _clickAction;

        public MenuButton(Rectangle bounds, string text, SpriteFont font, Color normalBg, Color textColor, Color hoverBg)
            : base(bounds, text, font)
        {
            _normalBg = normalBg;
            _textColor = textColor;
            _hoverBg = hoverBg;
        }

        public void SetClickAction(Action action)
        {
            _clickAction = action;
            OnClick += action;
        }

        public override void Update()
        {
            Point mousePos = InputManager.MousePosition;
            bool isCurrentlyHovered = bounds.Contains(mousePos);

            _hoverIntensity = isCurrentlyHovered
                ? MathHelper.Min(_hoverIntensity + 0.10f, 1f)
                : MathHelper.Max(_hoverIntensity - 0.08f, 0f);

            _wasHovered = isCurrentlyHovered;

            if (isCurrentlyHovered && InputManager.IsLeftMouseButtonPressed())
                _clickAction?.Invoke();
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            Color currentBg = Color.Lerp(_normalBg, _hoverBg, _hoverIntensity);

            Rectangle outerBorder = new Rectangle(bounds.X - 2, bounds.Y - 2, bounds.Width + 4, bounds.Height + 4);
            spriteBatch.Draw(pixelTexture, outerBorder, new Color(60, 40, 80) * 0.8f);

            spriteBatch.Draw(pixelTexture, bounds, currentBg);

            if (_hoverIntensity > 0.1f)
            {
                Color glowColor = _hoverBg * 0.3f * _hoverIntensity;
                Rectangle glowRect = new Rectangle(bounds.X - 1, bounds.Y - 1, bounds.Width + 2, bounds.Height + 2);
                spriteBatch.Draw(pixelTexture, glowRect, glowColor);
            }

            Vector2 textSize = font.MeasureString(text);
            Vector2 textPos = new Vector2(
                bounds.X + (bounds.Width - textSize.X) / 2,
                bounds.Y + (bounds.Height - textSize.Y) / 2
            );

            Color currentTextColor = Color.Lerp(_textColor, Color.White, _hoverIntensity * 0.4f);
            spriteBatch.DrawString(font, text, textPos, currentTextColor);
        }
    }
}
