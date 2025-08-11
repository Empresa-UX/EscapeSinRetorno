using EscapeSinRetorno.Source.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public abstract class Enemy
    {
        protected int hitboxWidth, hitboxHeight;
        protected Dictionary<string, AnimationClip> animations = new();
        protected string currentAnimation = "Idle";
        protected Vector2 position;
        protected int currentFrame = 0;
        protected double animationTimer = 0;
        protected double frameInterval = 100;
        protected SpriteEffects flip = SpriteEffects.None;
        protected static Texture2D debugPixel;

        public Vector2 Position => position;
        public Vector2 Center => animations.TryGetValue(currentAnimation, out var clip)
            ? new Vector2(position.X, position.Y - clip.FrameHeight / 2f) : position;

        public Enemy(Vector2 startPosition) => position = startPosition;

        public abstract void LoadContent(ContentManager content);
        public abstract void Update(GameTime gameTime, Player player, TileMap tileMap);
        public abstract void TakeDamage(int dmg);
        public static void LoadDebugTexture(GraphicsDevice device)
        {
            debugPixel = new Texture2D(device, 1, 1);
            debugPixel.SetData(new[] { Color.White });
        }

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

            var source = new Rectangle(Math.Clamp(currentFrame, 0, clip.TotalFrames - 1) * clip.FrameWidth, 0, clip.FrameWidth, clip.FrameHeight);
            var origin = new Vector2(clip.FrameWidth / 2f, clip.FrameHeight * 0.7f);

            spriteBatch.Draw(clip.Texture, position, source, Color.White, 0f, origin, 1f, flip, 0f);
            if (debugPixel != null) spriteBatch.Draw(debugPixel, GetHitbox(), Color.Red * 0.3f);
        }

        protected void PlayAnimation(string name)
        {
            if (currentAnimation == name || !animations.ContainsKey(name)) return;
            currentAnimation = name;
            currentFrame = 0;
            animationTimer = 0;
        }

        public virtual Rectangle GetHitbox() => CreateHitboxAt(position);

        protected bool TryMoveToward(Vector2 move, float delta, TileMap tileMap)
        {
            if (move.LengthSquared() < 1e-6f) return false;

            var positions = new[] {
                position + move * delta,
                position + new Vector2(move.X, 0) * delta,
                position + new Vector2(0, move.Y) * delta
            };

            foreach (var pos in positions)
            {
                var hitbox = CreateHitboxAt(pos);
                if (!tileMap.IsColliding(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height))
                {
                    position = pos;
                    UpdateFlip(pos == positions[0] ? move : pos == positions[1] ? new Vector2(move.X, 0) : new Vector2(0, move.Y));
                    return true;
                }
            }
            return false;
        }

        private void UpdateFlip(Vector2 direction)
        {
            if (direction.X > 0.1f) flip = SpriteEffects.None;
            else if (direction.X < -0.1f) flip = SpriteEffects.FlipHorizontally;
        }

        private Rectangle CreateHitboxAt(Vector2 pos)
        {
            if (!animations.TryGetValue(currentAnimation, out var clip))
                return new Rectangle((int)pos.X, (int)pos.Y, 1, 1);

            int width = hitboxWidth > 0 ? hitboxWidth : (int)(clip.FrameWidth * 0.4f);
            int height = hitboxHeight > 0 ? hitboxHeight : (int)(clip.FrameHeight * 0.4f);

            return new Rectangle((int)(pos.X - width / 2f), (int)(pos.Y - height), width, height);
        }


        protected bool IsCollidingWith(Rectangle other) => GetHitbox().Intersects(other);
    }
}