using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public class NightBorne : Enemy
    {
        private float moveTimer = 0f;
        private Vector2 velocity = Vector2.Zero;
        private Vector2 initialPosition;
        private float patrolRange = 100f;

        public NightBorne(Vector2 startPosition) : base(startPosition)
        {
            initialPosition = startPosition;
        }

        public override void LoadContent(ContentManager content)
        {
            string basePath = "Characters/NightBorne/";

            animations["Attack"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Attack"),
                FrameWidth = 85,
                FrameHeight = 69
            };

            animations["Death_1"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Death_1"),
                FrameWidth = 85,
                FrameHeight = 67
            };

            animations["Death_2"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Death_2"),
                FrameWidth = 94,
                FrameHeight = 73
            };

            animations["Hurt"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Hurt"),
                FrameWidth = 91,
                FrameHeight = 46
            };

            animations["Idle"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Idle"),
                FrameWidth = 58,
                FrameHeight = 51
            };

            animations["Run"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Run"),
                FrameWidth = 45,
                FrameHeight = 45
            };

            currentAnimation = "Idle";
        }

        public override void Update(GameTime gameTime, Vector2 playerPosition)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            moveTimer -= delta;

            if (moveTimer <= 0f)
            {
                velocity = new Vector2(
                    (float)(Game1.Random.NextDouble() * 2 - 1),
                    (float)(Game1.Random.NextDouble() * 2 - 1));
                if (velocity != Vector2.Zero) velocity.Normalize();
                velocity *= 40f;
                moveTimer = 2f;
            }

            Vector2 nextPos = position + velocity * delta;
            if ((nextPos - initialPosition).Length() <= patrolRange)
            {
                position = nextPos;
                currentAnimation = "Run";
            }
            else
            {
                currentAnimation = "Idle";
            }
        }
    }
}
