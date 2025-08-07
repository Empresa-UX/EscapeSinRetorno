using EscapeSinRetorno.Source.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public class NightBorne : Enemy
    {
        private enum State { Idle, Run, Attack, Death }
        private State currentState = State.Idle;

        private float speed = 60f;
        private float attackDuration = 0.8f;
        private float attackCooldown = 3.5f;

        private float attackTimer = 0f;
        private float attackTimeElapsed = 0f;

        private Vector2 velocity = Vector2.Zero;
        private float detectionRange = 160f;
        private float attackRange = 20f;

        private int maxHealth = 100;
        private int health = 100;

        private bool deathAnim1Done = false;
        private bool deathAnim2Done = false;

        public NightBorne(Vector2 startPosition) : base(startPosition) { }

        public override void LoadContent(ContentManager content)
        {
            string basePath = "Characters/NightBorne/";

            animations["Attack"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Attack"),
                FrameWidth = 80,
                FrameHeight = 80,
                Offset = new Vector2(0, -10f)
            };
            animations["Death_1"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Death_1"), FrameWidth = 80, FrameHeight = 80 };
            animations["Death_2"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Death_2"), FrameWidth = 80, FrameHeight = 80 };
            animations["Hurt"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Hurt"), FrameWidth = 80, FrameHeight = 80 };
            animations["Idle"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Idle"), FrameWidth = 80, FrameHeight = 80 };
            animations["Run"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Run"), FrameWidth = 80, FrameHeight = 80 };

            currentAnimation = "Idle";
            hitboxWidth = (int)(animations["Idle"].FrameWidth * 0.25f);
            hitboxHeight = (int)(animations["Idle"].FrameHeight * 0.35f);
        }

        public override void Update(GameTime gameTime, Player player, TileMap tileMap)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 toPlayer = player.Position - Center;
            float distance = toPlayer.Length();

            attackTimer -= delta;

            if (health <= 0)
            {
                if (!deathAnim1Done)
                {
                    PlayAnimation("Death_1");
                    if (UpdateDeathAnimation(gameTime, "Death_1"))
                        deathAnim1Done = true;
                }
                else if (!deathAnim2Done)
                {
                    PlayAnimation("Death_2");
                    if (UpdateDeathAnimation(gameTime, "Death_2"))
                        deathAnim2Done = true;
                }
                return;
            }

            switch (currentState)
            {
                case State.Attack:
                    attackTimeElapsed += delta;
                    if (attackTimeElapsed <= delta)
                        PlayAnimation("Attack");

                    if (attackTimeElapsed >= attackDuration)
                    {
                        attackTimeElapsed = 0f;
                        attackTimer = attackCooldown;
                        currentState = State.Run;
                    }
                    break;

                case State.Run:
                default:
                    if (distance < attackRange || IsCollidingWith(player.GetHitbox()))
                    {
                        currentState = State.Attack;
                        break;
                    }

                    if (distance < detectionRange)
                    {
                        Vector2 dir = toPlayer;
                        if (dir.LengthSquared() > 1e-2f)
                            dir.Normalize();

                        TryMoveToward(dir * speed, delta, tileMap);
                        PlayAnimation("Run");
                    }
                    else
                    {
                        currentState = State.Idle;
                        velocity = Vector2.Zero;
                        PlayAnimation("Idle");
                    }
                    break;
            }

            UpdateAnimation(gameTime);
        }

        private bool UpdateDeathAnimation(GameTime gameTime, string anim)
        {
            if (!animations.TryGetValue(anim, out var clip)) return true;

            animationTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (animationTimer >= frameInterval)
            {
                currentFrame++;
                animationTimer = 0;
                if (currentFrame >= clip.TotalFrames)
                {
                    currentFrame = 0;
                    return true;
                }
            }
            return false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if ((health <= 0) && deathAnim2Done)
                return;

            base.Draw(spriteBatch);
        }

        public void TakeDamage(int dmg)
        {
            if (health <= 0) return;
            health -= dmg;
        }
    }
}