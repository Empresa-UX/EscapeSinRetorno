using System;
using EscapeSinRetorno.Source.Entities;
using EscapeSinRetorno.Source.Entities.Enemies;
using EscapeSinRetorno.Source.Inventory;
using EscapeSinRetorno.Source.Items;
using EscapeSinRetorno.Source.Systems.Stats;
using EscapeSinRetorno.Source.World;
using Microsoft.Xna.Framework;

namespace EscapeSinRetorno.Source.Chat
{
    public static class ChatCommandBootstrap
    {
        public static void RegisterAll()
        {
            RegisterHelp();
            RegisterGodAndStats();
            RegisterTeleport();
            RegisterEnemies();
            RegisterInventory();
            RegisterGameFlow();
            RegisterDebugVisuals();

            RegisterAliases();
        }

        // =========================
        // HELP
        // =========================
        private static void RegisterHelp()
        {
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/help",
                Description = "Muestra la lista de comandos o ayuda de uno.",
                Usage = "/help [comando]",
                Execute = (args, ctx) =>
                {
                    // /help comando → ayuda específica
                    if (args.Length >= 1)
                    {
                        string target = args[0].StartsWith("/") ? args[0] : "/" + args[0];
                        if (CommandRegistry.TryGet(target, out var cmd))
                        {
                            ctx.Chat.AddSystemMessage($"Comando: {cmd.Name}");
                            if (!string.IsNullOrWhiteSpace(cmd.Usage))
                                ctx.Chat.AddSystemMessage($"Uso: {cmd.Usage}");
                            if (!string.IsNullOrWhiteSpace(cmd.Description))
                                ctx.Chat.AddSystemMessage($"Descripción: {cmd.Description}");
                        }
                        else
                        {
                            ctx.Chat.AddErrorMessage($"No se encontró ayuda para '{target}'.");
                        }
                        return;
                    }

                    // /help → lista todos
                    ctx.Chat.AddSystemMessage("Comandos disponibles:");
                    foreach (var c in CommandRegistry.AllCommands)
                    {
                        ctx.Chat.AddSystemMessage($"{c.Name,-14} {c.Description}");
                    }
                }
            });

