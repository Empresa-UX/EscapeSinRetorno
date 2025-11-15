using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EscapeSinRetorno.Source.Systems.Stats;

namespace EscapeSinRetorno.Source.UI
{
    public sealed class StatsHud : IDisposable
    {
        private readonly Texture2D _px;
        private SpriteFont _font;

        public StatsHud(GraphicsDevice gd, SpriteFont font = null)
        {
            _px = new Texture2D(gd, 1, 1);
            _px.SetData(new[] { Color.White });
            _font = font;
        }

        public void SetFont(SpriteFont font) => _font = font;

        public void Draw(SpriteBatch sb, PlayerStats stats, Point screen)
        {
            const int barW = 260, barH = 14, gap = 12;
            int x = 20, y = 20;

            // Panel semitransparente
            DrawBox(sb, new Rectangle(x - 12, y - 16, barW + 24, (barH + gap) * 5 + 8), Color.Black * 0.35f);

            DrawLabeledBar(sb, ref y, "VIDA", stats.Health.Ratio, Color.DarkRed, Color.Red, barW, barH, gap);
            DrawLabeledBar(sb, ref y, "ESTAMINA", stats.Stamina.Ratio, Color.DarkSlateBlue, Color.SlateBlue, barW, barH, gap);
            DrawLabeledBar(sb, ref y, "HAMBRE", stats.Hunger.Ratio, Color.DarkGoldenrod, Color.Gold, barW, barH, gap);
            DrawLabeledBar(sb, ref y, "SED", stats.Thirst.Ratio, Color.DarkCyan, Color.Cyan, barW, barH, gap);
            DrawLabeledBar(sb, ref y, "CORDURA", stats.Sanity.Ratio, Color.DarkOliveGreen, Color.LawnGreen, barW, barH, gap);
        }
        private void DrawBox(SpriteBatch sb, Rectangle r, Color c)
        {
            sb.Draw(_px, r, c);
            sb.Draw(_px, new Rectangle(r.X, r.Y, r.Width, 1), Color.White * 0.2f);
            sb.Draw(_px, new Rectangle(r.X, r.Bottom - 1, r.Width, 1), Color.Black * 0.5f);
        }
        private void DrawLabeledBar(SpriteBatch sb, ref int y, string label, float ratio, Color back, Color fill, int w, int h, int gap)
        {
            ratio = MathHelper.Clamp(ratio, 0f, 1f);
            int x = 20;

            // Etiqueta con sombra
            if (_font != null)
            {
                var pos = new Vector2(x, y);
                sb.DrawString(_font, label, pos + new Vector2(1, 1), Color.Black * 0.8f);
                sb.DrawString(_font, label, pos, Color.White);
            }

            y += _font != null ? (int)_font.LineSpacing - 2 : 14;

            var rectBack = new Rectangle(x, y, w, h);
            var rectFill = new Rectangle(x, y, (int)(w * ratio), h);
            sb.Draw(_px, rectBack, back * 0.45f);
            sb.Draw(_px, rectFill, fill);

            if (_font != null)
            {
                string pct = $"{(int)(ratio * 100)}%";
                var size = _font.MeasureString(pct);
                var pos = new Vector2(x + w - size.X - 4, y - (size.Y - h) / 2f);
                sb.DrawString(_font, pct, pos + new Vector2(1, 1), Color.Black * 0.8f);
                sb.DrawString(_font, pct, pos, Color.White);
            }

            // borde
            sb.Draw(_px, new Rectangle(rectBack.X - 1, rectBack.Y - 1, rectBack.Width + 2, 1), Color.Black);
            sb.Draw(_px, new Rectangle(rectBack.X - 1, rectBack.Bottom, rectBack.Width + 2, 1), Color.Black);
            sb.Draw(_px, new Rectangle(rectBack.X - 1, rectBack.Y, 1, rectBack.Height), Color.Black);
            sb.Draw(_px, new Rectangle(rectBack.Right, rectBack.Y, 1, rectBack.Height), Color.Black);

            y += h + gap;
        }

        public void Dispose() => _px?.Dispose();
    }
}