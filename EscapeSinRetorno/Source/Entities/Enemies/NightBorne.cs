using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public class NightBorne : Enemy
    {
        private enum State { Idle, Run, Attack, Death }
        private State currentState = State.Idle;

        private Vector2 velocity = Vector2.Zero;
        private float detectionRange = 160f;
        private float attackRange = 10f;

        private int maxHealth = 100;
        private int health = 100;

        private bool deathAnim1Done = false;
        private bool deathAnim2Done = false;

        public NightBorne(Vector2 startPosition) : base(startPosition) { }

        public override void LoadContent(ContentManager content)
        {
            Vector2 sharedOrigin = new Vector2(38.5f, 51f); // ideal para tus enemigos

            string basePath = "Characters/NightBorne/";

            animations["Attack"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Attack"),
                FrameWidth = 80,
                FrameHeight = 80,
                Offset = new Vector2(0, -10f) // 🔧 ajusta hacia arriba 10px
            };
            animations["Death_1"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Death_1"), FrameWidth = 80, FrameHeight = 80 };
            animations["Death_2"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Death_2"), FrameWidth = 80, FrameHeight = 80 };
            animations["Hurt"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Hurt"), FrameWidth = 80, FrameHeight = 80 };
            animations["Idle"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Idle"), FrameWidth = 80, FrameHeight = 80 };
            animations["Run"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Run"), FrameWidth = 80, FrameHeight = 80 };

            currentAnimation = "Idle";
        }

        public override void Update(GameTime gameTime, Vector2 playerPosition)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 toPlayer = playerPosition - Center;
            float distance = toPlayer.Length();

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

            if (distance < attackRange)
            {
                currentState = State.Attack;
                velocity = Vector2.Zero;
                PlayAnimation("Attack");
            }
            else if (distance < detectionRange)
            {
                currentState = State.Run;
                toPlayer.Normalize();
                velocity = toPlayer * 60f;
                position += velocity * delta;
                PlayAnimation("Run");
            }
            else
            {
                currentState = State.Idle;
                velocity = Vector2.Zero;
                PlayAnimation("Idle");
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
