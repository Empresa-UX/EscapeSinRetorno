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
    public class EnemyManager
    {
        private readonly List<Enemy> enemies = new();

        public void LoadContent(ContentManager content)
        {
            foreach (var enemy in enemies)
                enemy.LoadContent(content);
        }

        public void Update(GameTime gameTime, Vector2 playerPosition)
        {
            foreach (var enemy in enemies)
                enemy.Update(gameTime, playerPosition);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            foreach (var enemy in enemies)
                enemy.Draw(spriteBatch, camera);
        }

        public void SpawnFromMapData(List<(EnemyType type, Vector2 pos, string variant)> spawns)
        {
            foreach (var (type, pos, variant) in spawns)
            {
                Enemy enemy = type switch
                {
                    EnemyType.EvilWizard => new EvilWizard(pos),
                    EnemyType.NightBorne => new NightBorne(pos),
                    EnemyType.MageGuardian => new MageGuardian(pos, variant),
                    _ => null
                };

                if (enemy != null) enemies.Add(enemy);
            }
        }


        public void Add(Enemy enemy) => enemies.Add(enemy);
    }
}
