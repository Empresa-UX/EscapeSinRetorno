// File: Source/Entities/Enemies/EvilWizard.cs
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public class EvilWizard : Enemy
    {
        private enum State { Idle, Run, Attack }
        private State currentState = State.Run;

        private float speed = 50f;
        private float runDuration = 10.0f;
        private float idleDuration = 2.0f;
        private float attackRange = 20f;
        private float attackCooldown = 3.5f;

        private float runTimer = 0f;
        private float idleTimer = 0f;
        private float attackTimer = 0f;

        public EvilWizard(Vector2 startPosition) : base(startPosition) { }

        public override void LoadContent(ContentManager content)
        {
            string basePath = "Characters/EvilWizard/";

            animations["Idle"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Idle"), FrameWidth = 250, FrameHeight = 250 };
            animations["Run"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Run"), FrameWidth = 250, FrameHeight = 250 };
            animations["Attack1"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Attack1"), FrameWidth = 250, FrameHeight = 250 };
            animations["Attack2"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Attack2"), FrameWidth = 250, FrameHeight = 250 };
            animations["Jump"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Jump"), FrameWidth = 250, FrameHeight = 250 };
            animations["Fall"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Fall"), FrameWidth = 250, FrameHeight = 250 };
            animations["Death"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Death"), FrameWidth = 250, FrameHeight = 250 };
            animations["Take_hit"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Take_hit"), FrameWidth = 250, FrameHeight = 250 };

            PlayAnimation("Idle");
        }

        public override void Update(GameTime gameTime, Vector2 playerPosition)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 toPlayer = playerPosition - Center;
            float distance = toPlayer.Length();

            attackTimer -= delta;

            switch (currentState)
            {
                case State.Idle:
                    idleTimer += delta;
                    PlayAnimation("Idle");

                    if (idleTimer >= idleDuration)
                    {
                        idleTimer = 0f;
                        currentState = State.Run;
                        runTimer = 0f;
                    }
                    break;

                case State.Run:
                    runTimer += delta;

                    if (distance <= attackRange && attackTimer <= 0f)
                    {
                        currentState = State.Attack;
                        break;
                    }

                    if (distance > 5f)
                    {
                        toPlayer.Normalize();
                        position += toPlayer * speed * delta;
                    }

                    PlayAnimation("Run");

                    if (runTimer >= runDuration)
                    {
                        currentState = State.Idle;
                        idleTimer = 0f;
                    }
                    break;

                case State.Attack:
                    PlayAnimation((attackTimer % 2f < 1f) ? "Attack1" : "Attack2");
                    attackTimer = attackCooldown;
                    currentState = State.Idle;
                    idleTimer = 0f;
                    break;
            }

            UpdateAnimation(gameTime);
        }
    }
}
