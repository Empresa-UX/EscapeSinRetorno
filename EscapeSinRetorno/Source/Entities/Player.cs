using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSinRetorno.Source.World;
using System.Collections.Generic;
using System;
using EscapeSinRetorno.Source.Entities.Enemies;

namespace EscapeSinRetorno.Source.Entities
{
    public class Player
    {
        private Dictionary<string, Texture2D> _animations = new();
        private Queue<string> _attackCombo = new();
        private string _currentAnim = "Idle";
        private int _currentFrame;
        private double _timer, _interval = 120;
        private Vector2 _position, _velocity;
        private float _speed = 100f, _runMultiplier = 1.8f, _scale = 0.5f;
        private bool _isAttacking, _isJumping, _isRunning, _animLocked, _wasMoving;
        private KeyboardState _previousKeyboardState;
        private SpriteEffects _flip = SpriteEffects.None;
        private Texture2D _debugPixel;
        private readonly int _frameWidth = 128, _frameHeight = 128, _hitboxWidth = 64, _hitboxHeight = 64;
        private HashSet<Enemy> _hitEnemies = new();
        private int _attackDamage = 20;
        private EnemyManager _enemyManager;


        public int Width => (int)(_hitboxWidth * _scale);
        public int Height => (int)(_hitboxHeight * _scale);
        public Vector2 Position => _position;
        public void SetPosition(Vector2 pos) => _position = pos;
        public Vector2 HitboxPosition => new(_position.X + (_frameWidth * _scale - Width) / 2, _position.Y + (_frameHeight * _scale - Height));
        public Rectangle GetHitbox() => new((int)HitboxPosition.X, (int)HitboxPosition.Y, Width, Height);
        public Vector2 Center => new(_position.X, _position.Y - _frameHeight / 2f);
        public void SetEnemyManager(EnemyManager enemyManager) {_enemyManager = enemyManager; }

        private readonly Dictionary<string, int[]> _attackHitFrames = new()
        {
            { "Attack_1", new[] { 3 } }, // frame donde pega
            { "Attack_2", new[] { 2 } },
            { "Attack_3", new[] { 3 } },
            { "Attack_4", new[] { 4 } }
        };

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            foreach (var anim in new[]
            {
                "Idle", "Walk", "Hurt", "Run", "Jump",
                "Attack_1", "Attack_2", "Attack_3", "Attack_4"
            })
             _animations[anim] = content.Load<Texture2D>($"Characters/Enchantress/{anim}");

            _position = new Vector2(300, 300);
            _debugPixel = new Texture2D(graphicsDevice, 1, 1);
            _debugPixel.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime, TileMap tileMap)
        {
            var ks = Keyboard.GetState();
            Vector2 input = Vector2.Zero;
            bool JustPressed(Keys key) => ks.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);

            if (!_animLocked)
            {
                if (JustPressed(Keys.C)) TriggerComboAttack();
                if ((_isJumping = JustPressed(Keys.Z))) PlayAnimation("Jump", true);
            }

            if (_animLocked) { Animate(gameTime); _previousKeyboardState = ks; return; }

            // Input handling
            if (ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D)) input.X++;
            if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A)) input.X--;
            if (ks.IsKeyDown(Keys.Up) || ks.IsKeyDown(Keys.W)) input.Y--;
            if (ks.IsKeyDown(Keys.Down) || ks.IsKeyDown(Keys.S)) input.Y++;

            _isRunning = ks.IsKeyDown(Keys.X);
            bool isMoving = input != Vector2.Zero;
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (isMoving)
            {
                input.Normalize();
                _velocity = input * _speed * (_isRunning ? _runMultiplier : 1f) * delta;

                if (!tileMap.IsColliding(HitboxPosition + new Vector2(_velocity.X, 0), Width, Height)) _position.X += _velocity.X;
                if (!tileMap.IsColliding(HitboxPosition + new Vector2(0, _velocity.Y), Width, Height)) _position.Y += _velocity.Y;

                if (_velocity.X != 0) _flip = _velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                PlayAnimation(_isRunning ? "Run" : "Walk");
            }
            else if (_wasMoving && !_isAttacking && !_isJumping)
            {
                PlayAnimation("Idle");
            }

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

            if (_attackCombo.Count == 0) { _isAttacking = _animLocked = false; PlayAnimation("Idle"); return; }
            PlayAnimation(_attackCombo.Dequeue(), true);
            _isAttacking = true;
        }

        private void PlayAnimation(string anim, bool lockAnim = false)
        {
            if (_currentAnim == anim && !lockAnim) return;
            if (!_animations.ContainsKey(anim)) return;

            (_currentAnim, _currentFrame, _timer, _animLocked) = (anim, 0, 0, lockAnim);
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

                // --- Si estamos en frame de golpe ---
                if (IsHitFrame() && _enemyManager != null)
                {
                    var attackHitbox = GetAttackHitbox();

                    foreach (var enemy in _enemyManager.GetEnemies())
                    {
                        if (enemy is MageGuardian) continue; // inmune
                        if (_hitEnemies.Contains(enemy)) continue;

                        if (attackHitbox.Intersects(enemy.GetHitbox()))
                        {
                            enemy.TakeDamage(_attackDamage);
                            _hitEnemies.Add(enemy);
                        }
                    }
                }

                if (_currentFrame >= frameCount)
                {
                    if (_currentAnim.StartsWith("Attack_"))
                        StartNextAttack();
                    else if (_currentAnim == "Jump")
                    {
                        _isJumping = false;
                        _animLocked = false;
                        PlayAnimation("Idle");
                    }
                    else if (_currentAnim is "Run" or "Walk")
                        _currentFrame = 0;
                    else
                    {
                        _currentFrame = 0;
                        _isAttacking = _animLocked = false;
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
            spriteBatch.Draw(_debugPixel, new Rectangle((int)HitboxPosition.X, (int)HitboxPosition.Y, Width, Height), Color.Red * 0.3f);
        }

        private Rectangle GetAttackHitbox()
        {
            int range = 60; // ajusta según alcance
            int height = Height;

            if (_flip == SpriteEffects.None) // mirando a la derecha
            {
                return new Rectangle(
                    (int)(HitboxPosition.X + Width),
                    (int)HitboxPosition.Y,
                    range,
                    height
                );
            }
            else // mirando a la izquierda
            {
                return new Rectangle(
                    (int)(HitboxPosition.X - range),
                    (int)HitboxPosition.Y,
                    range,
                    height
                );
            }
        }


        private int _health = 10000;

        public void TakeDamage(int dmg)
        {
            if (_health <= 0) return; // ya muerto
            _health -= dmg;

            PlayAnimation("Hurt", true);
        }

        private bool IsAttackingFrame()
        {
            return _isAttacking && _currentAnim.StartsWith("Attack_");
        }
    }
}