using System;
using System.Collections.Generic;
using EscapeSinRetorno.Source.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public abstract class Enemy
    {
        // --- Animación / render
        protected readonly Dictionary<string, AnimationClip> animations = new();
        protected string currentAnimation = "Idle";
        protected int currentFrame = 0;
        protected double animationTimer = 0;
        protected double frameInterval = 100; // ms por frame
        protected SpriteEffects flip = SpriteEffects.None;
        protected static Texture2D debugPixel;

        // --- Colisiones y posición
        protected int hitboxWidth, hitboxHeight;
        protected Vector2 position, velocity;
        protected float speed = 50f;

        // --- Vida simple
        protected int health = 100;
        public bool IsDead => health <= 0;
        public bool IsRemovable { get; protected set; } = false;

        // --- Ataque
        protected enum State { Idle, Run, Attack, Hurt, Dying, Dead }
        protected State currentState = State.Idle;

        protected readonly Dictionary<string, AttackDef> attacks = new();
        protected string activeAttack;
        protected float attackElapsed = 0f;
        protected float attackCooldownTimer = 0f;
        protected bool hasHitThisAttack = false;

        // Facing congelado al iniciar el ataque (cono estable)
        private Vector2 _attackFacing = new(1, 0);
        // Duración efectiva del ataque (por anim si Duration<=0)
        private float _attackDurationSec = 0f;

        protected struct AttackDef
        {
            public int Damage;
            public float Duration;        // si <= 0, se usa duración real del clip
            public float Cooldown;
            public int[] HitFrames;       // índices base 0

            // Hurtbox rectangular (si se usa)
            public Point HitboxSizePx;
            public Point HitboxOffsetPx;

            // Variantes
            public bool UseProximity;      // radio + cono
            public int ProximityRadiusPx;  // px
            public float FrontConeDeg;     // 0..180 (180 = omni)
            public bool UseCollisionOnly;  // solo contacto de hitboxes
        }

        public Vector2 Position => position;
        public Vector2 Center =>
            animations.TryGetValue(currentAnimation, out var clip)
                ? new Vector2(position.X, position.Y - clip.FrameHeight / 2f)
                : position;

        protected Enemy(Vector2 startPosition) => position = startPosition;

        public abstract void LoadContent(ContentManager content);
        public abstract void Update(GameTime gameTime, Player player, TileMap tileMap);
        public abstract void TakeDamage(int dmg);

        public static void LoadDebugTexture(GraphicsDevice device)
        {
            debugPixel = new Texture2D(device, 1, 1);
            debugPixel.SetData(new[] { Color.White });
        }

        // --- Animación
        protected void UpdateAnimation(GameTime gameTime)
        {
            if (!animations.TryGetValue(currentAnimation, out var clip)) return;
            animationTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (animationTimer >= frameInterval)
            {
                currentFrame = (currentFrame + 1) % Math.Max(1, clip.TotalFrames);
                animationTimer = 0;
            }
        }

        protected void PlayAnimation(string name, bool restart = false)
        {
            if (!animations.ContainsKey(name)) return;
            if (restart || currentAnimation != name)
            {
                currentAnimation = name;
                currentFrame = 0;
                animationTimer = 0;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!animations.TryGetValue(currentAnimation, out var clip)) return;

            var source = new Rectangle(
                Math.Clamp(currentFrame, 0, clip.TotalFrames - 1) * clip.FrameWidth,
                0, clip.FrameWidth, clip.FrameHeight);

            var origin = new Vector2(clip.FrameWidth / 2f, clip.FrameHeight * 0.7f) + clip.Offset;

            spriteBatch.Draw(clip.Texture, position, source, Color.White, 0f, origin, 1f, flip, 0f);

            // Debug hitbox
            if (debugPixel != null) spriteBatch.Draw(debugPixel, GetHitbox(), Color.Red * 0.25f);
        }

        // --- Colisión
        public virtual Rectangle GetHitbox() => CreateHitboxAt(position);

        protected Rectangle CreateHitboxAt(Vector2 pos)
        {
            if (!animations.TryGetValue(currentAnimation, out var clip))
                return new Rectangle((int)pos.X, (int)pos.Y, 1, 1);

            int width = hitboxWidth > 0 ? hitboxWidth : (int)(clip.FrameWidth * 0.4f);
            int height = hitboxHeight > 0 ? hitboxHeight : (int)(clip.FrameHeight * 0.4f);
            return new Rectangle((int)(pos.X - width / 2f), (int)(pos.Y - height), width, height);
        }

        protected static Vector2 RectCenter(Rectangle r) => new(r.X + r.Width * 0.5f, r.Y + r.Height * 0.5f);

        protected bool IsCollidingWith(Rectangle other) => GetHitbox().Intersects(other);

        protected void FaceTowards(Vector2 direction)
        {
            if (direction.X > 0.1f) flip = SpriteEffects.None;
            else if (direction.X < -0.1f) flip = SpriteEffects.FlipHorizontally;
        }

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
                    var dir = pos - position;
                    position = pos;
                    FaceTowards(dir);
                    return true;
                }
            }
            return false;
        }

        // --- Ataques
        protected void TryStartAttack(string attackKey)
        {
            if (currentState == State.Attack) return;
            if (attackCooldownTimer > 0f) return;
            if (!attacks.ContainsKey(attackKey)) return;

            activeAttack = attackKey;
            attackElapsed = 0f;
            hasHitThisAttack = false;
            currentState = State.Attack;
            PlayAnimation(activeAttack, restart: true);

            // Congelar facing para el cono
            _attackFacing = (flip == SpriteEffects.None) ? new Vector2(1, 0) : new Vector2(-1, 0);

            // Duración real si no se definió
            _attackDurationSec = attacks[activeAttack].Duration;
            if (_attackDurationSec <= 0f && animations.TryGetValue(activeAttack, out var clip))
                _attackDurationSec = (float)(clip.TotalFrames * (frameInterval / 1000.0));
        }

        protected void ProcessAttackFrame(float dt, Player player)
        {
            attackElapsed += dt;
            if (!attacks.TryGetValue(activeAttack, out var def)) { EndAttack(); return; }

            // Hitframe clamp
            bool isHitFrame = false;
            if (def.HitFrames != null && animations.TryGetValue(activeAttack, out var aclip))
            {
                foreach (var f in def.HitFrames)
                {
                    int clamped = Math.Clamp(f, 0, Math.Max(0, aclip.TotalFrames - 1));
                    if (clamped == currentFrame) { isHitFrame = true; break; }
                }
            }

            if (!hasHitThisAttack && isHitFrame)
            {
                bool hit = false;

                if (def.UseProximity)
                {
                    // Usar centro de hurtbox del jugador para simetría
                    var pc = RectCenter(player.GetHitbox());
                    var ec = RectCenter(GetHitbox()); // centro del enemigo por hurtbox (evita origen visual)

                    if (Vector2.Distance(ec, pc) <= def.ProximityRadiusPx)
                    {
                        if (def.FrontConeDeg >= 179.9f) hit = true;
                        else
                        {
                            Vector2 toPlayer = Vector2.Normalize(pc - ec);
                            float cos = Vector2.Dot(_attackFacing, toPlayer);
                            float limitCos = MathF.Cos(MathHelper.ToRadians(def.FrontConeDeg * 0.5f));
                            hit = cos >= limitCos;
                        }
                    }

                    // Además, cuenta si está superpuesto
                    if (!hit) hit = IsCollidingWith(player.GetHitbox());
                }
                else if (def.UseCollisionOnly)
                {
                    hit = IsCollidingWith(player.GetHitbox());
                }
                else
                {
                    hit = BuildAttackRect(def).Intersects(player.GetHitbox());
                }

                if (hit)
                {
                    player.TakeDamage(def.Damage);
                    hasHitThisAttack = true;
                }
            }

            if (attackElapsed >= _attackDurationSec)
            {
                attackCooldownTimer = def.Cooldown;
                EndAttack();
            }
        }

        protected Rectangle BuildAttackRect(AttackDef def)
        {
            var c = Center;
            int offX = def.HitboxOffsetPx.X * (flip == SpriteEffects.FlipHorizontally ? -1 : 1);
            var center = new Vector2(c.X + offX, c.Y + def.HitboxOffsetPx.Y);
            return new Rectangle(
                (int)(center.X - def.HitboxSizePx.X / 2f),
                (int)(center.Y - def.HitboxSizePx.Y / 2f),
                def.HitboxSizePx.X,
                def.HitboxSizePx.Y
            );
        }

        protected void EndAttack()
        {
            hasHitThisAttack = false;
            attackElapsed = 0f;
            currentState = State.Run;
            PlayAnimation("Run");
        }

        protected void TickDeath(GameTime gameTime, string deathAnimName = "Death")
        {
            PlayAnimation(deathAnimName);
            UpdateAnimation(gameTime);
            if (animations.TryGetValue(deathAnimName, out var clip) && currentFrame >= clip.TotalFrames - 1)
            {
                currentState = State.Dead;
                IsRemovable = true;
            }
        }
    }
}