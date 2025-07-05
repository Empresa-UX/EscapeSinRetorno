
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
        private bool hasSpawnedFromMap = false; // Más específico
        private static int spawnCallCount = 0; // Para debugging

        public void LoadContent(ContentManager content)
        {
            Console.WriteLine($"🔄 EnemyManager.LoadContent called. Current enemies: {enemies.Count}");
            foreach (var enemy in enemies)
                enemy.LoadContent(content);
        }

        public void Update(GameTime gameTime, Vector2 playerPosition)
        {
            foreach (var enemy in enemies)
                enemy.Update(gameTime, playerPosition);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var enemy in enemies)
                enemy.Draw(spriteBatch);
        }

        public void SpawnFromMapData(List<(EnemyType type, Vector2 pos, string variant)> spawns)
        {
            spawnCallCount++;
            Console.WriteLine($"🎯 SpawnFromMapData called #{spawnCallCount}");
            Console.WriteLine($"   Current enemies count: {enemies.Count}");
            Console.WriteLine($"   hasSpawnedFromMap: {hasSpawnedFromMap}");
            Console.WriteLine($"   Spawns parameter count: {spawns?.Count ?? 0}");

            if (hasSpawnedFromMap)
            {
                Console.WriteLine("⚠️ Enemies ya fueron spawneados desde mapa. Se ignora duplicación.");
                return;
            }

            if (spawns == null || spawns.Count == 0)
            {
                Console.WriteLine("⚠️ No hay spawns para procesar");
                return;
            }

            Console.WriteLine($"📦 Procesando {spawns.Count} spawns...");

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
                    Console.WriteLine($"✅ Enemy {type} added at {pos}. Total enemies: {enemies.Count}");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to create enemy of type {type}");
                }
            }

            hasSpawnedFromMap = true;
            Console.WriteLine($"🏁 SpawnFromMapData completed. Final count: {enemies.Count}");
        }

        public void Add(Enemy enemy)
        {
            enemies.Add(enemy);
            Console.WriteLine($"➕ Enemy added via Add() method. Total: {enemies.Count}");
        }
    }
}