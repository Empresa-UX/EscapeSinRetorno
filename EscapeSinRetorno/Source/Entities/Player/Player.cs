// File: Source/Entities/Player.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSinRetorno.Source.World;
using System.Collections.Generic;
using System;
using EscapeSinRetorno.Source.Entities.Enemies;
using EscapeSinRetorno.Source.Systems.Stats;
using EscapeSinRetorno.Source.Inventory;

namespace EscapeSinRetorno.Source.Entities
{
    public class Player
    {
        private readonly Dictionary<string, Texture2D> _animations = new();
        private readonly Queue<string> _attackCombo = new();
        private string _currentAnim = "Idle";
        private int _currentFrame;
        private double _timer, _interval = 120;
        private Vector2 _position, _velocity;
        private float _speed = 100f, _runMultiplier = 1.8f, _scale = 0.5f;
        private bool _isAttacking, _isJumping, _isRunning, _animLocked, _wasMoving, _inputBlocked;
        private KeyboardState _previousKeyboardState;
        private SpriteEffects _flip = SpriteEffects.None;
        private Texture2D _debugPixel;
        private readonly int _frameWidth = 128, _frameHeight = 128, _hitboxWidth = 64, _hitboxHeight = 64;
        private readonly HashSet<Enemy> _hitEnemies = new();
        private EnemyManager _enemyManager;

        private bool _staminaExhausted;
        private float _staminaRecoverDelay = 0.50f;
        private float _staminaRecoverTimer;

        public PlayerInventory Inventory { get; set; }

        private bool _freezeOnLastFrameWhenDead;

        public PlayerStats Stats { get; private set; } = new PlayerStats(new StatsConfig());
        private VisualEffectState _lastFx;

        public int Width => (int)(_hitboxWidth * _scale);
        public int Height => (int)(_hitboxHeight * _scale);
        public Vector2 Position => _position;
        public void SetPosition(Vector2 pos) => _position = pos;
        public Vector2 HitboxPosition => new(_position.X + (_frameWidth * _scale - Width) / 2, _position.Y + (_frameHeight * _scale - Height));
        public Rectangle GetHitbox() => new((int)HitboxPosition.X, (int)HitboxPosition.Y, Width, Height);

        // Centro más lógico del sprite
        public Vector2 Center => new(
            _position.X + _frameWidth * _scale / 2f,
            _position.Y + _frameHeight * _scale / 2f);

        public void SetEnemyManager(EnemyManager enemyManager) { _enemyManager = enemyManager; }
        public VisualEffectState CurrentFx => _lastFx;

        private readonly Dictionary<string, int[]> _attackHitFrames = new()
        {
            { "Attack_1", new[] { 3 } },
            { "Attack_2", new[] { 2 } },
            { "Attack_3", new[] { 3 } },
            { "Attack_4", new[] { 4 } }
        };

        public Player()
        {
            Stats.OnDeath += () =>
            {
                _inputBlocked = true;
                _animLocked = true;

                if (_animations.ContainsKey("Death"))
                {
                    PlayAnimation("Death", true);
                    _freezeOnLastFrameWhenDead = false;
                }
                else
                {
                    _freezeOnLastFrameWhenDead = true;
                }
            };
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            foreach (var anim in new[]
                     { "Idle", "Walk", "Hurt", "Run", "Jump",
                       "Attack_1", "Attack_2", "Attack_3", "Attack_4", "Death" })
            {
                try
                {
                    _animations[anim] = content.Load<Texture2D>($"Characters/Enchantress/{anim}");
                }
                catch
                {
                    if (anim != "Death") throw;
                }
            }

            _position = new Vector2(300, 300);
            _debugPixel = new Texture2D(graphicsDevice, 1, 1);
            _debugPixel.SetData(new[] { Color.White });
        }

        // Helper para reconciliación segura (cliente MP)
        public bool IsPositionFree(TileMap map, Vector2 worldPos)
        {
            var hbPos = new Vector2(
                worldPos.X + (_frameWidth * _scale - Width) / 2,
                worldPos.Y + (_frameHeight * _scale - Height)
            );
            return !map.IsColliding(hbPos, Width, Height);
        }

        // Overload viejo para estados que no usan puertas
        public void Update(GameTime gameTime, TileMap tileMap)
        {
            Update(gameTime, tileMap, null);
        }

        public void Update(GameTime gameTime, TileMap tileMap, DoorManager doorManager)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var ks = Keyboard.GetState();
            bool JustPressed(Keys k) => ks.IsKeyDown(k) && !_previousKeyboardState.IsKeyDown(k);

            if (Stats.IsDead || _inputBlocked)
            {
                Stats.Tick(dt, false, out _lastFx);
                Animate(gameTime);
                _previousKeyboardState = ks;
                return;
            }

            Vector2 input = Vector2.Zero;
            if (!_animLocked)
            {
                if (JustPressed(Keys.C)) TriggerComboAttack();
                if ((_isJumping = JustPressed(Keys.Z))) PlayAnimation("Jump", true);
            }

            if (_animLocked)
            {
                Stats.Tick(dt, false, out _lastFx);
                Animate(gameTime);
                _previousKeyboardState = ks;
                return;
            }

