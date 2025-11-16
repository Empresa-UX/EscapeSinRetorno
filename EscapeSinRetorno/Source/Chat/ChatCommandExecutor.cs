using EscapeSinRetorno.Source.Entities;
using EscapeSinRetorno.Source.Entities.Enemies;
using EscapeSinRetorno.Source.Systems.Stats;
using EscapeSinRetorno.Source.World;
using Microsoft.Xna.Framework;

namespace EscapeSinRetorno.Source.Chat
{
    public static class ChatCommandExecutor
    {
        public static void Execute(
            string cmd,
            string[] args,
            Game1 game,        // para acceder a ReturnToMenu / etc
            Player player,
            TileMap tileMap,
            EnemyManager enemyManager,
            ChatManager chat)
        {
            cmd = cmd.ToLowerInvariant();

            if (player == null || tileMap == null)
            {
                chat.AddErrorMessage("No hay mundo / jugador cargado.");
                return;
            }

            var stats = player.Stats;

            switch (cmd)
            {
                case "/help":
                case "/ayuda":
                    chat.AddSystemMessage("Comandos disponibles:");
                    chat.AddSystemMessage("/god      - Alternar inmortalidad");
                    chat.AddSystemMessage("/tprand   - TP aleatorio seguro");
                    chat.AddSystemMessage("/heal     - Vida al máximo");
                    chat.AddSystemMessage("/stamina  - Estamina al máximo");
                    chat.AddSystemMessage("/thirst   - Sed al máximo");
                    chat.AddSystemMessage("/sanity   - Cordura al máximo");
                    chat.AddSystemMessage("/maxall   - Todos los stats al máximo");
                    chat.AddSystemMessage("/spawnew  - Spawnear EvilWizard en tu posición");
                    chat.AddSystemMessage("/spawnnb  - Spawnear NightBorne en tu posición");
                    chat.AddSystemMessage("/menu     - Volver al menú");
                    chat.AddSystemMessage("/win      - Ganar instantáneamente");
                    chat.AddSystemMessage("/die      - Morir instantáneamente");
                    break;

                case "/god":
                    stats.DebugGodMode = !stats.DebugGodMode;
                    chat.AddSystemMessage(stats.DebugGodMode
                        ? "Modo inmortal ACTIVADO."
                        : "Modo inmortal DESACTIVADO.");
                    break;

                case "/tprand":
                case "/tprandom":
                    if (TeleportPlayerRandom(player, tileMap))
                        chat.AddSystemMessage("Teletransportado a una posición aleatoria.");
                    else
                        chat.AddErrorMessage("No se encontró posición segura para TP.");
                    break;

                case "/heal":
                    stats.Heal(99999f);
                    chat.AddSystemMessage("Vida al máximo.");
                    break;

                case "/stamina":
                    stats.Stamina.Set(stats.Stamina.Max);
                    chat.AddSystemMessage("Estamina al máximo.");
                    break;

                case "/thirst":
                    stats.Thirst.Set(stats.Thirst.Max);
                    chat.AddSystemMessage("Sed al máximo.");
                    break;

                case "/sanity":
                    stats.Sanity.Set(stats.Sanity.Max);
                    chat.AddSystemMessage("Cordura al máximo.");
                    break;

                case "/maxall":
                    stats.Heal(99999f);
                    stats.Stamina.Set(stats.Stamina.Max);
                    stats.Hunger.Set(stats.Hunger.Max);
                    stats.Thirst.Set(stats.Thirst.Max);
                    stats.Sanity.Set(stats.Sanity.Max);
                    chat.AddSystemMessage("Todos los stats al máximo.");
                    break;

                case "/spawnew":
                    {
                        var pos = player.Position;
                        var ew = new EvilWizard(pos);
                        enemyManager.Add(ew, game.Content);
                        chat.AddSystemMessage("EvilWizard spawneado.");
                    }
                    break;

                case "/spawnnb":
                    {
                        var pos = player.Position;
                        var nb = new NightBorne(pos);
                        enemyManager.Add(nb, game.Content);
                        chat.AddSystemMessage("NightBorne spawneado.");
                    }
                    break;

                case "/menu":
                    chat.AddSystemMessage("Volviendo al menú...");
                    chat.Close();
                    game.ReturnToMenu();
                    break;

                case "/win":
                    chat.AddSystemMessage("Has ganado (comando debug). Volviendo al menú...");
                    chat.Close();
                    game.ReturnToMenu();
                    break;

                case "/die":
                    stats.ApplyDamage(new DamageRequest(
                        stats.Health.Max + 9999f,
                        DamageType.True,
                        true));
                    chat.AddSystemMessage("Has muerto por comando.");
                    break;

                default:
                    chat.AddErrorMessage("Comando no reconocido. Usa /help.");
                    break;
            }
        }

        private static bool TeleportPlayerRandom(Player player, TileMap tileMap)
        {
            const int MaxTries = 200;

            for (int i = 0; i < MaxTries; i++)
            {
                int tx = Game1.Random.Next(0, tileMap.Width);
                int ty = Game1.Random.Next(0, tileMap.Height);

                var worldPos = new Vector2(
                    tx * tileMap.TileSize,
                    ty * tileMap.TileSize
                );

                if (player.IsPositionFree(tileMap, worldPos))
                {
                    player.SetPosition(worldPos);
                    return true;
                }
            }

            return false;
        }
    }
}
