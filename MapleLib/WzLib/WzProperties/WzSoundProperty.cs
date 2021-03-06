﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using MapleLib.Helper;
using MapleLib.WzLib.Util;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace MapleLib.WzLib.WzProperties
{
    /// <summary>
    /// A property that contains data for an MP3 file
    /// </summary>
    public class WzSoundProperty : WzExtended
    {
        public static ILogger Log = LogManager.Log;

        #region Fields

        internal string name;
        internal byte[] mp3bytes;
        internal WzObject parent;
        internal int len_ms;

        internal byte[] header;

        //internal WzImage imgParent;
        internal WzBinaryReader wzReader;
        internal bool headerEncrypted;
        internal long offs;
        internal int soundDataLen;

        public static readonly byte[] soundHeader =
        {
            0x02,
            0x83, 0xEB, 0x36, 0xE4, 0x4F, 0x52, 0xCE, 0x11, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70,
            0x8B, 0xEB, 0x36, 0xE4, 0x4F, 0x52, 0xCE, 0x11, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70,
            0x00,
            0x01,
            0x81, 0x9F, 0x58, 0x05, 0x56, 0xC3, 0xCE, 0x11, 0xBF, 0x01, 0x00, 0xAA, 0x00, 0x55, 0x59, 0x5A
        };

        internal WaveFormat wavFormat;

        #endregion

        #region Inherited Members

        public override WzImageProperty DeepClone()
        {
            var clone = new WzSoundProperty(name, len_ms, header, mp3bytes);
            return clone;
        }

        public override object WzValue => GetBytes(false);

        public override void SetValue(object value)
        {
        }

        /// <summary>
        /// The parent of the object
        /// </summary>
        public override WzObject Parent
        {
            get => parent;
            internal set => parent = value;
        }

        /*/// <summary>
		/// The image that this property is contained in
		/// </summary>
		public override WzImage ParentImage { get { return imgParent; } internal set { imgParent = value; } }*/
        /// <summary>
        /// The name of the property
        /// </summary>
        public override string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType => WzPropertyType.Sound;

        public override void WriteValue(WzBinaryWriter writer)
        {
            var data = GetBytes(false);
            writer.WriteStringValue("Sound_DX8", 0x73, 0x1B);
            writer.Write((byte) 0);
            writer.WriteCompressedInt(data.Length);
            writer.WriteCompressedInt(len_ms);
            writer.Write(header);
            writer.Write(data);
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            name = null;
            mp3bytes = null;
        }

        #endregion

        #region Custom Members

        /// <summary>
        /// The data of the mp3 header
        /// </summary>
        public byte[] Header
        {
            get => header;
            set => header = value;
        }

        /// <summary>
        /// Length of the mp3 file in milliseconds
        /// </summary>
        public int Length
        {
            get => len_ms;
            set => len_ms = value;
        }

        /// <summary>
        /// Frequency of the mp3 file in Hz
        /// </summary>
        public int Frequency => wavFormat?.SampleRate ?? 0;

        /// <summary>
        /// BPS of the mp3 file
        /// </summary>
        //public byte BPS { get { return bps; } set { bps = value; } }
        /// <summary>
        /// Creates a WzSoundProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="reader">The wz reader</param>
        /// <param name="parseNow">Indicating whether to parse the property now</param>
        public WzSoundProperty(string name, WzBinaryReader reader, bool parseNow)
        {
            this.name = name;
            wzReader = reader;
            reader.BaseStream.Position++;

            //note - soundDataLen does NOT include the length of the header.
            soundDataLen = reader.ReadCompressedInt();
            len_ms = reader.ReadCompressedInt();

            var headerOff = reader.BaseStream.Position;
            reader.BaseStream.Position += soundHeader.Length; //skip GUIDs
            int wavFormatLen = reader.ReadByte();
            reader.BaseStream.Position = headerOff;

            header = reader.ReadBytes(soundHeader.Length + 1 + wavFormatLen);
            ParseHeader();

            //sound file offs
            offs = reader.BaseStream.Position;
            if (parseNow)
            {
                mp3bytes = reader.ReadBytes(soundDataLen);
            }
            else
            {
                reader.BaseStream.Position += soundDataLen;
            }
        }

        /*public WzSoundProperty(string name)
        {
            this.name = name;
            this.len_ms = 0;
            this.header = null;
            this.mp3bytes = null;
        }*/

        /// <summary>
        /// Creates a WzSoundProperty with the specified name and data
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="len_ms">The sound length</param>
        /// <param name="header">The sound header</param>
        /// <param name="data">The sound data</param>
        public WzSoundProperty(string name, int len_ms, byte[] header, byte[] data)
        {
            this.name = name;
            this.len_ms = len_ms;
            this.header = header;
            mp3bytes = data;
            ParseHeader();
        }

        /// <summary>
        /// Creates a WzSoundProperty with the specified name from a file
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="file">The path to the sound file</param>
        public WzSoundProperty(string name, string file)
        {
            this.name = name;
            var reader = new Mp3FileReader(file);
            wavFormat = reader.Mp3WaveFormat;
            len_ms = (int) (reader.Length * 1000d / reader.WaveFormat.AverageBytesPerSecond);
            RebuildHeader();
            reader.Dispose();
            mp3bytes = File.ReadAllBytes(file);
        }

        public static bool Memcmp(byte[] a, byte[] b, int n)
        {
            for (var i = 0; i < n; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 3);
            foreach (var b in ba)
            {
                hex.AppendFormat("{0:x2} ", b);
            }

            return hex.ToString();
        }

        public void RebuildHeader()
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(soundHeader);
                var wavHeader = StructToBytes(wavFormat);
                if (headerEncrypted)
                {
                    for (var i = 0; i < wavHeader.Length; i++)
                    {
                        wavHeader[i] ^= wzReader.WzKey[i];
                    }
                }

                bw.Write((byte) wavHeader.Length);
                bw.Write(wavHeader, 0, wavHeader.Length);
                header = ((MemoryStream) bw.BaseStream).ToArray();
            }
        }

        private static byte[] StructToBytes<T>(T obj)
        {
            var result = new byte[Marshal.SizeOf(obj)];
            var handle = GCHandle.Alloc(result, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(obj, handle.AddrOfPinnedObject(), false);
                return result;
            }
            finally
            {
                handle.Free();
            }
        }

        private static T BytesToStruct<T>(byte[] data) where T : new()
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        private static T BytesToStructConstructorless<T>(byte[] data)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var obj = (T) FormatterServices.GetUninitializedObject(typeof(T));
                Marshal.PtrToStructure(handle.AddrOfPinnedObject(), obj);
                return obj;
            }
            finally
            {
                handle.Free();
            }
        }

        private void ParseHeader()
        {
            var wavHeader = new byte[header.Length - soundHeader.Length - 1];
            Buffer.BlockCopy(header, soundHeader.Length + 1, wavHeader, 0, wavHeader.Length);

            if (wavHeader.Length < Marshal.SizeOf<WaveFormat>())
            {
                return;
            }

            var wavFmt = BytesToStruct<WaveFormat>(wavHeader);

            if (Marshal.SizeOf<WaveFormat>() + wavFmt.ExtraSize != wavHeader.Length)
            {
                //try decrypt
                for (var i = 0; i < wavHeader.Length; i++)
                {
                    wavHeader[i] ^= wzReader.WzKey[i];
                }

                wavFmt = BytesToStruct<WaveFormat>(wavHeader);

                if (Marshal.SizeOf<WaveFormat>() + wavFmt.ExtraSize != wavHeader.Length)
                {
                    Log.LogCritical("Failed to parse sound header");
                    return;
                }

                headerEncrypted = true;
            }

            // parse to mp3 header
            if (wavFmt.Encoding == WaveFormatEncoding.MpegLayer3 && wavHeader.Length >= Marshal.SizeOf<Mp3WaveFormat>())
            {
                wavFormat = BytesToStructConstructorless<Mp3WaveFormat>(wavHeader);
            }
            else if (wavFmt.Encoding == WaveFormatEncoding.Pcm)
            {
                wavFormat = wavFmt;
            }
            else
            {
                Log.LogError($"Unknown wave encoding: {wavFmt.Encoding}");
            }
        }

        #endregion

        #region Parsing Methods

        public byte[] GetBytes(bool saveInMemory)
        {
            if (mp3bytes != null)
            {
                return mp3bytes;
            }

            if (wzReader == null)
            {
                return null;
            }

            var currentPos = wzReader.BaseStream.Position;
            wzReader.BaseStream.Position = offs;
            mp3bytes = wzReader.ReadBytes(soundDataLen);
            wzReader.BaseStream.Position = currentPos;

            if (saveInMemory)
            {
                return mp3bytes;
            }

            var result = mp3bytes;
            mp3bytes = null;
            return result;
        }

        public void SaveToFile(string file)
        {
            File.WriteAllBytes(file, GetBytes(false));
        }

        #endregion

        #region Cast Values

        public override byte[] GetBytes()
        {
            return GetBytes(false);
        }

        #endregion
    }
}