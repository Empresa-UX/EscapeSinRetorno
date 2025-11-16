using EscapeSinRetorno.Source.Entities;
using EscapeSinRetorno.Source.Entities.Enemies;
using EscapeSinRetorno.Source.Systems.Stats;
using EscapeSinRetorno.Source.World;
using Microsoft.Xna.Framework;
using EscapeSinRetorno.Source.Inventory;


namespace EscapeSinRetorno.Source.Chat
{
    public static class ChatCommandExecutor
    {
        public static void Execute(
            string cmd,
            string[] args,
            Game1 game,
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
                // =========================
                // HELP / LISTA
                // =========================
                case "/help":
                case "/ayuda":
                case "/commands":
                    chat.AddSystemMessage("Comandos disponibles:");
                    chat.AddSystemMessage("/god            - Alternar inmortalidad");
                    chat.AddSystemMessage("/tprand         - TP aleatorio seguro");
                    chat.AddSystemMessage("/tp x y         - TP a tile");
                    chat.AddSystemMessage("/whereami       - Mostrar coords");
                    chat.AddSystemMessage("/heal           - Vida al máximo");
                    chat.AddSystemMessage("/stamina        - Estamina al máximo");
                    chat.AddSystemMessage("/thirst         - Sed al máximo");
                    chat.AddSystemMessage("/sanity         - Cordura al máximo");
                    chat.AddSystemMessage("/maxall         - Todos los stats al máximo");
                    chat.AddSystemMessage("/killall        - Eliminar todos los enemigos");
                    chat.AddSystemMessage("/spawnew        - Spawnear EvilWizard");
                    chat.AddSystemMessage("/spawnnb        - Spawnear NightBorne");
                    chat.AddSystemMessage("/spawn mg c     - Spawnear MageGuardian");
                    chat.AddSystemMessage("/clear          - Limpiar chat local");
                    chat.AddSystemMessage("/menu           - Volver al menú");
                    chat.AddSystemMessage("/win            - Ganar instantáneamente");
                    chat.AddSystemMessage("/die            - Morir instantáneamente");
                    break;

                // =========================
                // GOD MODE
                // =========================
                case "/god":
                    stats.DebugGodMode = !stats.DebugGodMode;
                    chat.AddSystemMessage(stats.DebugGodMode
                        ? "Modo inmortal ACTIVADO."
                        : "Modo inmortal DESACTIVADO.");
                    break;

                // =========================
                // TP RANDOM
                // =========================
                case "/tprand":
                case "/tprandom":
                    if (TeleportPlayerRandom(player, tileMap))
                        chat.AddSystemMessage("Teletransportado a una posición aleatoria.");
                    else
                        chat.AddErrorMessage("No se encontró posición segura para TP.");
                    break;

                // =========================
                // TP EXACTO (tile coords)
                // =========================
                case "/tp":
                    HandleTpExact(player, tileMap, args, chat);
                    break;

                // =========================
                // WHEREAMI
                // =========================
                case "/whereami":
                    ShowPosition(player, tileMap, chat);
                    break;

                // =========================
                // CURACIONES / STATS
                // =========================
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

                // =========================
                // ENEMIGOS
                // =========================
                case "/killall":
                    enemyManager.KillAll();
                    chat.AddSystemMessage("Todos los enemigos eliminados.");
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

                // /spawn mg red|blue|magenta   (también dejo /spawnmg color)
                case "/spawn":
                case "/spawnmg":
                    HandleSpawnMageGuardian(cmd, args, player, enemyManager, game, chat);
                    break;

                // =========================
                // CHAT LOCAL
                // =========================
                case "/clear":
                    chat.ClearMessages();
                    break;

                // =========================
                // FLUJO DE JUEGO
                // =========================
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
                case "/give":
                    {
                        if (player.Inventory == null)
                            player.Inventory = new PlayerInventory();

                        if (args.Length == 0)
                        {
                            chat.AddErrorMessage("Uso: /give <itemId> [cantidad]");
                            break;
                        }

                        string itemId = args[0];

                        int amount = 1;
                        if (args.Length >= 2 && !int.TryParse(args[1], out amount))
                            amount = 1;

                        if (!ItemDatabase.Items.ContainsKey(itemId))
                        {
                            chat.AddErrorMessage($"Item '{itemId}' no existe en ItemDatabase.");
                            break;
                        }

                        bool ok = player.Inventory.AddItem(itemId, amount);
                        if (ok)
                            chat.AddSystemMessage($"Dado {amount}x '{itemId}' al inventario.");
                        else
                            chat.AddErrorMessage("Inventario lleno, no se pudo agregar.");
                    }
                    break;

                // =========================
                // DEFAULT
                // =========================
                default:
                    chat.AddErrorMessage("Comando no reconocido. Usa /help.");
                    break;
            }
        }

        // ----------------------------------------
        // Helpers
        // ----------------------------------------

        private static void HandleTpExact(Player player, TileMap tileMap, string[] args, ChatManager chat)
        {
            if (args.Length < 2)
            {
                chat.AddErrorMessage("Uso: /tp tileX tileY");
                return;
            }

            if (!int.TryParse(args[0], out int tx) || !int.TryParse(args[1], out int ty))
            {
                chat.AddErrorMessage("Coordenadas inválidas. Usa enteros: /tp 10 15");
                return;
            }

            int tileSize = tileMap.TileSize;
            var worldPos = new Vector2(tx * tileSize, ty * tileSize);

            if (!player.IsPositionFree(tileMap, worldPos))
            {
                chat.AddErrorMessage("La posición destino está bloqueada por colisión.");
                return;
            }

            player.SetPosition(worldPos);
            chat.AddSystemMessage($"Teletransportado a tile ({tx}, {ty}).");
        }

        private static void ShowPosition(Player player, TileMap tileMap, ChatManager chat)
        {
            var w = player.Position;
            int ts = tileMap.TileSize;
            int tx = (int)(w.X / ts);
            int ty = (int)(w.Y / ts);

            chat.AddSystemMessage(
                $"Posición: world=({w.X:0},{w.Y:0}) tile=({tx},{ty})");
        }

        private static void HandleSpawnMageGuardian(
            string cmd,
            string[] args,
            Player player,
            EnemyManager enemyManager,
            Game1 game,
            ChatManager chat)
        {
            // /spawn mg color  OR  /spawnmg color
            string variantArg;

            if (cmd == "/spawn")
            {
                if (args.Length < 2 || args[0].ToLowerInvariant() != "mg")
                {
                    chat.AddErrorMessage("Uso: /spawn mg red|blue|magenta");
                    return;
                }
                variantArg = args[1];
            }
            else
            {
                // /spawnmg color
                if (args.Length < 1)
                {
                    chat.AddErrorMessage("Uso: /spawnmg red|blue|magenta");
                    return;
                }
                variantArg = args[0];
            }

            string v = variantArg.ToLowerInvariant();
            string variant = v switch
            {
                "r" or "red" => "red",
                "b" or "blue" => "blue",
                "m" or "magenta" => "magenta",
                _ => ""
            };

            if (string.IsNullOrEmpty(variant))
            {
                chat.AddErrorMessage("Color inválido. Usa: red | blue | magenta");
                return;
            }

            var pos = player.Position;
            var mg = new MageGuardian(pos, variant);
            enemyManager.Add(mg, game.Content);
            chat.AddSystemMessage($"MageGuardian {variant} spawneado.");
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
