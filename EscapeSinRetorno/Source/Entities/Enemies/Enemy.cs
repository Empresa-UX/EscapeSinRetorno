// File: Source/Entities/Enemies/Enemy.cs
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
        protected int hitboxWidth;
        protected int hitboxHeight;

        protected Dictionary<string, AnimationClip> animations;
        protected string currentAnimation = "Idle";

        protected Vector2 position;
        protected int currentFrame = 0;
        protected double animationTimer = 0;
        protected double frameInterval = 100; // ms por frame

        protected SpriteEffects flip = SpriteEffects.None;

        public Vector2 Position => position;
        public Vector2 Center
        {
            get
            {
                if (!animations.TryGetValue(currentAnimation, out var clip))
                    return position;

                return new Vector2(
                    position.X,
                    position.Y - clip.FrameHeight / 2f  // Centro vertical del sprite
                );
            }
        }
        public Enemy(Vector2 startPosition)
        {
            position = startPosition;
            animations = new();
        }

        public abstract void LoadContent(ContentManager content);
        public abstract void Update(GameTime gameTime, Vector2 playerPosition, TileMap tileMap);

        protected static Texture2D debugPixel;

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

            int frame = Math.Clamp(currentFrame, 0, clip.TotalFrames - 1);
            Rectangle source = new Rectangle(frame * clip.FrameWidth, 0, clip.FrameWidth, clip.FrameHeight);

            Vector2 origin = new Vector2(clip.FrameWidth / 2f, clip.FrameHeight * 0.7f); // menos cabeza, más cuerpo

            spriteBatch.Draw(
                clip.Texture,
                position,
                source,
                Color.White,
                0f,
                origin,
                1f,
                flip,
                0f
            );


            if (debugPixel != null)
                spriteBatch.Draw(debugPixel, GetHitbox(), Color.Red * 0.3f);
        }

        protected void PlayAnimation(string name)
        {
            if (currentAnimation == name) return;

            if (animations.ContainsKey(name))
            {
                currentAnimation = name;
                currentFrame = 0;
                animationTimer = 0;
            }
        }
        public virtual Rectangle GetHitbox()
        {
            return CreateHitboxAt(position);
        }


        protected bool TryMoveToward(Vector2 move, float delta, TileMap tileMap)
        {
            if (move.LengthSquared() < 1e-6f) return false;

            Vector2 newPosition = position + move * delta;

            // ✅ SIMPLIFICADO: Crear hitbox temporal en la nueva posición
            Rectangle newHitbox = CreateHitboxAt(newPosition);

            // Probar movimiento completo
            if (!tileMap.IsColliding(new Vector2(newHitbox.X, newHitbox.Y), newHitbox.Width, newHitbox.Height))
            {
                position = newPosition;
                UpdateFlip(move);
                return true;
            }

            // Probar solo movimiento X
            Vector2 xOnlyPosition = position + new Vector2(move.X, 0) * delta;
            Rectangle xOnlyHitbox = CreateHitboxAt(xOnlyPosition);
            if (!tileMap.IsColliding(new Vector2(xOnlyHitbox.X, xOnlyHitbox.Y), xOnlyHitbox.Width, xOnlyHitbox.Height))
            {
                position = xOnlyPosition;
                UpdateFlip(new Vector2(move.X, 0));
                return true;
            }

            // Probar solo movimiento Y
            Vector2 yOnlyPosition = position + new Vector2(0, move.Y) * delta;
            Rectangle yOnlyHitbox = CreateHitboxAt(yOnlyPosition);
            if (!tileMap.IsColliding(new Vector2(yOnlyHitbox.X, yOnlyHitbox.Y), yOnlyHitbox.Width, yOnlyHitbox.Height))
            {
                position = yOnlyPosition;
                UpdateFlip(new Vector2(0, move.Y));
                return true;
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

            // Ajustar la hitbox a partir del pie (bottom center) del sprite
            int x = (int)(pos.X - width / 2f);
            int y = (int)(pos.Y - height);  // Desde los pies hacia arriba

            return new Rectangle(x, y, width, height);
        }
    }
}