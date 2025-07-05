using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public class AnimationClip
    {
        public Texture2D Texture { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }

        public int TotalFrames => Texture.Width / FrameWidth;
    }
}
