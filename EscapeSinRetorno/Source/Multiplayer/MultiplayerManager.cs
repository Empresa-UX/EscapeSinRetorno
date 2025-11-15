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
        public int LocalId { get; private set; }
        public bool Enabled => Client != null;

        private readonly Dictionary<int, RemotePlayer> _remotes = new();
        private ContentManager _content;

        public void StartClient(ContentManager content, string host, string name = "Player", int port = NetConfig.ServerPort)
        {
            Stop();
            _content = content;
            Client = new NetClient(host, port);
            Client.Start(name); // tu NetClient expone Start()
        }

        public void Stop()
        {
            try { Client?.Dispose(); } catch { }
            Client = null;
            LocalId = 0;
            _remotes.Clear();
        }

        public void Update(GameTime gt)
        {
            if (Client == null) return;

            // LocalId llega asíncrono tras Welcome
            if (Client.LocalId != 0) LocalId = Client.LocalId;

            // Sincronizar remotos con el snapshot del cliente
            var seen = new HashSet<int>();
            foreach (var kv in Client.Players)
            {
                int id = kv.Key;
                if (id == LocalId) continue; // NO dibujar/crear al local

                seen.Add(id);

                if (!_remotes.TryGetValue(id, out var rp))
                {
                    rp = new RemotePlayer();
                    rp.LoadContent(_content);
                    _remotes[id] = rp;
                }
                rp.ApplyNetState(kv.Value);
            }

            // GC de remotos no vistos en este snapshot
            var toRemove = new List<int>();
            foreach (var kv in _remotes)
                if (!seen.Contains(kv.Key)) toRemove.Add(kv.Key);
            foreach (var id in toRemove) _remotes.Remove(id);

            // Tick de animación
            foreach (var rp in _remotes.Values) rp.Update(gt);
        }

        public void Draw(SpriteBatch sb)
        {
            if (Client == null) return;
            foreach (var rp in _remotes.Values) rp.Draw(sb);
        }
    }
}