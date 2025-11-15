using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EscapeSinRetorno.Source.Net
{
    public sealed class UdpTransport : IDisposable
    {
        private readonly UdpClient _udp;
        public IPEndPoint Remote { get; private set; }

        public UdpTransport(string host, int remotePort, int localPort = 0)
        {
            _udp = new UdpClient(localPort);
            Remote = new IPEndPoint(IPAddress.Parse(host), remotePort);
        }

        public Task<int> SendAsync(byte[] data, IPEndPoint to = null)
        {
            var ep = to ?? Remote;
            return _udp.SendAsync(data, data.Length, ep);
        }

        public async Task<(byte[] data, IPEndPoint ep)> ReceiveAsync(CancellationToken ct)
        {
            using (ct.Register(() => _udp.Close()))
            {
                var result = await _udp.ReceiveAsync();
                return (result.Buffer, result.RemoteEndPoint);
            }
        }

        public void Dispose() => _udp?.Dispose();
    }
}