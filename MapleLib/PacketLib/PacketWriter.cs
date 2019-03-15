using System.IO;
using System.Text;

namespace MapleLib.PacketLib
{
    /// <summary>
    /// Class to handle writing packets
    /// </summary>
    public class PacketWriter : AbstractPacket
    {
        /// <summary>
        /// The main writer tool
        /// </summary>
        private readonly BinaryWriter _binWriter;

        /// <summary>
        /// Amount of data writen in the writer
        /// </summary>
        public short Length => (short) Buffer.Length;

        /// <summary>
        /// Creates a new instance of PacketWriter
        /// </summary>
        /// <param name="size">Starting size of the buffer</param>
        public PacketWriter(int size = 0)
        {
            Buffer = new MemoryStream(size);
            _binWriter = new BinaryWriter(Buffer, Encoding.ASCII);
        }

        public PacketWriter(byte[] data)
        {
            Buffer = new MemoryStream(data);
            _binWriter = new BinaryWriter(Buffer, Encoding.ASCII);
        }

        /// <summary>
        /// Restart writing from the point specified. This will overwrite data in the packet.
        /// </summary>
        /// <param name="length">The point of the packet to start writing from.</param>
        public void Reset(int length) => Buffer.Seek(length, SeekOrigin.Begin);

        /// <summary>
        /// Writes a byte to the stream
        /// </summary>
        /// <param name="writeValue">The byte to write</param>
        public void WriteByte(int writeValue) => _binWriter.Write((byte) writeValue);

        /// <summary>
        /// Writes a byte array to the stream
        /// </summary>
        /// <param name="writeValue">The byte array to write</param>
        public void WriteBytes(byte[] writeValue) => _binWriter.Write(writeValue);

        /// <summary>
        /// Writes a boolean to the stream
        /// </summary>
        /// <param name="writeValue">The boolean to write</param>
        public void WriteBool(bool writeValue) => _binWriter.Write(writeValue);

        /// <summary>
        /// Writes a short to the stream
        /// </summary>
        /// <param name="writeValue">The short to write</param>
        public void WriteShort(int writeValue) => _binWriter.Write((short) writeValue);

        /// <summary>
        /// Writes an int to the stream
        /// </summary>
        /// <param name="writeValue">The int to write</param>
        public void WriteInt(int writeValue) => _binWriter.Write(writeValue);

        /// <summary>
        /// Writes a long to the stream
        /// </summary>
        /// <param name="writeValue">The long to write</param>
        public void WriteLong(long writeValue) => _binWriter.Write(writeValue);

        /// <summary>
        /// Writes a string to the stream
        /// </summary>
        /// <param name="writeValue">The string to write</param>
        public void WriteString(string writeValue) => _binWriter.Write(writeValue.ToCharArray());

        /// <summary>
        /// Writes a string prefixed with a [short] length before it, to the stream
        /// </summary>
        /// <param name="writeValue">The string to write</param>
        public void WriteMapleString(string writeValue)
        {
            WriteShort((short) writeValue.Length);
            WriteString(writeValue);
        }

        /// <summary>
        /// Writes a hex-string to the stream
        /// </summary>
        /// <param name="pHexString">The hex-string to write</param>
        public void WriteHexString(string pHexString) => WriteBytes(HexEncoding.GetBytes(pHexString));

        /// <summary>
        /// Sets a byte in the stream
        /// </summary>
        /// <param name="index">The index of the stream to set data at</param>
        /// <param name="writeValue">The byte to set</param>
        public void SetByte(long index, int writeValue)
        {
            var oldIndex = Buffer.Position;
            Buffer.Position = index;
            WriteByte((byte) writeValue);
            Buffer.Position = oldIndex;
        }

        /// <summary>
        /// Sets a byte array in the stream
        /// </summary>
        /// <param name="index">The index of the stream to set data at</param>
        /// <param name="writeValue">The bytes to set</param>
        public void SetBytes(long index, byte[] writeValue)
        {
            var oldIndex = Buffer.Position;
            Buffer.Position = index;
            WriteBytes(writeValue);
            Buffer.Position = oldIndex;
        }

        /// <summary>
        /// Sets a bool in the stream
        /// </summary>
        /// <param name="index">The index of the stream to set data at</param>
        /// <param name="writeValue">The bool to set</param>
        public void SetBool(long index, bool writeValue)
        {
            var oldIndex = Buffer.Position;
            Buffer.Position = index;
            WriteBool(writeValue);
            Buffer.Position = oldIndex;
        }

        /// <summary>
        /// Sets a short in the stream
        /// </summary>
        /// <param name="index">The index of the stream to set data at</param>
        /// <param name="writeValue">The short to set</param>
        public void SetShort(long index, int writeValue)
        {
            var oldIndex = Buffer.Position;
            Buffer.Position = index;
            WriteShort((short) writeValue);
            Buffer.Position = oldIndex;
        }

        /// <summary>
        /// Sets an int in the stream
        /// </summary>
        /// <param name="index">The index of the stream to set data at</param>
        /// <param name="writeValue">The int to set</param>
        public void SetInt(long index, int writeValue)
        {
            var oldIndex = Buffer.Position;
            Buffer.Position = index;
            WriteInt(writeValue);
            Buffer.Position = oldIndex;
        }

        /// <summary>
        /// Sets a long in the stream
        /// </summary>
        /// <param name="index">The index of the stream to set data at</param>
        /// <param name="writeValue">The long to set</param>
        public void SetLong(long index, long writeValue)
        {
            var oldIndex = Buffer.Position;
            Buffer.Position = index;
            WriteLong(writeValue);
            Buffer.Position = oldIndex;
        }

        /// <summary>
        /// Sets a long in the stream
        /// </summary>
        /// <param name="index">The index of the stream to set data at</param>
        /// <param name="writeValue">The long to set</param>
        public void SetString(long index, string writeValue)
        {
            var oldIndex = Buffer.Position;
            Buffer.Position = index;
            WriteString(writeValue);
            Buffer.Position = oldIndex;
        }

        /// <summary>
        /// Sets a string prefixed with a [short] length before it, in the stream
        /// </summary>
        /// <param name="index">The index of the stream to set data at</param>
        /// <param name="writeValue">The string to set</param>
        public void SetMapleString(long index, string writeValue)
        {
            var oldIndex = Buffer.Position;
            Buffer.Position = index;
            WriteMapleString(writeValue);
            Buffer.Position = oldIndex;
        }

        /// <summary>
        /// Sets a hex-string in the stream
        /// </summary>
        /// <param name="index">The index of the stream to set data at</param>
        /// <param name="writeValue">The hex-string to set</param>
        public void SetHexString(long index, string writeValue)
        {
            var oldIndex = Buffer.Position;
            Buffer.Position = index;
            WriteHexString(writeValue);
            Buffer.Position = oldIndex;
        }
    }
}