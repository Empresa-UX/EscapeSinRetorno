using System;
using System.IO;
using System.Text;

namespace EscapeSinRetorno.Source.Net
{
    public sealed class NetWriter : IDisposable
    {
        private readonly MemoryStream _ms;
        private readonly BinaryWriter _bw;

        public NetWriter(int capacity = 256)
        {
            _ms = new MemoryStream(capacity);
            _bw = new BinaryWriter(_ms);
        }

        public void Write(byte v) => _bw.Write(v);
        public void Write(int v) => _bw.Write(v);
        public void Write(float v) => _bw.Write(v);
        public void Write(bool v) => _bw.Write(v);

        public void Write(string s)
        {
            var b = Encoding.UTF8.GetBytes(s ?? "");
            _bw.Write((ushort)b.Length);
            _bw.Write(b);
        }

        public byte[] ToArray() => _ms.ToArray();

        public void Dispose()
        {
            _bw.Dispose();
            _ms.Dispose();
        }
    }

    // ⬇️ CLASE normal (NO ref struct) — segura para async/await
    public sealed class NetReader
    {
        private readonly byte[] _buf;
        private int _pos;

        public NetReader(byte[] data)
        {
            _buf = data ?? Array.Empty<byte>();
            _pos = 0;
        }

        public byte ReadByte()
        {
            var v = _buf[_pos];
            _pos += 1;
            return v;
        }

        public int ReadInt()
        {
            int v = BitConverter.ToInt32(_buf, _pos);
            _pos += 4;
            return v;
        }

        public float ReadFloat()
        {
            float v = BitConverter.ToSingle(_buf, _pos);
            _pos += 4;
            return v;
        }

        public bool ReadBool()
        {
            bool v = _buf[_pos] != 0;
            _pos += 1;
            return v;
        }

        public string ReadString()
        {
            ushort len = BitConverter.ToUInt16(_buf, _pos);
            _pos += 2;
            var s = Encoding.UTF8.GetString(_buf, _pos, len);
            _pos += len;
            return s;
        }

        public bool EoF => _pos >= _buf.Length;
    }
}