using System.Collections.Generic;
using EscapeSinRetorno.Source.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using EscapeSinRetorno.Source.Inventory;
using EscapeSinRetorno.Source.Items;
using EscapeSinRetorno.Source.Chat;

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
        public void Add(Enemy enemy, ContentManager content)
        {
            if (enemy == null) return;
            enemy.LoadContent(content);
            enemies.Add(enemy);
        }

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

        public void KillAll()
        {
            enemies.Clear();
        }

        public bool TryInteractWithMageGuardian(Rectangle playerHitbox, PlayerInventory inv, ChatManager chat)
        {
            if (inv == null) return false;

            // Zona de interacción alrededor del jugador
            const int interactPadding = 8;
            var area = playerHitbox;
            area.Inflate(interactPadding, interactPadding);

            foreach (var e in enemies)
            {
                if (e is not MageGuardian mg)
                    continue;

                // ¿Está cerca del jugador?
                if (!area.Intersects(e.GetHitbox()))
                    continue;

                // Si ya dio la llave antes
                if (mg.KeyAlreadyGiven)
                {
                    chat?.AddSystemMessage("Esta llave ya la tomaste.");
                    return true;
                }

                // Si no tiene llave asociada por algún motivo
                string keyId = mg.KeyItemId;
                if (string.IsNullOrEmpty(keyId))
                {
                    chat?.AddSystemMessage("Este guardián no tiene ninguna llave.");
                    return true;
                }

                // Intentar agregar la llave al inventario
                bool added = inv.AddItem(keyId, 1);
                if (!added)
                {
                    chat?.AddSystemMessage("No tienes espacio en el inventario para la llave.");
                    return true;
                }

                mg.MarkKeyGiven();

                if (ItemDatabase.Items.TryGetValue(keyId, out var item))
                    chat?.AddSystemMessage($"Obtuviste {item.Name}.");
                else
                    chat?.AddSystemMessage("Obtuviste una llave.");

                return true;
            }

            return false; // no había ningún guardián cerca
        }

    }
}