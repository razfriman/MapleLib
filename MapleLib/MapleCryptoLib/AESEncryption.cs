using System;
using System.IO;
using System.Security.Cryptography;
using MapleLib.Helper;
using Microsoft.Extensions.Logging;

namespace MapleLib.MapleCryptoLib
{

    /// <summary>
    /// Class to handle the AES Encryption routines
    /// </summary>
    public static class AESEncryption
    {
        public static ILogger Log = LogManager.Log;

        /// <summary>
        /// Encrypt data using MapleStory's AES algorithm
        /// </summary>
        /// <param name="iv">IV to use for encryption</param>
        /// <param name="data">Data to encrypt</param>
        /// <param name="length">Length of data</param>
        /// <returns>Crypted data</returns>
        public static byte[] AesCrypt(byte[] iv, byte[] data, int length) => AesCrypt(iv, data, length, CryptoConstants.TrimmedUserKey);

        /// <summary>
        /// Encrypt data using MapleStory's AES method
        /// </summary>
        /// <param name="iv">IV to use for encryption</param>
        /// <param name="data">data to encrypt</param>
        /// <param name="length">length of data</param>
        /// <param name="key">the AES key to use</param>
        /// <returns>Crypted data</returns>
        public static byte[] AesCrypt(byte[] iv, byte[] data, int length, byte[] key)
        {
            var crypto = new AesManaged
            {
                KeySize = 256, //in bits
                Key = key,
                Mode = CipherMode.ECB // Should be OFB, but this works too
            };

            using (MemoryStream memStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memStream, crypto.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    int remaining = length;
                    int llength = 0x5B0;
                    int start = 0;

                    while (remaining > 0)
                    {
                        byte[] myIV = MapleCrypto.MultiplyBytes(iv, 4, 4);
                        if (remaining < llength)
                        {
                            llength = remaining;
                        }

                        for (int x = start; x < (start + llength); x++)
                        {
                            if ((x - start) % myIV.Length == 0)
                            {
                                cryptoStream.Write(myIV, 0, myIV.Length);
                                byte[] newIV = memStream.ToArray();
                                Array.Copy(newIV, myIV, myIV.Length);
                                memStream.Position = 0;
                            }
                            data[x] ^= myIV[(x - start) % myIV.Length];
                        }
                        start += llength;
                        remaining -= llength;
                        llength = 0x5B4;
                    }
                }
            }

            return data;
        }
    }
}