            if (ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D)) input.X++;
            if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A)) input.X--;
            if (ks.IsKeyDown(Keys.Up) || ks.IsKeyDown(Keys.W)) input.Y--;
            if (ks.IsKeyDown(Keys.Down) || ks.IsKeyDown(Keys.S)) input.Y++;

            bool isMoving = input != Vector2.Zero;

            if (Stats.Stamina.IsZero)
            {
                _staminaExhausted = true;
                _staminaRecoverTimer = _staminaRecoverDelay;
            }
            else if (_staminaExhausted)
            {
                _staminaRecoverTimer -= dt;
                if (_staminaRecoverTimer <= 0f && Stats.Stamina.Ratio >= 0.15f)
                    _staminaExhausted = false;
            }

            bool wantsRun = ks.IsKeyDown(Keys.X);
            bool effectiveRun = wantsRun && !_staminaExhausted && isMoving;

            Stats.Tick(dt, isSprinting: effectiveRun, out _lastFx);
            if (Stats.IsDead)
            {
                Animate(gameTime);
                _previousKeyboardState = ks;
                return;
            }

            // Combinar colisión de mapa + puertas cerradas
            bool IsBlocked(Vector2 hbPos, int w, int h)
            {
                bool blockedTiles = tileMap.IsColliding(hbPos, w, h);
                if (!blockedTiles && doorManager != null)
                {
                    var rect = new Rectangle((int)hbPos.X, (int)hbPos.Y, w, h);
                    if (doorManager.IsCollidingClosedDoor(rect))
                        return true;
                }
                return blockedTiles;
            }

            if (isMoving)
            {
                input.Normalize();
                float speedMul = effectiveRun ? _runMultiplier : 1f;
                _velocity = input * _speed * speedMul * dt;

                var hbPosX = HitboxPosition + new Vector2(_velocity.X, 0);
                if (!IsBlocked(hbPosX, Width, Height))
                    _position.X += _velocity.X;

                var hbPosY = HitboxPosition + new Vector2(0, _velocity.Y);
                if (!IsBlocked(hbPosY, Width, Height))
                    _position.Y += _velocity.Y;

                if (_velocity.X != 0)
                    _flip = _velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                PlayAnimation(effectiveRun ? "Run" : "Walk");
            }
            else if (_wasMoving && !_isAttacking && !_isJumping)
            {
                PlayAnimation("Idle");
            }

            _isRunning = effectiveRun;
            _wasMoving = isMoving;

            Animate(gameTime);
            _previousKeyboardState = ks;
        }

        private void TriggerComboAttack()
        {
            if (_isAttacking) return;
            foreach (var atk in new[] { "Attack_1", "Attack_2", "Attack_3", "Attack_4" })
                _attackCombo.Enqueue(atk);
            StartNextAttack();
        }

        private void StartNextAttack()
        {
            _hitEnemies.Clear();
            if (_attackCombo.Count == 0)
            {
                _isAttacking = _animLocked = false;
                PlayAnimation("Idle");
                return;
            }
            PlayAnimation(_attackCombo.Dequeue(), true);
            _isAttacking = true;
        }

        private void PlayAnimation(string anim, bool lockAnim = false)
        {
            if (_currentAnim == anim && !lockAnim) return;
            if (!_animations.ContainsKey(anim)) return;
            _currentAnim = anim;
            _currentFrame = 0;
            _timer = 0;
            _animLocked = lockAnim;
        }

        private void Animate(GameTime gameTime)
        {
            if (!_animations.TryGetValue(_currentAnim, out var tex)) return;
            int frameCount = tex.Width / _frameWidth;
            if (frameCount <= 0) return;

            _timer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_timer > _interval)
            {
                _currentFrame++;
                _timer = 0;

                if (_currentFrame >= frameCount)
                {
                    if (_currentAnim == "Death" || (_freezeOnLastFrameWhenDead && Stats.IsDead))
                    {
                        _currentFrame = frameCount - 1;
                        _animLocked = true;
                        return;
                    }

                    if (_currentAnim.StartsWith("Attack_"))
                        StartNextAttack();
                    else if (_currentAnim == "Jump")
                    {
                        _isJumping = false;
                        _animLocked = false;
                        PlayAnimation("Idle");
                    }
                    else if (_currentAnim is "Run" or "Walk")
                    {
                        _currentFrame = 0;
                    }
                    else
                    {
                        _currentFrame = 0;
                        _isAttacking = false;
                        _animLocked = false;
                        PlayAnimation("Idle");
                    }
                }
            }
        }

        private bool IsHitFrame()
        {
            return _isAttacking &&
                   _attackHitFrames.TryGetValue(_currentAnim, out var hitFrames) &&
                   Array.Exists(hitFrames, f => f == _currentFrame);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_animations.TryGetValue(_currentAnim, out var tex)) return;
            int clampedFrame = Math.Clamp(_currentFrame, 0, tex.Width / _frameWidth - 1);
            var source = new Rectangle(clampedFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            spriteBatch.Draw(tex, _position, source, Color.White, 0f, Vector2.Zero, _scale, _flip, 0f);

            // Debug hitbox opcional:
            // spriteBatch.Draw(_debugPixel, GetHitbox(), Color.Red * 0.3f);
        }

        private Rectangle GetAttackHitbox()
        {
            int range = 60;
            int height = Height;
            if (_flip == SpriteEffects.None)
                return new Rectangle((int)(HitboxPosition.X + Width), (int)HitboxPosition.Y, range, height);
            else
                return new Rectangle((int)(HitboxPosition.X - range), (int)HitboxPosition.Y, range, height);
        }

        public void TakeDamage(int dmg)
        {
            if (Stats.IsDead) return;
            Stats.ApplyDamage(new DamageRequest(dmg, DamageType.Physical, false));
            if (!Stats.IsDead) PlayAnimation("Hurt", true);
        }
    }
}
