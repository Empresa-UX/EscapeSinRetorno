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
    public class MageGuardian : Enemy
    {
        private readonly string color;

        public MageGuardian(Vector2 startPosition, string color) : base(startPosition)
        {
            this.color = color;
        }

        public override void LoadContent(ContentManager content)
        {
            string path = $"Characters/MageGuardian/Idle_{color.ToLower()}";
            animations["Idle"] = content.Load<Texture2D>(path);
            currentAnimation = "Idle";
        }

        public override void Update(GameTime gameTime, Vector2 playerPosition)
        {
            currentAnimation = "Idle"; // Siempre quieto
        }
    }
}