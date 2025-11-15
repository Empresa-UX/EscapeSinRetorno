using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using EscapeSinRetorno.Source.Net;

namespace EscapeSinRetorno.Source.Multiplayer
{
    public sealed class RemotePlayer
    {
        private Texture2D _idle, _walk, _run, _hurt, _death;
        private string _anim = "Idle";
        private int _frame, _frameW = 128, _frameH = 128;
        private double _timer, _interval = 120;
        private SpriteEffects _flip = SpriteEffects.None;
        private float _scale = 0.5f;
        public Vector2 Position;

        public void LoadContent(ContentManager content)
        {
            _idle = content.Load<Texture2D>("Characters/Enchantress/Idle");
            _walk = content.Load<Texture2D>("Characters/Enchantress/Walk");
            _run = content.Load<Texture2D>("Characters/Enchantress/Run");
            _hurt = content.Load<Texture2D>("Characters/Enchantress/Hurt");
            try { _death = content.Load<Texture2D>("Characters/Enchantress/Death"); } catch { _death = null; }
        }

        public void ApplyNetState(NetPlayerState s)
        {
            Position = new Vector2(s.X, s.Y);
            _flip = s.Flip == NetFlip.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            _anim = s.Anim switch
            {
                NetAnim.Run => "Run",
                NetAnim.Walk => "Walk",
                NetAnim.Hurt => "Hurt",
                NetAnim.Death => "Death",
                _ => "Idle"
            };
        }

        public void Update(GameTime gt)
        {
            _timer += gt.ElapsedGameTime.TotalMilliseconds;
            if (_timer > _interval) { _timer = 0; _frame++; }
        }

        public void Draw(SpriteBatch sb)
        {
            var tex = _anim switch
            {
                "Run" => _run,
                "Walk" => _walk,
                "Hurt" => _hurt,
                "Death" => _death ?? _hurt,
                _ => _idle
            };
            if (tex == null) return;
            int frames = tex.Width / _frameW; if (frames <= 0) return;
            if (_frame >= frames) _frame = (_anim == "Run" || _anim == "Walk" || _anim == "Idle") ? 0 : frames - 1;
            var src = new Rectangle((_frame % frames) * _frameW, 0, _frameW, _frameH);
            sb.Draw(tex, Position, src, Color.White, 0f, Vector2.Zero, _scale, _flip, 0f);
        }
    }
}