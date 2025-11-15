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
        private bool hasSpawnedFromMap = false;
        private List<(EnemyType type, Vector2 pos, string variant)> _lastSpawns; // para respawn

        public void LoadContent(ContentManager content) => enemies.ForEach(e => e.LoadContent(content));

        public void Update(GameTime gameTime, Player player, TileMap tileMap)
        {
            // Si el jugador está muerto, no actualizamos IA ni aplicamos daño.
            if (player?.Stats?.IsDead == true) return;

            // Actualiza normalmente
            enemies.ForEach(e => e.Update(gameTime, player, tileMap));
        }

        public void Draw(SpriteBatch spriteBatch) => enemies.ForEach(e => e.Draw(spriteBatch));
        public List<Enemy> GetEnemies() => enemies;
        public void Add(Enemy enemy) => enemies.Add(enemy);

        public void SpawnFromMapData(List<(EnemyType type, Vector2 pos, string variant)> spawns)
        {
            if (hasSpawnedFromMap || spawns == null || spawns.Count == 0) return;

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
            _lastSpawns = spawns;
            hasSpawnedFromMap = true;
        }

        // Para respawn rápido del nivel
        public void ResetFromLastSpawns()
        {
            if (_lastSpawns == null) return;
            enemies.Clear();
            hasSpawnedFromMap = false;
            SpawnFromMapData(_lastSpawns);
        }
    }
}