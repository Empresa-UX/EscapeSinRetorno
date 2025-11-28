using EscapeSinRetorno.Source.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public class NightBorne : Enemy
    {
        private bool death1Done = false, death2Done = false;

        // Rango muy corto para habilitar el ataque (medido con centros de hurtbox).
        // Ajustá si querés más/menos exigente (20f ≈ ~1.25 tiles si tus tiles son 16px).
        private const float AttackTriggerRange = 20f;
        public override string TypeId => "nightborne";

        public NightBorne(Vector2 startPosition) : base(startPosition)
        {
            speed = 60f;
            health = 100;
        }

        public override void LoadContent(ContentManager content)
        {
            string basePath = "Characters/NightBorne/";

            animations["Attack"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Attack"), FrameWidth = 80, FrameHeight = 80, Offset = new Vector2(0, -10f) };
            animations["Death_1"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Death_1"), FrameWidth = 80, FrameHeight = 80 };
            animations["Death_2"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Death_2"), FrameWidth = 80, FrameHeight = 80 };
            animations["Hurt"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Hurt"), FrameWidth = 80, FrameHeight = 80 };
            animations["Idle"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Idle"), FrameWidth = 80, FrameHeight = 80 };
            animations["Run"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Run"), FrameWidth = 80, FrameHeight = 80 };

            // Contacto únicamente en hitframes; la hitbox se amplía en Attack (ver override).
            attacks["Attack"] = new AttackDef
            {
                Damage = 10,
                Duration = 0f,        // usa duración real del clip
                Cooldown = 1.75f,
                HitFrames = new[] { 9 }, // si tu sheet tiene menos frames, se clamp en Enemy

                UseCollisionOnly = true,
                UseProximity = false,

                HitboxSizePx = new Point(40, 40),
                HitboxOffsetPx = new Point(32, -10)
            };

            currentAnimation = "Idle";
            hitboxWidth = (int)(animations["Idle"].FrameWidth * 0.40f);
            hitboxHeight = (int)(animations["Idle"].FrameHeight * 0.40f);
        }

        // --- HITBOX AMPLIADA DURANTE ATTACK (x2 en ambos ejes)
        public override Rectangle GetHitbox()
        {
            var rect = base.GetHitbox();
            if (currentState == State.Attack)
            {
                // inflate en X e Y por la mitad del tamaño actual → ancho/alto se duplican.
                int dx = rect.Width / 2;
                int dy = rect.Height / 2;
                rect.Inflate(dx, dy);
            }
            return rect;
        }

        public override void Update(GameTime gameTime, Player player, TileMap tileMap)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // ---- Muerte (igual que antes)
            if (health <= 0)
            {
                if (!death1Done)
                {
                    PlayAnimation("Death_1");
                    UpdateAnimation(gameTime);
                    if (currentFrame >= animations["Death_1"].TotalFrames - 1)
                    { death1Done = true; PlayAnimation("Death_2", restart: true); }
                    return;
                }
                if (!death2Done)
                {
                    PlayAnimation("Death_2");
                    UpdateAnimation(gameTime);
                    if (currentFrame >= animations["Death_2"].TotalFrames - 1)
                    { death2Done = true; IsRemovable = true; }
                }
                return;
            }

            attackCooldownTimer -= dt;

            // === SEGUIMIENTO / DECISIÓN DE ATAQUE ==============================
            // Usamos centros de HURTBOX (rectángulos) para evitar asimetrías visuales.
            var playerHB = player.GetHitbox();
            var enemyHB = GetHitbox(); // OJO: en Idle/Run es normal; en Attack ya viene ampliada.

            Vector2 pc = new Vector2(playerHB.X + playerHB.Width * 0.5f,
                                     playerHB.Y + playerHB.Height * 0.5f);
            Vector2 ec = new Vector2(enemyHB.X + enemyHB.Width * 0.5f,
                                     enemyHB.Y + enemyHB.Height * 0.5f);

            Vector2 toPlayer = pc - ec;
            float distanceHB = toPlayer.Length();

            if (currentState != State.Attack)
            {
                bool touching = enemyHB.Intersects(playerHB); // contacto real de hurtboxes

                // 1) Solo iniciar ataque si está tocando o MUY cerca (rango bajo real)
                if (touching || distanceHB <= AttackTriggerRange)
                {
                    TryStartAttack("Attack");
                    if (currentState != State.Attack) PlayAnimation("Idle");
                }
                else
                {
                    // 2) Si no, perseguir si está relativamente cerca (detección)
                    const float detectionRange = 180f;
                    if (distanceHB <= detectionRange)
                    {
                        var dir = toPlayer;
                        if (dir.LengthSquared() > 1e-3f) dir.Normalize();
                        TryMoveToward(dir * speed, dt, tileMap);
                        PlayAnimation("Run");
                    }
                    else
                    {
                        PlayAnimation("Idle");
                    }
                }

                // 3) Mirar hacia el jugador usando delta en X entre centros de hurtbox
                if (toPlayer.X > 0) flip = SpriteEffects.None;
                else if (toPlayer.X < 0) flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                // En Attack: daño SOLO en hitframes y SI hay contacto (con la hitbox ampliada).
                ProcessAttackFrame(dt, player);
            }

            UpdateAnimation(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (health <= 0 && death2Done) return;
            base.Draw(spriteBatch);
        }

        public override void TakeDamage(int dmg)
        {
            if (health <= 0) return;
            health -= (dmg < 0 ? 0 : dmg);
            if (health > 0) PlayAnimation("Hurt", true);
        }
    }
}