using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace EscapeSinRetorno.Source.UI
{
    public sealed class DeathScreen
    {
        private readonly Texture2D _px;
        private readonly SpriteFont _font;

        private MenuButton _btnRetry;
        private MenuButton _btnMenu;

        private readonly Color accentA = new Color(120, 80, 150); // púrpura tenue
        private readonly Color accentB = new Color(200, 40, 60);  // rojo tenue
        private readonly Color textCol = new Color(230, 230, 240);

        private float _pulse;

        public DeathScreen(GraphicsDevice gd, SpriteFont font, System.Action onRetry, System.Action onMenu)
        {
            _px = new Texture2D(gd, 1, 1);
            _px.SetData(new[] { Color.White });
            _font = font;

            BuildButtons(gd.Viewport, onRetry, onMenu);
        }

        public void Resize(Viewport vp, System.Action onRetry, System.Action onMenu)
        {
            BuildButtons(vp, onRetry, onMenu);
        }

        private void BuildButtons(Viewport vp, System.Action onRetry, System.Action onMenu)
        {
            // Layout centrado bajo el título – sin paneles de fondo
            int btnW = 300, btnH = 56, gap = 18;
            int centerX = vp.Width / 2;
            int baseY = vp.Height / 2 + 40;

            _btnRetry = new MenuButton(
                new Rectangle(centerX - btnW - gap / 2, baseY, btnW, btnH),
                "REINTENTAR", _font, accentA, textCol, accentB);
            _btnRetry.SetClickAction(onRetry);

            _btnMenu = new MenuButton(
                new Rectangle(centerX + gap / 2, baseY, btnW, btnH),
                "VOLVER AL MENÚ", _font, accentA, textCol, accentB);
            _btnMenu.SetClickAction(onMenu);
        }

        public void Update(GameTime gt)
        {
            _pulse += (float)gt.ElapsedGameTime.TotalSeconds * 2.2f;

            // Atajos de teclado
            if (InputManager.IsKeyPressed(Keys.Enter))
                _btnRetry?.Update();

            if (InputManager.IsKeyPressed(Keys.Escape))
                _btnMenu?.Update();

            // Mouse/hover
            _btnRetry?.Update();
            _btnMenu?.Update();
        }


        public void Draw(SpriteBatch sb, Viewport vp)
        {
            // Fondo negro translúcido
            sb.Draw(_px, new Rectangle(0, 0, vp.Width, vp.Height), Color.Black * 0.78f);

            // Viñeta suave (sin paneles raros)
            DrawVignette(sb, vp);

            // Título + subtítulo
            string title = "HAS MUERTO";
            string subtitle = "La mazmorra te reclama";

            float scale = 1.15f + 0.03f * MathF.Sin(_pulse); // pulso sutil
            var tSize = _font.MeasureString(title) * scale;
            var sSize = _font.MeasureString(subtitle);

            var tPos = new Vector2(vp.Width / 2f - tSize.X / 2f, vp.Height / 2f - 120);
            var sPos = new Vector2(vp.Width / 2f - sSize.X / 2f, tPos.Y + tSize.Y + 6);

            // Sombra + texto
            sb.DrawString(_font, title, tPos + new Vector2(2, 2), Color.Black * 0.6f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            sb.DrawString(_font, title, tPos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            sb.DrawString(_font, subtitle, sPos + new Vector2(1, 1), Color.Black * 0.55f);
            sb.DrawString(_font, subtitle, sPos, Color.LightGray);

            // Botones (sin fondo adicional)
            _btnRetry.Draw(sb, _px);
            _btnMenu.Draw(sb, _px);
        }

        private void DrawVignette(SpriteBatch sb, Viewport vp)
        {
            // Bordes oscuros sin paneles internos
            int thickness = 120;
            var c = Color.Black * 0.35f;

            // Top / Bottom
            sb.Draw(_px, new Rectangle(0, 0, vp.Width, thickness), c);
            sb.Draw(_px, new Rectangle(0, vp.Height - thickness, vp.Width, thickness), c);
            // Left / Right
            sb.Draw(_px, new Rectangle(0, 0, thickness, vp.Height), c);
            sb.Draw(_px, new Rectangle(vp.Width - thickness, 0, thickness, vp.Height), c);

            // Línea sutil separadora encima de los botones
            sb.Draw(_px, new Rectangle(0, vp.Height / 2 + 20, vp.Width, 1), Color.White * 0.08f);
        }
    }
}
