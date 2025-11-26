using EscapeSinRetorno.Source.Entities;
using EscapeSinRetorno.Source.Entities.Enemies;
using EscapeSinRetorno.Source.World;

namespace EscapeSinRetorno.Source.Chat
{
    public sealed class CommandContext
    {
        public Game1 Game { get; init; }
        public Player Player { get; init; }
        public TileMap Map { get; init; }
        public EnemyManager EnemyManager { get; init; }
        public ChatManager Chat { get; init; }
    }
}
