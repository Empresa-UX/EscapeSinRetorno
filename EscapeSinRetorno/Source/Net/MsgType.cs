namespace EscapeSinRetorno.Source.Net
{
    public enum MsgType : byte
    {
        Hello = 1,
        Welcome = 2,
        Input = 3,
        State = 4,
        Ping = 5,
        Pong = 6,
        Chat = 7, // 👈 NUEVO
    }
}
