using EscapeSinRetorno.Source.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public class EvilWizard : Enemy
    {
        // --- Tunables ---
        private const float LosCheckInterval = 0.12f; // s entre raycasts
        private const float LosStep = 8f;             // px por paso de LOS
        private const float WaypointReach = 6f;       // px (para snaps cortos directos)
        private const float SlideSpeedMul = 0.92f;    // al deslizar
        private const float SweepSpeedMul = 0.85f;    // al barrer ángulos
        private const int MaxStuckFrames = 14;

        // --- Runtime ---
        private float _losTimer = 0f;
        private bool _hasLos = false;
        private Vector2 _lastPos;
        private int _stuckFrames = 0;

        public EvilWizard(Vector2 startPosition) : base(startPosition)
        {
            speed = 50f;
            health = 100;
        }

        public override void LoadContent(ContentManager content)
        {
            string basePath = "Characters/EvilWizard/";

            animations["Idle"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Idle"), FrameWidth = 250, FrameHeight = 250 };
            animations["Run"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Run"), FrameWidth = 250, FrameHeight = 250 };
            animations["Attack1"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Attack1"), FrameWidth = 250, FrameHeight = 250 };
            animations["Attack2"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Attack2"), FrameWidth = 250, FrameHeight = 250 };
            animations["Death"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Death"), FrameWidth = 250, FrameHeight = 250 };
            animations["Take_hit"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Take_hit"), FrameWidth = 250, FrameHeight = 250 };

            // Contacto únicamente en hitframes
            attacks["Attack1"] = new AttackDef
            {
                Damage = 12,
                Duration = 0.80f,
                Cooldown = 1.60f,
                HitFrames = new[] { 5, 6 },
                UseCollisionOnly = true,
                UseProximity = false,
                HitboxSizePx = new Point(80, 60),
                HitboxOffsetPx = new Point(70, -20)
            };
            attacks["Attack2"] = new AttackDef
            {
                Damage = 16,
                Duration = 0.95f,
                Cooldown = 2.00f,
                HitFrames = new[] { 7 },
                UseCollisionOnly = true,
                UseProximity = false,
                HitboxSizePx = new Point(90, 60),
                HitboxOffsetPx = new Point(80, -20)
            };

            PlayAnimation("Idle");
            hitboxWidth = (int)(animations["Idle"].FrameWidth * 0.25f);
            hitboxHeight = (int)(animations["Idle"].FrameHeight * 0.25f);

            _lastPos = position;
        }

        public override void Update(GameTime gameTime, Player player, TileMap tileMap)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (health <= 0)
            {
                TickDeath(gameTime, "Death");
                return;
            }

            attackCooldownTimer -= dt;

            // Centros por HURTBOX para decisiones simétricas
            Rectangle eHB = GetHitbox();
            Rectangle pHB = player.GetHitbox();
            Vector2 selfC = new Vector2(eHB.X + eHB.Width * 0.5f, eHB.Y + eHB.Height * 0.5f);
            Vector2 targetC = new Vector2(pHB.X + pHB.Width * 0.5f, pHB.Y + pHB.Height * 0.5f);

            Vector2 toPlayer = targetC - selfC;
            float dist = toPlayer.Length();

            if (currentState != State.Attack)
            {
                bool touching = eHB.Intersects(pHB);

                if (touching)
                {
                    string next = (currentAnimation == "Attack1") ? "Attack2" : "Attack1";
                    TryStartAttack(next);
                    if (currentState != State.Attack) PlayAnimation("Idle");
                }
                else
                {
                    // === FOLLOW INTELIGENTE (LOS + SLIDE) ===
                    _losTimer -= dt;
                    if (_losTimer <= 0f)
                    {
                        _hasLos = HasLineOfSight(tileMap, selfC, targetC, eHB.Width, eHB.Height);
                        _losTimer = LosCheckInterval;
                    }

                    if (toPlayer.LengthSquared() > WaypointReach * WaypointReach)
                    {
                        Vector2 dir = toPlayer / Math.Max(toPlayer.Length(), 1e-5f);

                        bool moved = false;
                        if (_hasLos)
                        {
                            moved = SmartMove(tileMap, dir, dt, allowSweep: false); // directo si hay LOS
                        }
                        if (!moved)
                        {
                            moved = SmartMove(tileMap, dir, dt, allowSweep: true);  // slide + barrido
                        }

                        if (!moved)
                        {
                            // micro-jitter para despegar
                            var jitter = new Vector2(
                                (float)(Game1.Random.NextDouble() - 0.5),
                                (float)(Game1.Random.NextDouble() - 0.5));
                            if (jitter.LengthSquared() > 1e-4f)
                                TryMoveToward(Vector2.Normalize(jitter) * (speed * 0.6f), dt, tileMap);
                            _stuckFrames++;
                        }
                        else
                        {
                            // Reseteo si realmente avanzamos
                            if (Vector2.DistanceSquared(_lastPos, position) > 0.5f)
                                _stuckFrames = Math.Max(0, _stuckFrames - 1);
                        }
                    }

                    PlayAnimation("Run");
                }

                if (toPlayer.X > 0) flip = SpriteEffects.None; else if (toPlayer.X < 0) flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                // Daño por contacto solo en los hitframes
                ProcessAttackFrame(dt, player);
            }

            _lastPos = position;
            UpdateAnimation(gameTime);
        }

        // --- Movimiento con slide + barrido opcional ---
        private bool SmartMove(TileMap map, Vector2 dir, float dt, bool allowSweep)
        {
            // 1) intento directo
            if (TryMoveToward(dir * speed, dt, map)) return true;

            // 2) slide por ejes
            bool moved = false;
            if (Math.Abs(dir.X) > 1e-4f)
                moved |= TryMoveToward(new Vector2(dir.X, 0f) * (speed * SlideSpeedMul), dt, map);
            if (!moved && Math.Abs(dir.Y) > 1e-4f)
                moved |= TryMoveToward(new Vector2(0f, dir.Y) * (speed * SlideSpeedMul), dt, map);
            if (moved) return true;

            if (!allowSweep) return false;

            // 3) barrido angular (±30/±60/±90)
            float[] angles = { 30f, -30f, 60f, -60f, 90f, -90f, 120f, -120f };
            foreach (var a in angles)
            {
                float rad = MathHelper.ToRadians(a);
                var rdir = new Vector2(
                    dir.X * MathF.Cos(rad) - dir.Y * MathF.Sin(rad),
                    dir.X * MathF.Sin(rad) + dir.Y * MathF.Cos(rad)
                );
                if (TryMoveToward(rdir * (speed * SweepSpeedMul), dt, map))
                    return true;
            }

            // 4) si nada funcionó, seguimos bloqueados
            return false;
        }

        // --- Line of sight (raycast por pasos con hitbox real del enemigo) ---
        private bool HasLineOfSight(TileMap map, Vector2 fromCenter, Vector2 toCenter, int hbW, int hbH)
        {
            Vector2 delta = toCenter - fromCenter;
            float len = delta.Length();
            if (len < 1e-3f) return true;
            Vector2 step = delta / Math.Max(1f, (float)Math.Ceiling(len / LosStep));

            Vector2 probe = fromCenter;
            for (float t = 0; t < len; t += LosStep)
            {
                probe += step;
                // recrear hitbox del enemigo centrado en "probe"
                var hb = new Rectangle(
                    (int)(probe.X - hbW * 0.5f),
                    (int)(probe.Y - hbH),
                    hbW, hbH
                );
                if (map.IsColliding(new Vector2(hb.X, hb.Y), hb.Width, hb.Height))
                    return false;
            }
            return true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (health <= 0 && IsRemovable) return;
            base.Draw(spriteBatch);
        }

        public override void TakeDamage(int dmg)
        {
            if (health <= 0) return;
            health -= Math.Max(0, dmg);
            if (health > 0) PlayAnimation("Take_hit", true);
        }
    }
}