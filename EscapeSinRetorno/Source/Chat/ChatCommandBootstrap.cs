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
            RegisterInfo();          // ← NUEVO
            RegisterAliases();
            RegisterSocial();

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

            // /setstat <stat> <valor>
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/setstat",
                Description = "Setea un stat concreto.",
                Usage = "/setstat health 50",
                Execute = (args, ctx) =>
                {
                    if (args.Length < 2)
                    {
                        ctx.Chat.AddErrorMessage("Uso: /setstat <health|stamina|hunger|thirst|sanity> <valor>");
                        return;
                    }

                    var stats = ctx.Player.Stats;
                    if (!stats.TryGetStat(args[0], out var stat))
                    {
                        ctx.Chat.AddErrorMessage("Stat desconocido.");
                        return;
                    }

                    if (!float.TryParse(args[1], out var value))
                    {
                        ctx.Chat.AddErrorMessage("Valor inválido.");
                        return;
                    }

                    stat.Set(value);
                    ctx.Chat.AddSystemMessage($"Stat '{args[0]}' seteado a {value:0.##}.");
                }
            });

            // /addstat <stat> <valor>
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/addstat",
                Description = "Suma/resta a un stat.",
                Usage = "/addstat stamina -20",
                Execute = (args, ctx) =>
                {
                    if (args.Length < 2)
                    {
                        ctx.Chat.AddErrorMessage("Uso: /addstat <stat> <valor>");
                        return;
                    }

                    var stats = ctx.Player.Stats;
                    if (!stats.TryGetStat(args[0], out var stat))
                    {
                        ctx.Chat.AddErrorMessage("Stat desconocido.");
                        return;
                    }

                    if (!float.TryParse(args[1], out var value))
                    {
                        ctx.Chat.AddErrorMessage("Valor inválido.");
                        return;
                    }

                    stat.Add(value);
                    ctx.Chat.AddSystemMessage($"Stat '{args[0]}' modificado en {value:0.##}.");
                }
            });

            // /drain <stat> <valor>
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/drain",
                Description = "Drena directo de un stat.",
                Usage = "/drain health 10",
                Execute = (args, ctx) =>
                {
                    if (args.Length < 2)
                    {
                        ctx.Chat.AddErrorMessage("Uso: /drain <stat> <valor>");
                        return;
                    }

                    var stats = ctx.Player.Stats;
                    if (!stats.TryGetStat(args[0], out var stat))
                    {
                        ctx.Chat.AddErrorMessage("Stat desconocido.");
                        return;
                    }

                    if (!float.TryParse(args[1], out var value))
                    {
                        ctx.Chat.AddErrorMessage("Valor inválido.");
                        return;
                    }

                    stat.Sub(value);
                    ctx.Chat.AddSystemMessage($"Stat '{args[0]}' drenado en {value:0.##}.");
                }
            });

            // /maxstats
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/maxstats",
                Description = "Todos los stats al máximo.",
                Usage = "/maxstats",
                Execute = (args, ctx) =>
                {
                    ctx.Player.Stats.MaxAllStats();
                    ctx.Chat.AddSystemMessage("Todos los stats al máximo.");
                }
            });

            // /zerostats
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/zerostats",
                Description = "Todos los stats a 0 (debug).",
                Usage = "/zerostats",
                Execute = (args, ctx) =>
                {
                    ctx.Player.Stats.ZeroAllStats();
                    ctx.Chat.AddSystemMessage("Todos los stats seteados a 0.");
                }
            });

            // /restore
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/restore",
                Description = "Restaura todos los stats.",
                Usage = "/restore",
                Execute = (args, ctx) =>
                {
                    ctx.Player.Stats.Restore();
                    ctx.Chat.AddSystemMessage("Stats restaurados.");
                }
            });

            // /slowdrain
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/slowdrain",
                Description = "Reduce drenaje de hambre/sed/corodura.",
                Usage = "/slowdrain",
                Execute = (args, ctx) =>
                {
                    ctx.Player.Stats.SetDrainModeSlow();
                    ctx.Chat.AddSystemMessage("Drenaje de supervivencia LENTO.");
                }
            });

            // /fastdrain
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/fastdrain",
                Description = "Aumenta drenaje de hambre/sed/corodura.",
                Usage = "/fastdrain",
                Execute = (args, ctx) =>
                {
                    ctx.Player.Stats.SetDrainModeFast();
                    ctx.Chat.AddSystemMessage("Drenaje de supervivencia RÁPIDO.");
                }
            });

            // /godstamina
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/godstamina",
                Description = "Correr sin gastar stamina.",
                Usage = "/godstamina",
                Execute = (args, ctx) =>
                {
                    var s = ctx.Player.Stats;
                    s.DebugGodStamina = !s.DebugGodStamina;
                    ctx.Chat.AddSystemMessage(
                        s.DebugGodStamina ? "GodStamina ACTIVADO." : "GodStamina DESACTIVADO.");
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

            // /calm - por ahora solo mensaje, o más adelante llamás a un CalmAll() real
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/calm",
                Description = "Resetea la IA (placeholder).",
                Usage = "/calm",
                Execute = (args, ctx) =>
                {
                    // Si más adelante agregás EnemyManager.CalmAll(), llamalo acá.
                    ctx.Chat.AddSystemMessage("Calm: (placeholder) IA reseteada.");
                }
            });

            // /freezeai
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/freezeai",
                Description = "Congela AI de todos los enemigos.",
                Usage = "/freezeai",
                Execute = (args, ctx) =>
                {
                    ctx.EnemyManager.SetFrozen(true);
                    ctx.Chat.AddSystemMessage("IA de enemigos congelada.");
                }
            });

            // /unfreezeai
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/unfreezeai",
                Description = "Descongela AI.",
                Usage = "/unfreezeai",
                Execute = (args, ctx) =>
                {
                    ctx.EnemyManager.SetFrozen(false);
                    ctx.Chat.AddSystemMessage("IA de enemigos reanudada.");
                }
            });

            // /killnear <dist>
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/killnear",
                Description = "Mata enemigos en un radio.",
                Usage = "/killnear <dist>",
                Execute = (args, ctx) =>
                {
                    if (args.Length == 0 || !float.TryParse(args[0], out var radius))
                    {
                        ctx.Chat.AddErrorMessage("Uso: /killnear <dist>");
                        return;
                    }

                    radius = MathF.Max(10f, radius);
                    ctx.EnemyManager.KillEnemiesInRadius(ctx.Player.Center, radius);
                    ctx.Chat.AddSystemMessage($"Enemigos en radio {radius:0} eliminados.");
                }
            });

            // /clone <tipo> [cant]
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/clone",
                Description = "Spawnea múltiples enemigos de un tipo.",
                Usage = "/clone <tipo> [cant]",
                Execute = (args, ctx) =>
                {
                    if (args.Length == 0)
                    {
                        ctx.Chat.AddErrorMessage("Uso: /clone <tipo> [cant]");
                        return;
                    }

                    string typeId = args[0];
                    int count = 1;
                    if (args.Length >= 2 && !int.TryParse(args[1], out count))
                        count = 1;

                    count = Math.Clamp(count, 1, 50);
                    ctx.EnemyManager.Clone(typeId, count, ctx.Player.Position, ctx.Game.Content);
                    ctx.Chat.AddSystemMessage($"Spawn de {count}x '{typeId}'.");
                }
            });

            // /despawn <tipo>
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/despawn",
                Description = "Elimina todos los enemigos de un tipo.",
                Usage = "/despawn <tipo>",
                Execute = (args, ctx) =>
                {
                    if (args.Length == 0)
                    {
                        ctx.Chat.AddErrorMessage("Uso: /despawn <tipo>");
                        return;
                    }

                    ctx.EnemyManager.DespawnByType(args[0]);
                    ctx.Chat.AddSystemMessage($"Enemigos de tipo '{args[0]}' eliminados.");
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

            // /giveall
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/giveall",
                Description = "Da un pack de ítems.",
                Usage = "/giveall",
                Execute = (args, ctx) =>
                {
                    var inv = ctx.Player.Inventory ??= new PlayerInventory();

                    void Give(string id, int amt)
                    {
                        if (ItemDatabase.Items.ContainsKey(id))
                            inv.AddItem(id, amt);
                    }

                    Give("potion_life", 5);
                    Give("watter_bottle", 5);
                    Give("meal", 5);
                    Give("main_key", 1);
                    Give("cyan_key", 1);

                    ctx.Chat.AddSystemMessage("Pack de ítems otorgado.");
                }
            });

            // /remove <itemId> [cant]
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/remove",
                Description = "Remueve ítems del inventario.",
                Usage = "/remove <itemId> [cant]",
                Execute = (args, ctx) =>
                {
                    var inv = ctx.Player.Inventory;
                    if (inv == null)
                    {
                        ctx.Chat.AddErrorMessage("El jugador no tiene inventario.");
                        return;
                    }

                    if (args.Length == 0)
                    {
                        ctx.Chat.AddErrorMessage("Uso: /remove <itemId> [cant]");
                        return;
                    }

                    string itemId = args[0];
                    int amount = 1;
                    if (args.Length >= 2 && !int.TryParse(args[1], out amount))
                        amount = 1;

                    if (!inv.RemoveItem(itemId, amount))
                    {
                        ctx.Chat.AddErrorMessage("No se pudo remover el ítem (no encontrado o cantidad insuficiente).");
                    }
                    else
                    {
                        ctx.Chat.AddSystemMessage($"Removido {amount}x '{itemId}' del inventario.");
                    }
                }
            });

            // /drop <itemId> [cant]
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/drop",
                Description = "Dropea ítems al suelo.",
                Usage = "/drop <itemId> [cant]",
                Execute = (args, ctx) =>
                {
                    var inv = ctx.Player.Inventory;
                    if (inv == null)
                    {
                        ctx.Chat.AddErrorMessage("El jugador no tiene inventario.");
                        return;
                    }

                    if (args.Length == 0)
                    {
                        ctx.Chat.AddErrorMessage("Uso: /drop <itemId> [cant]");
                        return;
                    }

                    string itemId = args[0];
                    int amount = 1;
                    if (args.Length >= 2 && !int.TryParse(args[1], out amount))
                        amount = 1;

                    if (!inv.RemoveItem(itemId, amount))
                    {
                        ctx.Chat.AddErrorMessage("No se pudo remover el ítem para dropearlo.");
                        return;
                    }

                    ctx.Game.SpawnItemOnGround(itemId, amount, ctx.Player.Center);
                    ctx.Chat.AddSystemMessage($"Dropeado {amount}x '{itemId}' al suelo.");
                }
            });

            // /wipeinv
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/wipeinv",
                Description = "Vacía inventario y items del piso.",
                Usage = "/wipeinv",
                Execute = (args, ctx) =>
                {
                    if (ctx.Player.Inventory != null)
                        ctx.Player.Inventory.Clear();

                    ctx.Game.ClearAllPickups();
                    ctx.Chat.AddSystemMessage("Inventario y pickups limpiados.");
                }
            });

            // /searchitem <texto>
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/searchitem",
                Description = "Busca ítems en el ItemDatabase por texto.",
                Usage = "/searchitem <texto>",
                Execute = (args, ctx) =>
                {
                    if (args.Length == 0)
                    {
                        ctx.Chat.AddErrorMessage("Uso: /searchitem <texto>");
                        return;
                    }

                    string pattern = args[0].ToLowerInvariant();
                    int count = 0;
                    foreach (var kv in ItemDatabase.Items)
                    {
                        var id = kv.Key;
                        var def = kv.Value;
                        if (id.ToLowerInvariant().Contains(pattern) ||
                            def.Name.ToLowerInvariant().Contains(pattern))
                        {
                            ctx.Chat.AddSystemMessage($"- {id}: {def.Name}");
                            count++;
                        }
                    }

                    if (count == 0)
                        ctx.Chat.AddSystemMessage("No se encontraron ítems que coincidan.");
                }
            });

            // /spawnitem <itemId> [cant]
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/spawnitem",
                Description = "Spawnea ítems en el suelo sin tocar inventario.",
                Usage = "/spawnitem <itemId> [cant]",
                Execute = (args, ctx) =>
                {
                    if (args.Length == 0)
                    {
                        ctx.Chat.AddErrorMessage("Uso: /spawnitem <itemId> [cant]");
                        return;
                    }

                    string itemId = args[0];
                    if (!ItemDatabase.Items.ContainsKey(itemId))
                    {
                        ctx.Chat.AddErrorMessage($"Ítem '{itemId}' no existe en ItemDatabase.");
                        return;
                    }

                    int amount = 1;
                    if (args.Length >= 2 && !int.TryParse(args[1], out amount))
                        amount = 1;

                    ctx.Game.SpawnItemOnGround(itemId, amount, ctx.Player.Center);
                    ctx.Chat.AddSystemMessage($"Spawneado {amount}x '{itemId}' en el suelo.");
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

            // /showcollisions
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/showcollisions",
                Description = "Destaca tiles colisionables.",
                Usage = "/showcollisions",
                Execute = (args, ctx) =>
                {
                    ctx.Game.DebugShowCollisions = true;
                    ctx.Chat.AddSystemMessage("Mostrar colisiones ACTIVADO.");
                }
            });

            // /nocollisions (noclip jugador)
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/nocollisions",
                Description = "Ignora colisiones (noclip para el jugador).",
                Usage = "/nocollisions",
                Execute = (args, ctx) =>
                {
                    Player.DebugNoClip = !Player.DebugNoClip;
                    ctx.Chat.AddSystemMessage(
                        Player.DebugNoClip ? "Noclip ACTIVADO." : "Noclip DESACTIVADO.");
                }
            });

            // /fx off
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/fx",
                Description = "Controla efectos visuales (por ahora solo off).",
                Usage = "/fx off",
                Execute = (args, ctx) =>
                {
                    if (args.Length == 0)
                    {
                        ctx.Chat.AddErrorMessage("Uso: /fx off");
                        return;
                    }

                    string sub = args[0].ToLowerInvariant();
                    if (sub == "off")
                    {
                        ctx.Game.FxEnabled = false;
                        ctx.Chat.AddSystemMessage("FX visuales DESACTIVADOS.");
                    }
                    else if (sub == "on")
                    {
                        ctx.Game.FxEnabled = true;
                        ctx.Chat.AddSystemMessage("FX visuales ACTIVADOS.");
                    }
                    else
                    {
                        ctx.Chat.AddErrorMessage("Uso: /fx on|off");
                    }
                }
            });

            // /speed <valor>
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/speed",
                Description = "Cambia velocidad base del jugador.",
                Usage = "/speed <valor>",
                Execute = (args, ctx) =>
                {
                    if (args.Length == 0 || !float.TryParse(args[0], out var v))
                    {
                        ctx.Chat.AddErrorMessage("Uso: /speed <valor>");
                        return;
                    }

                    ctx.Player.BaseSpeed = v;
                    ctx.Chat.AddSystemMessage($"Velocidad base seteada a {v:0.##}.");
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

        private static void RegisterInfo()
        {
            // /stats
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/stats",
                Description = "Muestra stats básicos con %.",
                Usage = "/stats",
                Execute = (args, ctx) =>
                {
                    var s = ctx.Player.Stats;
                    ctx.Chat.AddSystemMessage(
                        $"HP: {s.Health.Current:0}/{s.Health.Max:0} ({s.Health.Ratio * 100:0}%)");
                    ctx.Chat.AddSystemMessage(
                        $"Stamina: {s.Stamina.Current:0}/{s.Stamina.Max:0} ({s.Stamina.Ratio * 100:0}%)");
                    ctx.Chat.AddSystemMessage(
                        $"Hambre: {s.Hunger.Current:0}/{s.Hunger.Max:0} ({s.Hunger.Ratio * 100:0}%)");
                    ctx.Chat.AddSystemMessage(
                        $"Sed: {s.Thirst.Current:0}/{s.Thirst.Max:0} ({s.Thirst.Ratio * 100:0}%)");
                    ctx.Chat.AddSystemMessage(
                        $"Cordura: {s.Sanity.Current:0}/{s.Sanity.Max:0} ({s.Sanity.Ratio * 100:0}%)");
                }
            });

            // /statdetail
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/statdetail",
                Description = "Stats + regen y drenajes.",
                Usage = "/statdetail",
                Execute = (args, ctx) =>
                {
                    var s = ctx.Player.Stats;
                    ctx.Chat.AddSystemMessage("=== STATS DETALLE ===");
                    ctx.Chat.AddSystemMessage($"Health: {s.Health.Current:0}/{s.Health.Max:0}  regen={s.Health.RegenPerSec:0.00}");
                    ctx.Chat.AddSystemMessage($"Stamina: {s.Stamina.Current:0}/{s.Stamina.Max:0}  regen={s.Stamina.RegenPerSec:0.00}");
                    ctx.Chat.AddSystemMessage($"Hunger: {s.Hunger.Current:0}/{s.Hunger.Max:0}  regen={s.Hunger.RegenPerSec:0.00}");
                    ctx.Chat.AddSystemMessage($"Thirst: {s.Thirst.Current:0}/{s.Thirst.Max:0}  regen={s.Thirst.RegenPerSec:0.00}");
                    ctx.Chat.AddSystemMessage($"Sanity: {s.Sanity.Current:0}/{s.Sanity.Max:0}  regen={s.Sanity.RegenPerSec:0.00}");
                }
            });

            // /pos y /coords (alias)
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/pos",
                Description = "Posición world/tile.",
                Usage = "/pos",
                Execute = (args, ctx) =>
                {
                    var w = ctx.Player.Position;
                    int ts = ctx.Map.TileSize;
                    int tx = (int)(w.X / ts);
                    int ty = (int)(w.Y / ts);
                    var hb = ctx.Player.GetHitbox();
                    ctx.Chat.AddSystemMessage(
                        $"Posición: world=({w.X:0},{w.Y:0}) tile=({tx},{ty}) hitbox=({hb.X},{hb.Y},{hb.Width},{hb.Height})");
                }
            });
            CommandRegistry.RegisterAlias("/coords", "/pos");

            // /time
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/time",
                Description = "Tiempo de juego (partida actual).",
                Usage = "/time",
                Execute = (args, ctx) =>
                {
                    double t = ctx.Game.WorldTimeSeconds; // propiedad nueva
                    int sec = (int)t;
                    int h = sec / 3600;
                    int m = (sec % 3600) / 60;
                    int s = sec % 60;
                    ctx.Chat.AddSystemMessage($"Tiempo de juego: {h:D2}:{m:D2}:{s:D2}");
                }
            });

            // /mapinfo
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/mapinfo",
                Description = "Info del mapa actual.",
                Usage = "/mapinfo",
                Execute = (args, ctx) =>
                {
                    var map = ctx.Map;
                    int ts = map.TileSize;
                    int w = map.Width;
                    int h = map.Height;

                    int solid = 0;
                    for (int y = 0; y < h; y++)
                        for (int x = 0; x < w; x++)
                        {
                            var pos = new Vector2(x * ts, y * ts);
                            if (map.IsColliding(pos, ts, ts))
                                solid++;
                        }

                    ctx.Chat.AddSystemMessage($"Mapa: {w}x{h} tiles (tileSize={ts})");
                    ctx.Chat.AddSystemMessage($"Tiles sólidos aproximados: {solid}");
                }
            });

            // /doors (requiere DoorManager helper GetDoorStats)
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/doors",
                Description = "Puertas en el mapa.",
                Usage = "/doors",
                Execute = (args, ctx) =>
                {
                    if (ctx.Game.DoorManager == null)
                    {
                        ctx.Chat.AddErrorMessage("No hay DoorManager.");
                        return;
                    }

                    ctx.Game.DoorManager.GetDoorStats(out int total, out int open, out int closed);
                    ctx.Chat.AddSystemMessage($"Puertas: total={total}, abiertas={open}, cerradas={closed}");
                }
            });

            // /near [radio]
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/near",
                Description = "Enemigos/objetos cercanos (debug).",
                Usage = "/near [radio]",
                Execute = (args, ctx) =>
                {
                    float radius = 200f;
                    if (args.Length >= 1 && float.TryParse(args[0], out var r))
                        radius = MathF.Max(10f, r);

                    var center = ctx.Player.Center;

                    // Enemigos
                    var enemies = ctx.EnemyManager.GetEnemiesInRadius(center, radius);
                    int count = 0;
                    foreach (var e in enemies)
                    {
                        count++;
                        ctx.Chat.AddSystemMessage($"Enemy: {e.TypeId} pos=({e.Position.X:0},{e.Position.Y:0})");
                    }

                    if (count == 0)
                        ctx.Chat.AddSystemMessage("No hay enemigos cercanos en el radio.");

                    // Opcional: pickups si agregaste GetPickupsInRadius al manager
                    /*
                    var pickups = ctx.Game.PickupManager.GetPickupsInRadius(center, radius);
                    foreach (var p in pickups)
                    {
                        ctx.Chat.AddSystemMessage($"Pickup: {p.ItemId} pos=({p.Position.X:0},{p.Position.Y:0})");
                    }
                    */
                }
            });

            // /target
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/target",
                Description = "Muestra el enemigo que estás mirando.",
                Usage = "/target",
                Execute = (args, ctx) =>
                {
                    var enemy = ctx.EnemyManager.GetEnemyInSight(ctx.Player);
                    if (enemy == null)
                    {
                        ctx.Chat.AddSystemMessage("No hay objetivo en la mira.");
                        return;
                    }

                    var pos = enemy.Position;
                    ctx.Chat.AddSystemMessage(
                        $"Target: {enemy.TypeId} HP={enemy.Health:0} pos=({pos.X:0},{pos.Y:0})");
                }
            });

            // /enemyhp
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/enemyhp",
                Description = "HP de enemigos cercanos.",
                Usage = "/enemyhp [radio]",
                Execute = (args, ctx) =>
                {
                    float radius = 200f;
                    if (args.Length >= 1 && float.TryParse(args[0], out var r))
                        radius = MathF.Max(10f, r);

                    var center = ctx.Player.Center;
                    int count = 0;
                    foreach (var e in ctx.EnemyManager.GetEnemiesInRadius(center, radius))
                    {
                        count++;
                        ctx.Chat.AddSystemMessage(
                            $"{e.TypeId} HP={e.Health:0}/{e.MaxHealth:0} pos=({e.Position.X:0},{e.Position.Y:0})");
                    }

                    if (count == 0)
                        ctx.Chat.AddSystemMessage("No hay enemigos en el radio.");
                }
            });
        }

        private static void RegisterSocial()
        {
            // /roll
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/roll",
                Description = "Tira un dado 1-100.",
                Usage = "/roll",
                Execute = (args, ctx) =>
                {
                    int value = Game1.Random.Next(1, 101);
                    ctx.Chat.AddSystemMessage($"[Roll] sacaste {value} (1-100).");
                }
            });

            // /me <acción>
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/me",
                Description = "Mensaje estilo roleplay.",
                Usage = "/me <acción>",
                Execute = (args, ctx) =>
                {
                    if (args.Length == 0)
                    {
                        ctx.Chat.AddErrorMessage("Uso: /me <acción>");
                        return;
                    }

                    string action = string.Join(" ", args);
                    ctx.Chat.AddSystemMessage($"* Player {action}");
                }
            });

            // /shrug
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/shrug",
                Description = "Inserta ¯\\_(ツ)_/¯",
                Usage = "/shrug",
                Execute = (args, ctx) =>
                {
                    ctx.Chat.AddPlayerMessage("¯\\_(ツ)_/¯");
                }
            });

            // /exec <cmd1; cmd2; cmd3>
            CommandRegistry.Register(new ChatCommand
            {
                Name = "/exec",
                Description = "Ejecuta varios comandos separados por ';'.",
                Usage = "/exec /heal; /give potion_life 5; /spawnnb",
                Execute = (args, ctx) =>
                {
                    if (args.Length == 0)
                    {
                        ctx.Chat.AddErrorMessage("Uso: /exec <cmd1; cmd2; ...>");
                        return;
                    }

                    string joined = string.Join(" ", args);
                    var parts = joined.Split(';', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var raw in parts)
                    {
                        string trimmed = raw.Trim();
                        if (string.IsNullOrEmpty(trimmed))
                            continue;

                        if (!trimmed.StartsWith("/"))
                        {
                            ctx.Chat.AddErrorMessage($"'/exec': '{trimmed}' no parece un comando (falta '/').");
                            continue;
                        }

                        string[] tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        string cmdName = tokens[0];
                        string[] subArgs = tokens.Length > 1 ? tokens[1..] : Array.Empty<string>();

                        ChatCommandExecutor.Execute(
                            cmdName,
                            subArgs,
                            ctx.Game,
                            ctx.Player,
                            ctx.Map,
                            ctx.EnemyManager,
                            ctx.Chat);
                    }
                }
            });
        }
    }
}
