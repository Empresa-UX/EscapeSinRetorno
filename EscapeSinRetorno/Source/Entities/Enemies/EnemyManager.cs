using System.Collections.Generic;
using EscapeSinRetorno.Source.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public class EnemyManager
    {
        private readonly List<Enemy> enemies = new();
        private bool hasSpawnedFromMap = false; // Más específico
        private static int spawnCallCount = 0; // Para debugging

        public void LoadContent(ContentManager content)
        {
            foreach (var enemy in enemies)
                enemy.LoadContent(content);
        }

        public void Update(GameTime gameTime, Player player, TileMap tileMap)
        {
            foreach (var enemy in enemies)
                enemy.Update(gameTime, player, tileMap);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var enemy in enemies)
                enemy.Draw(spriteBatch);
        }

        public void SpawnFromMapData(List<(EnemyType type, Vector2 pos, string variant)> spawns)
        {
            spawnCallCount++;

            if (hasSpawnedFromMap)
            {
                return;
            }

            if (spawns == null || spawns.Count == 0)
            {
                return;
            }

            foreach (var (type, pos, variant) in spawns)
            {
                Enemy enemy = type switch
                {
                    EnemyType.EvilWizard => new EvilWizard(pos),
                    EnemyType.NightBorne => new NightBorne(pos),
                    EnemyType.MageGuardian => new MageGuardian(pos, variant),
                    _ => null
                };

                if (enemy != null)
                {
                    enemies.Add(enemy);
                }

            }

            hasSpawnedFromMap = true;
        }

        public void Add(Enemy enemy)
        {
            enemies.Add(enemy);
        }
    }
}