using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using EscapeSinRetorno.Source.Net;

namespace EscapeSinRetorno.Source.Multiplayer
{
    public sealed class MultiplayerManager
    {
        public NetClient Client { get; private set; }
        private readonly Dictionary<int, RemotePlayer> _remotes = new();
        private ContentManager _content;

        public void StartClient(ContentManager content, string host = "127.0.0.1", string name = "Jugador")
        {
            _content = content;
            Client = new NetClient(host, NetConfig.ServerPort);
            Client.Start(name);
        }

        public void Update(GameTime gt)
        {
            if (Client == null) return;
            foreach (var kv in Client.Players)
            {
                if (kv.Key == Client.LocalId) continue; // ignora el local
                if (!_remotes.TryGetValue(kv.Key, out var rp))
                {
                    rp = new RemotePlayer();
                    rp.LoadContent(_content);
                    _remotes[kv.Key] = rp;
                }
                rp.ApplyNetState(kv.Value);
                rp.Update(gt);
            }
        }

        public bool TryGetLocalState(out NetPlayerState s)
        {
            s = default;
            var c = Client;
            if (c == null) return false;
            if (c.LocalId == 0) return false;
            return c.Players.TryGetValue(c.LocalId, out s);
        }

        public bool HasLocalReady()
        {
            var c = Client;
            if (c == null) return false;
            if (c.LocalId == 0) return false;
            return c.Players.ContainsKey(c.LocalId);
        }

        public void Draw(SpriteBatch sb)
        {
            foreach (var rp in _remotes.Values) rp.Draw(sb);
        }
    }
}
