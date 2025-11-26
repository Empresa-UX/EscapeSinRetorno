using System;

namespace EscapeSinRetorno.Source.Chat
{
    public sealed class ChatCommand
    {
        public string Name { get; init; }            // "/tp"
        public string Description { get; init; }     // texto corto
        public string Usage { get; init; }           // "/tp x y"
        public Action<string[], CommandContext> Execute { get; init; }
    }
}
