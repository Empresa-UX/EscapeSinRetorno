using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Entities.Enemies
public class EvilWizard : Enemy
{
    public EvilWizard(Vector2 startPosition) : base(startPosition) { }

    public override void LoadContent(ContentManager content)
    {
        string basePath = "Characters/EVilWizard/";
        string[] states = { "Attack1", "Attack2", "Death", "Fall", "Idle", "Jump", "Run", "Take_hit" };
        foreach (var state in states)
            animations[state] = content.Load<Texture2D>($"{basePath}{state}");

        currentAnimation = "Idle";
    }

    public override void Update(GameTime gameTime, Vector2 playerPosition)
    {
        Vector2 direction = playerPosition - position;
        if (direction.Length() > 1f)
        {
            direction.Normalize();
            position += direction * 50f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            currentAnimation = "Run";
        }
        else
        {
            currentAnimation = "Attack1";
        }
    }
}
}