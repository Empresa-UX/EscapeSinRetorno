using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Chat
{
    public sealed class ChatRenderer
    {
        private readonly SpriteFont _font;
        private readonly Texture2D _pixel;

        public ChatRenderer(SpriteFont font, Texture2D pixel)
        {
            _font = font;
            _pixel = pixel;
        }

        public void Draw(SpriteBatch spriteBatch, ChatManager chat)
        {
            var messages = chat.Messages;
            bool showPanel = chat.IsOpen || messages.Count > 0;
            if (!showPanel) return;

            int viewportWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            int viewportHeight = spriteBatch.GraphicsDevice.Viewport.Height;

            const int margin = 12;
            const int maxLines = 6;
            int lineHeight = (int)_font.MeasureString("Ay").Y + 2;

            int panelHeight = lineHeight * (maxLines + (chat.IsOpen ? 2 : 0)) + margin * 2;
            int panelWidth = 450;

            Rectangle panel = new Rectangle(
                margin,
                viewportHeight - panelHeight - margin,
                panelWidth,
                panelHeight
            );

            // Fondo
            spriteBatch.Draw(_pixel, panel, new Color(0, 0, 0, 180));

            // Mensajes (últimos maxLines, de más antiguos a más nuevos)
            int start = messages.Count > maxLines ? messages.Count - maxLines : 0;
            int y = panel.Y + margin;
            int x = panel.X + margin;

            for (int i = start; i < messages.Count; i++)
            {
                var m = messages[i];

                Color color = m.Type switch
                {
                    ChatMessageType.System => Color.LightGray,
                    ChatMessageType.Error => Color.IndianRed,
                    _ => Color.White
                };

                spriteBatch.DrawString(_font, m.Text, new Vector2(x, y), color);
                y += lineHeight;
            }

            // Línea de input
            if (chat.IsOpen)
            {
                y += 4;
                string prefix = "> ";
                string text = prefix + chat.CurrentInput;

                bool caretOn = ((int)(chat.CaretTime * 2f) % 2) == 0;
                if (caretOn) text += "_";

                spriteBatch.DrawString(_font, text, new Vector2(x, y), Color.Yellow);
            }
        }
    }
}
