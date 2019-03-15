using System;
using System.IO;
using System.Security.Cryptography;

namespace MapleLib.MapleCryptoLib
{
    /// <summary>
    /// Class to handle the AES Encryption routines
    /// </summary>
    public static class AesEncryption
    {
        /// <summary>
        /// Encrypt data using MapleStory's AES algorithm
        /// </summary>
        /// <param name="iv">IV to use for encryption</param>
        /// <param name="data">Data to encrypt</param>
        /// <param name="length">Length of data</param>
        /// <returns>Crypted data</returns>
        public static byte[] AesCrypt(byte[] iv, byte[] data, int length) =>
            AesCrypt(iv, data, length, CryptoConstants.TrimmedUserKey);

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
                KeySize = 256,
                Key = key,
                Mode = CipherMode.ECB // Should be OFB, but this works too
            };

            using (var memStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memStream, crypto.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    var remaining = length;
                    var size = 0x5B0;
                    var start = 0;

                    while (remaining > 0)
                    {
                        var myIv = MapleCrypto.MultiplyBytes(iv, 4, 4);
                        if (remaining < size)
                        {
                            size = remaining;
                        }

                        for (var x = start; x < start + size; x++)
                        {
                            if ((x - start) % myIv.Length == 0)
                            {
                                cryptoStream.Write(myIv, 0, myIv.Length);
                                var newIv = memStream.ToArray();
                                Array.Copy(newIv, myIv, myIv.Length);
                                memStream.Position = 0;
                            }

                            data[x] ^= myIv[(x - start) % myIv.Length];
                        }

                        start += size;
                        remaining -= size;
                        size = 0x5B4;
                    }
                }
            }

            return data;
        }
    }
}