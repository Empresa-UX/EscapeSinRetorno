using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public class EvilWizard : Enemy
    {
        private float speed = 50f;
        private float detectionRadius = 180f;

        public EvilWizard(Vector2 startPosition) : base(startPosition) { }

        public override void LoadContent(ContentManager content)
        {
            string basePath = "Characters/EvilWizard/";

            animations["Idle"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Idle"),
                FrameWidth = 250,   // 2000 / 8 frames
                FrameHeight = 250
            };

            animations["Run"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Run"),
                FrameWidth = 250,   // 2000 / 8 frames
                FrameHeight = 250
            };

            animations["Attack1"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Attack1"),
                FrameWidth = 250,   // 2000 / 8 frames
                FrameHeight = 250
            };

            animations["Attack2"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Attack2"),
                FrameWidth = 250,   // 2000 / 8 frames
                FrameHeight = 250
            };

            animations["Jump"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Jump"),
                FrameWidth = 250,   // 500 / 2 frames
                FrameHeight = 250
            };

            animations["Fall"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Fall"),
                FrameWidth = 250,   // 500 / 2 frames
                FrameHeight = 250
            };

            animations["Death"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Death"),
                FrameWidth = 250,   // 1750 / 7 frames
                FrameHeight = 250
            };

            animations["Take_hit"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>($"{basePath}Take_hit"),
                FrameWidth = 250,   // 750 / 3 frames
                FrameHeight = 250
            };

            currentAnimation = "Idle";
        }



        public override void Update(GameTime gameTime, Vector2 playerPosition)
        {
            Vector2 toPlayer = playerPosition - position;
            float dist = toPlayer.Length();

            if (dist < detectionRadius)
            {
                toPlayer.Normalize();
                position += toPlayer * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                currentAnimation = "Run";
            }
            else
            {
                currentAnimation = "Idle";
            }
        }
    }
}