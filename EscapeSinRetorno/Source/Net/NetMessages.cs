using Microsoft.Xna.Framework;

namespace EscapeSinRetorno.Source.Net
{
    public static class NetMessages
    {
        public static byte[] Hello(string name)
        {
            using var w = new NetWriter();
            w.Write((byte)MsgType.Hello);
            w.Write(name ?? "");
            return w.ToArray();
        }

        public static byte[] Welcome(int playerId)
        {
            using var w = new NetWriter();
            w.Write((byte)MsgType.Welcome);
            w.Write(playerId);
            return w.ToArray();
        }

        public static byte[] Input(int playerId, Vector2 dir, bool run, bool attack)
        {
            using var w = new NetWriter();
            w.Write((byte)MsgType.Input);
            w.Write(playerId);
            w.Write(dir.X); w.Write(dir.Y);
            w.Write(run); w.Write(attack);
            return w.ToArray();
        }

        public static byte[] State(int tick, NetPlayerState[] players)
        {
            using var w = new NetWriter(256);
            w.Write((byte)MsgType.State);
            w.Write(tick);
            w.Write(players.Length);
            foreach (var p in players)
            {
                w.Write(p.Id);
                w.Write(p.X); w.Write(p.Y);
                w.Write((byte)p.Flip);
                w.Write((byte)p.Anim);
            }
            return w.ToArray();
        }

        public static byte[] Ping(int t) { using var w = new NetWriter(); w.Write((byte)MsgType.Ping); w.Write(t); return w.ToArray(); }
        public static byte[] Pong(int t) { using var w = new NetWriter(); w.Write((byte)MsgType.Pong); w.Write(t); return w.ToArray(); }
    }

    public enum NetAnim : byte { Idle = 0, Walk = 1, Run = 2, Attack = 3, Hurt = 4, Death = 5 }
    public enum NetFlip : byte { Right = 0, Left = 1 }

    public struct NetPlayerState
    {
        public int Id;
        public float X, Y;
        public NetFlip Flip;
        public NetAnim Anim;
    }
}