// File: Source/Entities/Enemies/EvilWizard.cs
using System;
using EscapeSinRetorno.Source.World;
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
        private float stopChaseDistance = 5f;   // Distancia mínima antes de parar

        private float attackCooldown = 3.5f;

        private float runTimer = 0f;
        private float idleTimer = 0f;
        private float attackTimer = 0f;

        private float attackDuration = 0.8f; // duración total del ataque
        private float attackTimeElapsed = 0f;
        private string activeAttack = "Attack1";


        public EvilWizard(Vector2 startPosition) : base(startPosition)
        {

        }

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

            hitboxWidth = (int)(animations["Idle"].FrameWidth * 0.25f);
            hitboxHeight = (int)(animations["Idle"].FrameHeight * 0.35f);
        }

        public override void Update(GameTime gameTime, Vector2 playerPosition, TileMap tileMap)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // ✅ CORREGIDO: Usar Center corregido para cálculo de distancia
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

                    // ✅ MEJORADO: Atacar cuando esté cerca
                    if (distance <= attackRange && attackTimer <= 0f)
                    {
                        currentState = State.Attack;
                        attackTimeElapsed = 0f;
                        break;
                    }

                    // ✅ CORREGIDO: Perseguir siempre, pero con distancia mínima
                    if (distance > stopChaseDistance)
                    {
                        Vector2 direction = toPlayer;
                        if (direction.LengthSquared() > 1e-2f)
                            direction.Normalize();

                        // ✅ CLAVE: Verificar si se pudo mover para evitar trabas
                        bool couldMove = TryMoveToward(direction * speed, delta, tileMap);

                        if (couldMove)
                        {
                            PlayAnimation("Run");
                            runTimer = 0f; // Reset timer si se está moviendo
                        }
                        else
                        {
                            // Si no puede moverse, intentar idle un momento
                            PlayAnimation("Idle");
                        }
                    }
                    else
                    {
                        // Muy cerca del jugador, esperar o atacar
                        PlayAnimation("Idle");
                    }

                    // ✅ OPCIONAL: Timeout solo si está lejos y no se mueve
                    if (runTimer >= runDuration && distance > attackRange * 2)
                    {
                        currentState = State.Idle;
                        idleTimer = 0f;
                    }
                    break;

                case State.Attack:
                    attackTimeElapsed += delta;

                    if (attackTimeElapsed <= delta) // primer frame del ataque
                    {
                        activeAttack = (attackTimer % 2f < 1f) ? "Attack1" : "Attack2";
                        PlayAnimation(activeAttack);
                    }

                    if (attackTimeElapsed >= attackDuration)
                    {
                        attackTimeElapsed = 0f;
                        attackTimer = attackCooldown;
                        currentState = State.Run; // ✅ Volver a correr inmediatamente
                        runTimer = 0f;
                    }
                    break;
            }

            UpdateAnimation(gameTime);
        }
    }
}
