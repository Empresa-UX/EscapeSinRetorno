using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public class AnimationClip
    {
        public Texture2D Texture { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public Vector2 Offset { get; set; } = Vector2.Zero;

        public int TotalFrames => Texture.Width / FrameWidth;
    }
}
