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
        private float moveTimer = 0;
        private Vector2 velocity = Vector2.Zero;

        public NightBorne(Vector2 startPosition) : base(startPosition) { }

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
            moveTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (moveTimer <= 0)
            {
                velocity = new Vector2(
                    (float)(Game1.Random.NextDouble() * 2 - 1),
                    (float)(Game1.Random.NextDouble() * 2 - 1));
                velocity.Normalize();
                velocity *= 40f;
                moveTimer = 2f;
            }

            position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            currentAnimation = "Run";
        }
    }
}