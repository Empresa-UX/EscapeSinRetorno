using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string[] states = { "Attack", "Death_1", "Death_2", "Hurt", "Idle", "Run" };
            foreach (var state in states)
                animations[state] = content.Load<Texture2D>($"{basePath}{state}");

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