using Microsoft.Xna.Framework;

namespace EscapeSinRetorno.Source.Chat
{
    public enum ChatMessageType
    {
        Player,
        System,
        Error
    }

    public sealed class ChatMessage
    {
        public string Text { get; }
        public ChatMessageType Type { get; }
        public float Time { get; }

        public ChatMessage(string text, ChatMessageType type, float time)
        {
            Text = text;
            Type = type;
            Time = time;
        }
    }
}
