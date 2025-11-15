using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace EscapeSinRetorno.Source.Net
{
    public sealed class NetClient : IDisposable
    {
        private readonly UdpTransport _udp;
        private CancellationTokenSource _cts;
        private readonly object _lock = new();

        public int LocalId { get; private set; }
        public Dictionary<int, NetPlayerState> Players { get; } = new();

        public NetClient(string host, int port = NetConfig.ServerPort)
        {
            _udp = new UdpTransport(host, port);
        }

        public void Start(string name = "Player")
        {
            _cts = new CancellationTokenSource();
            _ = RecvLoop(_cts.Token);
            _ = SendTickLoop(_cts.Token);
            _ = _udp.SendAsync(NetMessages.Hello(name));
        }

        private async Task RecvLoop(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var (data, _) = await _udp.ReceiveAsync(ct);
                    var r = new NetReader(data);
                    var t = (MsgType)r.ReadByte();
                    switch (t)
                    {
                        case MsgType.Welcome:
                            LocalId = r.ReadInt();
                            break;
                        case MsgType.State:
                            {
                                int tick = r.ReadInt();
                                int n = r.ReadInt();
                                lock (_lock)
                                {
                                    for (int i = 0; i < n; i++)
                                    {
                                        var id = r.ReadInt();
                                        var x = r.ReadFloat();
                                        var y = r.ReadFloat();
                                        var flip = (NetFlip)r.ReadByte();
                                        var anim = (NetAnim)r.ReadByte();
                                        Players[id] = new NetPlayerState { Id = id, X = x, Y = y, Flip = flip, Anim = anim };
                                    }
                                }
                                break;
                            }
                        case MsgType.Pong:
                            r.ReadInt(); // ignore
                            break;
                    }
                }
            }
            catch (ObjectDisposedException) { }
        }

        private async Task SendTickLoop(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                    await Task.Delay(1000 / NetConfig.TickRate, ct);
            }
            catch (TaskCanceledException) { }
        }

        public Task SendInputAsync(Vector2 dir, bool run, bool attack)
        {
            if (dir.LengthSquared() < 1e-6f) run = false;
            var pkt = NetMessages.Input(LocalId, dir, run, attack);
            return _udp.SendAsync(pkt);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _udp?.Dispose();
        }
    }
}
