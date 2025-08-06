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

            Vector2 origin = new Vector2(clip.FrameWidth / 2f, clip.FrameHeight);
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

            spriteBatch.Draw(clip.Texture, new Rectangle((int)(position.X - clip.FrameWidth / 2), (int)(position.Y - clip.FrameHeight), clip.FrameWidth, clip.FrameHeight), Color.Green * 0.2f);
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
            if (!animations.TryGetValue(currentAnimation, out var clip))
                return new Rectangle((int)position.X, (int)position.Y, 1, 1);

            // Asegurar valores por defecto si no están inicializados
            int width = hitboxWidth > 0 ? hitboxWidth : (int)(clip.FrameWidth * 0.4f);
            int height = hitboxHeight > 0 ? hitboxHeight : (int)(clip.FrameHeight * 0.6f);

            // Centrar la hitbox en el sprite
            int spriteBottom = (int)position.Y;
            int spriteTop = spriteBottom - clip.FrameHeight;
            int spriteCenterY = (spriteTop + spriteBottom) / 2;

            int x = (int)(position.X - width / 2f);
            int y = spriteCenterY - height / 2;

            return new Rectangle(x, y, width, height);
        }


        protected bool TryMoveToward(Vector2 move, float delta, TileMap tileMap)
        {
            if (move.LengthSquared() < 1e-6f) return false; // Sin movimiento

            Vector2 newPosition = position + move * delta;

            // ✅ CRUCIAL: Usar GetHitbox() para obtener la posición y tamaño correctos
            var currentHitbox = GetHitbox();

            // Calcular donde estaría la nueva hitbox
            Vector2 hitboxOffset = new Vector2(currentHitbox.X - position.X, currentHitbox.Y - position.Y);
            Vector2 newHitboxPosition = newPosition + hitboxOffset;

            // Probar movimiento completo
            if (!tileMap.IsColliding(newHitboxPosition, currentHitbox.Width, currentHitbox.Height))
            {
                position = newPosition;
                UpdateFlip(move);
                return true;
            }

            // Probar solo movimiento X
            Vector2 xOnlyPosition = position + new Vector2(move.X, 0) * delta;
            Vector2 xOnlyHitboxPos = xOnlyPosition + hitboxOffset;
            if (!tileMap.IsColliding(xOnlyHitboxPos, currentHitbox.Width, currentHitbox.Height))
            {
                position = xOnlyPosition;
                UpdateFlip(move);
                return true;
            }

            // Probar solo movimiento Y
            Vector2 yOnlyPosition = position + new Vector2(0, move.Y) * delta;
            Vector2 yOnlyHitboxPos = yOnlyPosition + hitboxOffset;
            if (!tileMap.IsColliding(yOnlyHitboxPos, currentHitbox.Width, currentHitbox.Height))
            {
                position = yOnlyPosition;
                UpdateFlip(move);
                return true;
            }

            return false; // No pudo moverse
        }

        private void UpdateFlip(Vector2 direction)
        {
            if (direction.X > 0.1f) flip = SpriteEffects.None;
            else if (direction.X < -0.1f) flip = SpriteEffects.FlipHorizontally;
        }
    }
}