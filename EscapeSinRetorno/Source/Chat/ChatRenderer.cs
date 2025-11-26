using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace EscapeSinRetorno.Source.Chat
{
    public sealed class ChatRenderer
    {
        private readonly SpriteFont _font;
        private readonly Texture2D _pixel;

        private int _scrollOffset = 0;  // en líneas

        // ancho máximo del panel para wrap
        private const int PanelWidth = 450 - 24; // 12px margen a cada lado

        public ChatRenderer(SpriteFont font, Texture2D pixel)
        {
            _font = font;
            _pixel = pixel;
        }

        public void ResetScroll() => _scrollOffset = 0;

        public void AdjustScroll(int delta)
        {
            _scrollOffset = Math.Max(0, _scrollOffset + delta);
        }

        public void Draw(SpriteBatch spriteBatch, ChatManager chat)
        {
            var messages = chat.Messages;
            bool showPanel = chat.IsOpen || messages.Count > 0;
            if (!showPanel) return;

            int viewportW = spriteBatch.GraphicsDevice.Viewport.Width;
            int viewportH = spriteBatch.GraphicsDevice.Viewport.Height;

            const int margin = 12;
            const int maxLines = 6;
            int lineHeight = (int)_font.MeasureString("Ay").Y + 2;

            // panel
            int panelHeight =
                lineHeight * (maxLines + (chat.IsOpen ? 2 : 0)) + margin * 2;

            Rectangle panel = new Rectangle(
                margin,
                viewportH - panelHeight - margin,
                450,
                panelHeight
            );

            // fondo
            spriteBatch.Draw(_pixel, panel, new Color(0, 0, 0, 180));

            // -----------------------------
            // PREPARAR LÍNEAS CON WRAP
            // -----------------------------
            List<(string text, Color col)> wrappedLines = new();

            foreach (var msg in messages)
            {
                Color color = msg.Type switch
                {
                    ChatMessageType.System => Color.LightGray,
                    ChatMessageType.Error => Color.IndianRed,
                    _ => Color.White
                };

                WrapIntoLines(msg.Text, color, wrappedLines);
            }

            // -----------------------------
            // Dibujar líneas con scroll
            // -----------------------------
            int maxVisible = maxLines;
            int total = wrappedLines.Count;

            int start = Math.Max(0, total - maxVisible - _scrollOffset);
            int end = Math.Min(total, start + maxVisible);

            int x = panel.X + margin;
            int y = panel.Y + margin;

            for (int i = start; i < end; i++)
            {
                var (text, color) = wrappedLines[i];
                string safeText = Sanitize(text);
                spriteBatch.DrawString(_font, safeText, new Vector2(x, y), color);
                y += lineHeight;
            }

            // -----------------------------
            // INPUT (con wrapping)
            // -----------------------------
            if (chat.IsOpen)
            {
                y += 6;
                List<string> inputLines = WrapInput(chat.CurrentInput);

                // caret
                bool caretOn = ((int)(chat.CaretTime * 2f) % 2) == 0;
                if (caretOn && inputLines.Count > 0)
                    inputLines[^1] += "|"; // se sanitiza antes de dibujar

                foreach (var l in inputLines)
                {
                    string safe = Sanitize(l);
                    spriteBatch.DrawString(_font, safe, new Vector2(x, y), Color.Yellow);
                    y += lineHeight;
                }
            }
        }

        // ============================================================
        // WORD WRAP DE MENSAJES
        // ============================================================
        private void WrapIntoLines(string text, Color col, List<(string, Color)> output)
        {
            string[] words = text.Split(' ');
            StringBuilder line = new();

            foreach (var w in words)
            {
                string tryLine = line.Length == 0 ? w : $"{line} {w}";
                float width = _font.MeasureString(Sanitize(tryLine)).X;

                if (width > PanelWidth)
                {
                    // guardar línea actual
                    if (line.Length > 0)
                        output.Add((line.ToString(), col));
                    line.Clear();
                    line.Append(w);
                }
                else
                {
                    if (line.Length > 0) line.Append(' ');
                    line.Append(w);
                }
            }

            if (line.Length > 0)
                output.Add((line.ToString(), col));
        }

        // ============================================================
        // WORD WRAP DEL INPUT DEL JUGADOR
        // ============================================================
        private List<string> WrapInput(string input)
        {
            List<string> lines = new();
            string prefix = "> ";

            string full = prefix + input;
            string[] words = full.Split(' ');

            StringBuilder line = new();

            foreach (var w in words)
            {
                string tryLine = line.Length == 0 ? w : $"{line} {w}";
                float width = _font.MeasureString(Sanitize(tryLine)).X;

                if (width > PanelWidth)
                {
                    if (line.Length > 0)
                        lines.Add(line.ToString());
                    line.Clear();
                    line.Append(w);
                }
                else
                {
                    if (line.Length > 0) line.Append(' ');
                    line.Append(w);
                }
            }

            if (line.Length > 0)
                lines.Add(line.ToString());

            return lines;
        }

        private string Sanitize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            StringBuilder sb = new();

            foreach (char c in text)
            {
                if (_font.Characters.Contains(c))
                    sb.Append(c);
                else
                    sb.Append('?'); // caracter de reemplazo
            }

            return sb.ToString();
        }
    }
}
