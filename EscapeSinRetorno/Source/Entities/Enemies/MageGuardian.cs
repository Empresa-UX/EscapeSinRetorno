using EscapeSinRetorno.Source.World;
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
                FrameWidth = 64,
                FrameHeight = 64,
            };

            currentAnimation = "Idle";

            hitboxWidth = (int)(animations["Idle"].FrameWidth * 0.4f);
            hitboxHeight = (int)(animations["Idle"].FrameHeight * 0.6f);
        }

        public override void Update(GameTime gameTime, Player player, TileMap tileMap)
        {
            currentAnimation = "Idle";
            UpdateAnimation(gameTime);
        }
        public override void TakeDamage(int dmg) { }
    }
}
