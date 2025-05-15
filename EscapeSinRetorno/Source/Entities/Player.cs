// File: Source/Entities/Player.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeSinRetorno.Source.World;
using System;
using System.Collections.Generic;

namespace EscapeSinRetorno.Source.Entities
{
    public class Player
    {
        private Dictionary<string, Texture2D> _animations;
        private string _currentAnim = "Idle";
        private int _currentFrame;
        private double _timer, _interval = 120;

        private Vector2 _position;
        private Vector2 _velocity;
        private float _speed = 100f;
        private float _runMultiplier = 1.8f;

        private bool _isAttacking = false;
        private bool _isJumping = false;
        private bool _isRunning = false;

        private SpriteEffects _flip = SpriteEffects.None;
        private Texture2D _debugPixel;

        private int _frameWidth = 128;
        private int _frameHeight = 128;

        private int _hitboxWidth = 32;
        private int _hitboxHeight = 32;

        private KeyboardState _previousKeyboardState;
        private bool _animLocked = false;
        private bool _wasMoving = false;

        public int Width => _hitboxWidth;
        public int Height => _hitboxHeight;

        public Vector2 HitboxPosition => new Vector2(
            _position.X + (_frameWidth - _hitboxWidth) / 2,
            _position.Y + (_frameHeight - _hitboxHeight)
        );

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _animations = new Dictionary<string, Texture2D>
            {
                ["Idle"] = content.Load<Texture2D>("Characters/Enchantress/Idle"),
                ["Walk"] = content.Load<Texture2D>("Characters/Enchantress/Walk"),
                ["Run"] = content.Load<Texture2D>("Characters/Enchantress/Run"),
                ["Jump"] = content.Load<Texture2D>("Characters/Enchantress/Jump"),
                ["Attack_1"] = content.Load<Texture2D>("Characters/Enchantress/Attack_1"),
                ["Attack_2"] = content.Load<Texture2D>("Characters/Enchantress/Attack_2"),
                ["Attack_3"] = content.Load<Texture2D>("Characters/Enchantress/Attack_3"),
                ["Attack_4"] = content.Load<Texture2D>("Characters/Enchantress/Attack_4"),
            };

            _position = new Vector2(300, 300);

            _debugPixel = new Texture2D(graphicsDevice, 1, 1);
            _debugPixel.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime, TileMap tileMap)
        {
            KeyboardState ks = Keyboard.GetState();
            Vector2 input = Vector2.Zero;

            bool justPressed(Keys key) =>
                ks.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);

            if (!_animLocked)
            {
                if (justPressed(Keys.C)) TriggerComboAttack();
                _isJumping = justPressed(Keys.Z);
                if (_isJumping)
                {
                    StartAnimation("Jump");
                }
            }

            if (_animLocked)
            {
                Animate(gameTime);
                _previousKeyboardState = ks;
                return;
            }

            if (ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D)) input.X += 1;
            if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A)) input.X -= 1;
            if (ks.IsKeyDown(Keys.Up) || ks.IsKeyDown(Keys.W)) input.Y -= 1;
            if (ks.IsKeyDown(Keys.Down) || ks.IsKeyDown(Keys.S)) input.Y += 1;

            _isRunning = ks.IsKeyDown(Keys.X);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bool isMoving = input != Vector2.Zero;

            if (isMoving)
            {
                input.Normalize();
                float finalSpeed = _speed * (_isRunning ? _runMultiplier : 1f);
                _velocity = input * finalSpeed * deltaTime;

                Vector2 newPosX = new Vector2(HitboxPosition.X + _velocity.X, HitboxPosition.Y);
                if (!tileMap.IsColliding(newPosX, Width, Height))
                    _position.X += _velocity.X;

                Vector2 newPosY = new Vector2(HitboxPosition.X, HitboxPosition.Y + _velocity.Y);
                if (!tileMap.IsColliding(newPosY, Width, Height))
                    _position.Y += _velocity.Y;

                _flip = _velocity.X > 0 ? SpriteEffects.None :
                        _velocity.X < 0 ? SpriteEffects.FlipHorizontally : _flip;

                SetMovementAnimation(_isRunning ? "Run" : "Walk");
            }
            else if (_wasMoving && !_isAttacking && !_isJumping)
            {
                SetMovementAnimation("Idle");
            }

            _wasMoving = isMoving;
            Animate(gameTime);
            _previousKeyboardState = ks;
        }

        private void SetMovementAnimation(string anim)
        {
            if (_currentAnim != anim && !_animLocked)
            {
                _currentAnim = anim;
                _currentFrame = 0;
                _timer = 0;
            }
        }

        private Queue<string> _attackCombo = new();

        private void TriggerComboAttack()
        {
            if (_isAttacking) return;

            _attackCombo.Enqueue("Attack_1");
            _attackCombo.Enqueue("Attack_2");
            _attackCombo.Enqueue("Attack_3");
            _attackCombo.Enqueue("Attack_4");
            StartNextAttackInCombo();
        }

        private void StartNextAttackInCombo()
        {
            if (_attackCombo.Count == 0)
            {
                _isAttacking = false;
                _animLocked = false;
                SetMovementAnimation("Idle");
                return;
            }

            string next = _attackCombo.Dequeue();
            StartAnimation(next);
            _isAttacking = true;
        }

        private void StartAnimation(string anim)
        {
            if (_animations.ContainsKey(anim))
            {
                _currentAnim = anim;
                _currentFrame = 0;
                _timer = 0;
                _animLocked = anim.StartsWith("Attack") || anim == "Jump";
            }
        }

        private void Animate(GameTime gameTime)
        {
            if (!_animations.ContainsKey(_currentAnim)) return;

            Texture2D tex = _animations[_currentAnim];
            int frameCount = tex.Width / _frameWidth;
            if (frameCount == 0) return;

            _timer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_timer > _interval)
            {
                _currentFrame++;
                _timer = 0;

                if (_currentFrame >= frameCount)
                {
                    if (_currentAnim.StartsWith("Attack_"))
                    {
                        StartNextAttackInCombo();
                    }
                    else if (_currentAnim == "Jump")
                    {
                        _isJumping = false;
                        _animLocked = false;
                        SetMovementAnimation("Idle");
                    }
                    else if (_currentAnim == "Run" || _currentAnim == "Walk")
                    {
                        _currentFrame = 0; // Loop correctamente
                    }
                    else
                    {
                        _currentFrame = 0;
                        _isAttacking = false;
                        _animLocked = false;
                        SetMovementAnimation("Idle");
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_animations.ContainsKey(_currentAnim)) return;

            var tex = _animations[_currentAnim];
            int totalFrames = tex.Width / _frameWidth;
            int clampedFrame = Math.Clamp(_currentFrame, 0, totalFrames - 1);

            Rectangle source = new Rectangle(clampedFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            spriteBatch.Draw(tex, _position, source, Color.White, 0f, Vector2.Zero, 1f, _flip, 0f);

            Rectangle hitboxRect = new Rectangle((int)HitboxPosition.X, (int)HitboxPosition.Y, _hitboxWidth, _hitboxHeight);
            spriteBatch.Draw(_debugPixel, hitboxRect, Color.Red * 0.3f);
        }
    }
}
