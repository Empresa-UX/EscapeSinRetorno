using System;
using System.Collections.Generic;

namespace EscapeSinRetorno.Source.Chat
{
    public static class CommandRegistry
    {
        private static readonly Dictionary<string, ChatCommand> _commands = new();
        private static readonly Dictionary<string, string> _aliases = new();

        public static void Register(ChatCommand cmd)
        {
            if (cmd == null || string.IsNullOrWhiteSpace(cmd.Name))
                throw new ArgumentException("Comando inválido.");

            _commands[cmd.Name.ToLowerInvariant()] = cmd;
        }

        public static void RegisterAlias(string alias, string targetName)
        {
            if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(targetName))
                return;

            alias = alias.ToLowerInvariant();
            targetName = targetName.ToLowerInvariant();
            _aliases[alias] = targetName;
        }

        public static bool TryGet(string name, out ChatCommand cmd)
        {
            cmd = null;
            if (string.IsNullOrWhiteSpace(name)) return false;

            name = name.ToLowerInvariant();

            if (_commands.TryGetValue(name, out cmd))
                return true;

            if (_aliases.TryGetValue(name, out var target) &&
                _commands.TryGetValue(target, out cmd))
                return true;

            return false;
        }

        public static IEnumerable<ChatCommand> AllCommands => _commands.Values;
    }
}
