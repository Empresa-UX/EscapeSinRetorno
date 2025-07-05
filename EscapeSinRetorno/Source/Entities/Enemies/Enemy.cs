// File: Source/Entities/Enemies/Enemy.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public abstract class Enemy
    {
        protected Dictionary<string, AnimationClip> animations;
        protected string currentAnimation = "Idle";

        protected Vector2 position;
        protected int currentFrame = 0;
        protected double animationTimer = 0;
        protected double frameInterval = 100; // ms por frame

        protected int frameWidth = 64;
        protected int frameHeight = 64;

        public Vector2 Position => position;

        public Enemy(Vector2 startPosition)
        {
            position = startPosition;
            animations = new();
        }

        public abstract void LoadContent(ContentManager content);
        public abstract void Update(GameTime gameTime, Vector2 playerPosition);

        protected void UpdateAnimation(GameTime gameTime)
        {
            if (!animations.TryGetValue(currentAnimation, out var clip)) return;

            animationTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (animationTimer >= frameInterval)
            {
                currentFrame = (currentFrame + 1) % clip.TotalFrames;
                animationTimer = 0;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!animations.TryGetValue(currentAnimation, out var clip)) return;

            int frame = Math.Clamp(currentFrame, 0, clip.TotalFrames - 1);
            Rectangle source = new Rectangle(frame * clip.FrameWidth, 0, clip.FrameWidth, clip.FrameHeight);

            spriteBatch.Draw(clip.Texture, position, source, Color.White);
        }


    }
}
