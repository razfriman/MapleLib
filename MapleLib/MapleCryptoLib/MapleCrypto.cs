﻿using System;

namespace MapleLib.MapleCryptoLib
{
    /// <summary>
    /// Class to manage Encryption and IV generation
    /// </summary>
    public class MapleCrypto
    {
        /// <summary>
        /// Version of MapleStory used in encryption
        /// </summary>
        private short _mapleVersion;

        /// <summary>
        /// (public) IV used in the packet encryption
        /// </summary>
        public byte[] IV { get; set; }

        /// <summary>
        /// Creates a new MapleCrypto class
        /// </summary>
        /// <param name="iv">Intializing Vector</param>
        /// <param name="mapleVersion">Version of MapleStory</param>
        public MapleCrypto(byte[] iv, short mapleVersion)
        {
            IV = iv;
            _mapleVersion = mapleVersion;
        }

        /// <summary>
        /// Updates the current IV
        /// </summary>
        public void UpdateIV() => IV = GetNewIV(IV);

        /// <summary>
        /// Encrypts data with AES and updates the IV
        /// </summary>
        /// <param name="data">The data to crypt</param>
        public void Crypt(byte[] data)
        {
            AesEncryption.AesCrypt(IV, data, data.Length);
            UpdateIV();
        }

        /// <summary>
        /// Generates a new IV
        /// </summary>
        /// <param name="oldIv">The Old IV used to generate the new IV</param>
        /// <returns>A new IV</returns>
        public static byte[] GetNewIV(byte[] oldIv)
        {
            byte[] start = {0xf2, 0x53, 0x50, 0xc6};
            for (var i = 0; i < 4; i++)
            {
                Shuffle(oldIv[i], start);
            }

            return start;
        }

        /// <summary>
        /// Shuffle the bytes in the IV
        /// </summary>
        /// <param name="inputByte">Byte of the old IV</param>
        /// <param name="start">The Default AES Key</param>
        /// <returns>The shuffled bytes</returns>
        public static byte[] Shuffle(byte inputByte, byte[] start)
        {
            var a = start[1];
            var b = a;
            uint c, d;
            b = CryptoConstants.ShuffleBytes[b];
            b -= inputByte;
            start[0] += b;
            b = start[2];
            b ^= CryptoConstants.ShuffleBytes[inputByte];
            a -= b;
            start[1] = a;
            a = start[3];
            b = a;
            a -= start[0];
            b = CryptoConstants.ShuffleBytes[b];
            b += inputByte;
            b ^= start[2];
            start[2] = b;
            a += CryptoConstants.ShuffleBytes[inputByte];
            start[3] = a;

            c = (uint) (start[0] + start[1] * 0x100 + start[2] * 0x10000 + start[3] * 0x1000000);
            d = c;
            c >>= 0x1D;
            d <<= 0x03;
            c |= d;
            start[0] = (byte) (c % 0x100);
            c /= 0x100;
            start[1] = (byte) (c % 0x100);
            c /= 0x100;
            start[2] = (byte) (c % 0x100);
            start[3] = (byte) (c / 0x100);

            return start;
        }

        /// <summary>
        /// Get a packet header for a packet being sent to the server
        /// </summary>
        /// <param name="size">Size of the packet</param>
        /// <returns>The packet header</returns>
        public byte[] GetHeaderToClient(int size)
        {
            var header = new byte[4];
            var a = IV[3] * 0x100 + IV[2];
            a ^= -(_mapleVersion + 1);
            var b = a ^ size;
            header[0] = (byte) (a % 0x100);
            header[1] = (byte) ((a - header[0]) / 0x100);
            header[2] = (byte) (b ^ 0x100);
            header[3] = (byte) ((b - header[2]) / 0x100);
            return header;
        }

        /// <summary>
        /// Get a packet header for a packet being sent to the client
        /// </summary>
        /// <param name="size">Size of the packet</param>
        /// <returns>The packet header</returns>
        public byte[] GetHeaderToServer(int size)
        {
            var header = new byte[4];
            var a = IV[3] * 0x100 + IV[2];
            a = a ^ _mapleVersion;
            var b = a ^ size;
            header[0] = Convert.ToByte(a % 0x100);
            header[1] = Convert.ToByte(a / 0x100);
            header[2] = Convert.ToByte(b % 0x100);
            header[3] = Convert.ToByte(b / 0x100);
            return header;
        }

        /// <summary>
        /// Gets the length of a packet from the header
        /// </summary>
        /// <param name="packetHeader">Header of the packet</param>
        /// <returns>The length of the packet</returns>
        public static int GetPacketLength(int packetHeader) => GetPacketLength(BitConverter.GetBytes(packetHeader));

        /// <summary>
        /// Gets the length of a packet from the header
        /// </summary>
        /// <param name="packetHeader">Header of the packet</param>
        /// <returns>The length of the packet</returns>
        public static int GetPacketLength(byte[] packetHeader)
        {
            if (packetHeader.Length < 4)
            {
                return -1;
            }

            return (packetHeader[0] + (packetHeader[1] << 8)) ^ (packetHeader[2] + (packetHeader[3] << 8));
        }

        /// <summary>
        /// Checks to make sure the packet is a valid MapleStory packet
        /// </summary>
        /// <returns>The packet is valid</returns>
        public bool CheckPacketToServer(byte[] packet)
        {
            var a = packet[0] ^ IV[2];
            int b = _mapleVersion;
            var c = packet[1] ^ IV[3];
            var d = _mapleVersion >> 8;
            return a == b && c == d;
        }

        /// <summary>
        /// Multiplies bytes
        /// </summary>
        /// <param name="input">Bytes to multiply</param>
        /// <param name="count">Amount of bytes to repeat</param>
        /// <param name="mult">Times to repeat the packet</param>
        /// <returns>The multiplied bytes</returns>
        public static byte[] MultiplyBytes(byte[] input, int count, int mult)
        {
            var ret = new byte[count * mult];
            for (var x = 0; x < ret.Length; x++)
            {
                ret[x] = input[x % count];
            }

            return ret;
        }
    }
}