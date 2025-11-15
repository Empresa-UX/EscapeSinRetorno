using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EscapeSinRetorno.Source.Systems.Stats;

namespace EscapeSinRetorno.Source.UI
{
    public sealed class VignetteOverlay
    {
        private readonly Texture2D _tex1px;
        public VignetteOverlay(GraphicsDevice gd)
        {
            _tex1px = new Texture2D(gd, 1, 1);
            _tex1px.SetData(new[] { Color.White });
        }

        public void Draw(SpriteBatch sb, Viewport vp, VisualEffectState fx)
        {
            if (fx.IsDead)
            {
                sb.Draw(_tex1px, new Rectangle(0, 0, vp.Width, vp.Height), Color.Black * 0.85f);
                return;
            }
            if (fx.VignetteIntensity > 0f)
                sb.Draw(_tex1px, new Rectangle(0, 0, vp.Width, vp.Height), Color.Red * MathHelper.Clamp(fx.VignetteIntensity * 0.6f, 0f, 0.6f));
            if (fx.Desaturate > 0f)
                sb.Draw(_tex1px, new Rectangle(0, 0, vp.Width, vp.Height), Color.Green * MathHelper.Clamp(fx.Desaturate * 0.35f, 0f, 0.35f));
            // Distortion se maneja con Effect externo usando fx.DistortionIntensity.
        }
    }
}