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
    public abstract class Enemy
    {
        protected Dictionary<string, Texture2D> animations;
        protected Vector2 position;
        protected string currentAnimation = "Idle";

        public Vector2 Position => position;

        public Enemy(Vector2 startPosition)
        {
            position = startPosition;
            animations = new();
        }

        public abstract void LoadContent(ContentManager content);
        public abstract void Update(GameTime gameTime, Vector2 playerPosition);

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            if (animations.TryGetValue(currentAnimation, out var tex))
                spriteBatch.Draw(tex, position - camera, Color.White);
        }
    }
}