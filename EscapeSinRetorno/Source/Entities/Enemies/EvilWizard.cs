using EscapeSinRetorno.Source.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public class EvilWizard : Enemy
    {
        private enum State { Idle, Run, Attack, Death }
        private State currentState = State.Idle;

        private float speed = 50f;
        private float attackCooldown = 3.5f;
        private float attackDuration = 0.8f;

        private float attackTimer = 0f;
        private float attackTimeElapsed = 0f;

        private string activeAttack = "Attack1";

        private int maxHealth = 100;
        private int health = 100;

        private bool deathPlayed = false;
        private readonly Dictionary<string, int[]> _attackHitFrames = new()
        {
            { "Attack1", new[] { 5 } }, // ejemplo, ajusta según sprite
            { "Attack2", new[] { 5 } }
        };
        private bool _hasHitPlayerThisAttack = false;

        public EvilWizard(Vector2 startPosition) : base(startPosition) { }

        public override void LoadContent(ContentManager content)
        {
            string basePath = "Characters/EvilWizard/";

            animations["Idle"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Idle"), FrameWidth = 250, FrameHeight = 250 };
            animations["Run"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Run"), FrameWidth = 250, FrameHeight = 250 };
            animations["Attack1"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Attack1"), FrameWidth = 250, FrameHeight = 250 };
            animations["Attack2"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Attack2"), FrameWidth = 250, FrameHeight = 250 };
            animations["Death"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Death"), FrameWidth = 250, FrameHeight = 250 };
            animations["Take_hit"] = new AnimationClip { Texture = content.Load<Texture2D>($"{basePath}Take_hit"), FrameWidth = 250, FrameHeight = 250 };

            PlayAnimation("Idle");

            hitboxWidth = (int)(animations["Idle"].FrameWidth * 0.25f);
            hitboxHeight = (int)(animations["Idle"].FrameHeight * 0.25f);
        }

        public override void Update(GameTime gameTime, Player player, TileMap tileMap)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 toPlayer = player.Center - Center;

            attackTimer -= delta;

            if (health <= 0)
            {
                if (!deathPlayed)
                {
                    PlayAnimation("Death");
                    deathPlayed = true;
                }
                UpdateAnimation(gameTime);
                return;
            }

            switch (currentState)
            {
                case State.Attack:
                    attackTimeElapsed += delta;

                    // Iniciar ataque (se llama solo al inicio)
                    if (attackTimeElapsed <= delta)
                    {
                        activeAttack = (attackTimer % 2f < 1f) ? "Attack1" : "Attack2";
                        PlayAnimation(activeAttack);
                        _hasHitPlayerThisAttack = false; // resetear para este nuevo ataque
                    }

                    // --- Detectar golpe en el frame configurado ---
                    if (_attackHitFrames.TryGetValue(currentAnimation, out var hitFrames) &&
                        System.Array.Exists(hitFrames, f => f == currentFrame))
                    {
                        if (!_hasHitPlayerThisAttack && GetHitbox().Intersects(player.GetHitbox()))
                        {
                            player.TakeDamage(100); // daño del enemigo (ajusta si quieres)
                            _hasHitPlayerThisAttack = true;
                        }
                    }

                    // Terminar ataque
                    if (attackTimeElapsed >= attackDuration)
                    {
                        attackTimeElapsed = 0f;
                        attackTimer = attackCooldown;
                        currentState = State.Run;
                        _hasHitPlayerThisAttack = false; // asegurarse de resetear al finalizar
                    }
                    break;

                case State.Run:
                default:
                    if (IsCollidingWith(player.GetHitbox()))
                    {
                        currentState = State.Attack;
                    }

                    Vector2 dir = toPlayer;
                    if (dir.LengthSquared() > 1e-2f)
                        dir.Normalize();

                    bool moved = TryMoveToward(dir * speed, delta, tileMap);
                    PlayAnimation(moved ? "Run" : "Idle");
                    break;
            }

            UpdateAnimation(gameTime);
        }


        public override void TakeDamage(int dmg)
        {
            if (health <= 0) return;
            health -= dmg;
            PlayAnimation("Take_hit");
        }
    }
}
