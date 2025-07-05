using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public class MageGuardian : Enemy
    {
        private readonly string color;

        public MageGuardian(Vector2 startPosition, string color) : base(startPosition)
        {
            this.color = color;
        }

        public override void LoadContent(ContentManager content)
        {
            string path = $"Characters/MageGuardian/Idle_{color.ToLower()}";

            animations["Idle"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>(path),
                FrameWidth = 64,    // 896 / 14 frames
                FrameHeight = 64
            };

            currentAnimation = "Idle";
        }

        public override void Update(GameTime gameTime, Vector2 playerPosition)
        {
            currentAnimation = "Idle"; // Siempre quieto
        }
    }
}