            // /ayuda y /commands como alias
            CommandRegistry.RegisterAlias("/ayuda", "/help");
            CommandRegistry.RegisterAlias("/commands", "/help");
        }

        // =========================
        // GOD + STATS
        // =========================
        private static void RegisterGodAndStats()
        {
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/god",
                Description = "Alternar modo inmortal.",
                Usage = "/god",
                Execute = (args, ctx) =>
                {
                    var stats = ctx.Player.Stats;
                    stats.DebugGodMode = !stats.DebugGodMode;
                    ctx.Chat.AddSystemMessage(stats.DebugGodMode
                        ? "Modo inmortal ACTIVADO."
                        : "Modo inmortal DESACTIVADO.");
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/heal",
                Description = "Vida al máximo.",
                Usage = "/heal",
                Execute = (args, ctx) =>
                {
                    ctx.Player.Stats.Heal(99999f);
                    ctx.Chat.AddSystemMessage("Vida al máximo.");
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/stamina",
                Description = "Estamina al máximo.",
                Usage = "/stamina",
                Execute = (args, ctx) =>
                {
                    var s = ctx.Player.Stats;
                    s.Stamina.Set(s.Stamina.Max);
                    ctx.Chat.AddSystemMessage("Estamina al máximo.");
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/thirst",
                Description = "Sed al máximo.",
                Usage = "/thirst",
                Execute = (args, ctx) =>
                {
                    var s = ctx.Player.Stats;
                    s.Thirst.Set(s.Thirst.Max);
                    ctx.Chat.AddSystemMessage("Sed al máximo.");
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/sanity",
                Description = "Cordura al máximo.",
                Usage = "/sanity",
                Execute = (args, ctx) =>
                {
                    var s = ctx.Player.Stats;
                    s.Sanity.Set(s.Sanity.Max);
                    ctx.Chat.AddSystemMessage("Cordura al máximo.");
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/maxall",
                Description = "Todos los stats al máximo.",
                Usage = "/maxall",
                Execute = (args, ctx) =>
                {
                    var s = ctx.Player.Stats;
                    s.Heal(99999f);
                    s.Stamina.Set(s.Stamina.Max);
                    s.Hunger.Set(s.Hunger.Max);
                    s.Thirst.Set(s.Thirst.Max);
                    s.Sanity.Set(s.Sanity.Max);
                    ctx.Chat.AddSystemMessage("Todos los stats al máximo.");
                }
            });
        }

        // =========================
        // TELEPORT / POSICIÓN
        // =========================
        private static void RegisterTeleport()
        {
            // /tprand
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/tprand",
                Description = "TP aleatorio a una posición segura.",
                Usage = "/tprand",
                Execute = (args, ctx) =>
                {
                    if (TeleportPlayerRandom(ctx.Player, ctx.Map))
                        ctx.Chat.AddSystemMessage("Teletransportado a una posición aleatoria.");
                    else
                        ctx.Chat.AddErrorMessage("No se encontró posición segura para TP.");
                }
            });
            CommandRegistry.RegisterAlias("/tprandom", "/tprand");

            // /tp x y (tiles)
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/tp",
                Description = "TP a una coordenada de tile.",
                Usage = "/tp tileX tileY",
                Execute = (args, ctx) =>
                {
                    HandleTpExact(ctx.Player, ctx.Map, args, ctx.Chat);
                }
            });

            // /whereami
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/whereami",
                Description = "Muestra posición actual.",
                Usage = "/whereami",
                Execute = (args, ctx) =>
                {
                    ShowPosition(ctx.Player, ctx.Map, ctx.Chat);
                }
            });
        }

        // =========================
        // ENEMIGOS
        // =========================
        private static void RegisterEnemies()
        {
            // /killall
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/killall",
                Description = "Elimina todos los enemigos.",
                Usage = "/killall",
                Execute = (args, ctx) =>
                {
                    ctx.EnemyManager.KillAll();
                    ctx.Chat.AddSystemMessage("Todos los enemigos eliminados.");
                }
            });

            // /spawnew
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/spawnew",
                Description = "Spawnea un EvilWizard en tu posición.",
                Usage = "/spawnew",
                Execute = (args, ctx) =>
                {
                    var pos = ctx.Player.Position;
                    var ew = new EvilWizard(pos);
                    ctx.EnemyManager.Add(ew, ctx.Game.Content);
                    ctx.Chat.AddSystemMessage("EvilWizard spawneado.");
                }
            });

            // /spawnnb
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/spawnnb",
                Description = "Spawnea un NightBorne en tu posición.",
                Usage = "/spawnnb",
                Execute = (args, ctx) =>
                {
                    var pos = ctx.Player.Position;
                    var nb = new NightBorne(pos);
                    ctx.EnemyManager.Add(nb, ctx.Game.Content);
                    ctx.Chat.AddSystemMessage("NightBorne spawneado.");
                }
            });

            // /spawn mg color  y /spawnmg color
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/spawn",
                Description = "Spawnea MageGuardian: /spawn mg color",
                Usage = "/spawn mg red|blue|magenta",
                Execute = (args, ctx) =>
                {
                    HandleSpawnMageGuardian("/spawn", args, ctx);
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/spawnmg",
                Description = "Spawnea MageGuardian: /spawnmg color",
                Usage = "/spawnmg red|blue|magenta",
                Execute = (args, ctx) =>
                {
                    HandleSpawnMageGuardian("/spawnmg", args, ctx);
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/spawnlist",
                Description = "Muestra qué enemigos se pueden spawnear.",
                Usage = "/spawnlist",
                Execute = (args, ctx) =>
                {
                    ctx.Chat.AddSystemMessage("Spawns disponibles:");
                    ctx.Chat.AddSystemMessage("- /spawnew");
                    ctx.Chat.AddSystemMessage("- /spawnnb");
                    ctx.Chat.AddSystemMessage("- /spawnmg red|blue|magenta");
                }
            });
        }

        // =========================
        // INVENTARIO
        // =========================
        private static void RegisterInventory()
        {
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/give",
                Description = "Da un ítem al jugador.",
                Usage = "/give <itemId> [cantidad]",
                Execute = (args, ctx) =>
                {
                    var player = ctx.Player;
                    var chat = ctx.Chat;

                    if (player.Inventory == null)
                        player.Inventory = new PlayerInventory();

                    if (args.Length == 0)
                    {
                        chat.AddErrorMessage("Uso: /give <itemId> [cantidad]");
                        return;
                    }

                    string itemId = args[0];

                    int amount = 1;
                    if (args.Length >= 2 && !int.TryParse(args[1], out amount))
                        amount = 1;

                    if (!ItemDatabase.Items.ContainsKey(itemId))
                    {
                        chat.AddErrorMessage($"Item '{itemId}' no existe en ItemDatabase.");
                        return;
                    }

                    bool ok = player.Inventory.AddItem(itemId, amount);
                    if (ok)
                        chat.AddSystemMessage($"Dado {amount}x '{itemId}' al inventario.");
                    else
                        chat.AddErrorMessage("Inventario lleno, no se pudo agregar.");
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/items",
                Description = "Lista los ítems del inventario.",
                Usage = "/items",
                Execute = (args, ctx) =>
                {
                    var inv = ctx.Player.Inventory;
                    if (inv == null)
                    {
                        ctx.Chat.AddSystemMessage("Inventario vacío.");
                        return;
                    }

                    bool any = false;
                    foreach (var slot in inv.Items)
                    {
                        if (!any)
                        {
                            ctx.Chat.AddSystemMessage("Inventario:");
                            any = true;
                        }

                        ctx.Chat.AddSystemMessage($"- {slot.ItemId} x{slot.Amount}");
                    }

                    if (!any)
                        ctx.Chat.AddSystemMessage("Inventario vacío.");
                }
            });


            CommandRegistry.Register(new ChatCommand
            {
                Name = "/clearinv",
                Description = "Vacía el inventario.",
                Usage = "/clearinv",
                Execute = (args, ctx) =>
                {
                    if (ctx.Player.Inventory == null)
                    {
                        ctx.Chat.AddErrorMessage("El jugador no tiene inventario.");
                        return;
                    }

                    ctx.Player.Inventory.Clear();
                    ctx.Chat.AddSystemMessage("Inventario vaciado.");
                }
            });

        }

        // =========================
        // FLUJO DE JUEGO
        // =========================
        private static void RegisterGameFlow()
        {
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/clear",
                Description = "Limpia el chat local.",
                Usage = "/clear",
                Execute = (args, ctx) =>
                {
                    ctx.Chat.ClearMessages();
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/menu",
                Description = "Vuelve al menú principal.",
                Usage = "/menu",
                Execute = (args, ctx) =>
                {
                    ctx.Chat.AddSystemMessage("Volviendo al menú...");
                    ctx.Chat.Close();
                    ctx.Game.ReturnToMenu();
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/win",
                Description = "Ganar instantáneamente y volver al menú.",
                Usage = "/win",
                Execute = (args, ctx) =>
                {
                    ctx.Chat.AddSystemMessage("Has ganado (comando debug). Volviendo al menú...");
                    ctx.Chat.Close();
                    ctx.Game.ReturnToMenu();
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/die",
                Description = "Mueres instantáneamente.",
                Usage = "/die",
                Execute = (args, ctx) =>
                {
                    var stats = ctx.Player.Stats;
                    stats.ApplyDamage(new DamageRequest(
                        stats.Health.Max + 9999f,
                        DamageType.True,
                        true));
                    ctx.Chat.AddSystemMessage("Has muerto por comando.");
                }
            });
        }

        private static void RegisterDebugVisuals()
        {
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/showhitbox",
                Description = "Muestra los hitboxes del jugador y enemigos.",
                Usage = "/showhitbox",
                Execute = (args, ctx) =>
                {
                    Player.DebugDrawHitboxes = true;
                    ctx.Chat.AddSystemMessage("Hitboxes activados.");
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/hidehitbox",
                Description = "Oculta los hitboxes.",
                Usage = "/hidehitbox",
                Execute = (args, ctx) =>
                {
                    Player.DebugDrawHitboxes = false;
                    ctx.Chat.AddSystemMessage("Hitboxes desactivados.");
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/showfps",
                Description = "Muestra el contador de FPS.",
                Usage = "/showfps",
                Execute = (args, ctx) =>
                {
                    Player.DebugDrawFPS = true;
                    ctx.Chat.AddSystemMessage("FPS activado.");
                }
            });

            CommandRegistry.Register(new ChatCommand
            {
                Name = "/hidefps",
                Description = "Oculta el contador de FPS.",
                Usage = "/hidefps",
                Execute = (args, ctx) =>
                {
                    Player.DebugDrawFPS = false;
                    ctx.Chat.AddSystemMessage("FPS desactivado.");
                }
            });
        }


        // =========================
        // ALIAS EXTRA
        // =========================
        private static void RegisterAliases()
        {
            // ejemplos de alias cortos
            CommandRegistry.RegisterAlias("/g", "/give");
            CommandRegistry.RegisterAlias("/k", "/killall");
        }

        // =========================
        // HELPERS (copiados/adaptados de tu código viejo)
        // =========================
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

            chat.AddSystemMessage($"Posición: world=({w.X:0},{w.Y:0}) tile=({tx},{ty})");
        }

        private static void HandleSpawnMageGuardian(
            string cmd,
            string[] args,
            CommandContext ctx)
        {
            var player = ctx.Player;
            var enemyManager = ctx.EnemyManager;
            var game = ctx.Game;
            var chat = ctx.Chat;

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
