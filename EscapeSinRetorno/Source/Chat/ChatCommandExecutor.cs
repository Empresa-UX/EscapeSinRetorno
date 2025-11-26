using EscapeSinRetorno.Source.Entities;
using EscapeSinRetorno.Source.Entities.Enemies;
using EscapeSinRetorno.Source.World;

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

            var ctx = new CommandContext
            {
                Game = game,
                Player = player,
                Map = tileMap,
                EnemyManager = enemyManager,
                Chat = chat
            };

            if (!CommandRegistry.TryGet(cmd, out var command))
            {
                chat.AddErrorMessage("Comando no reconocido. Usa /help.");
                return;
            }

            command.Execute(args, ctx);
        }
    }
}
