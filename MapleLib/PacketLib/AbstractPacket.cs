using System.IO;

namespace MapleLib.PacketLib
{
    public abstract class AbstractPacket
    {
        protected MemoryStream Buffer;

        public byte[] ToArray() => Buffer.ToArray();
    }
}