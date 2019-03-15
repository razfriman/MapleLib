namespace MapleLib.MapleCryptoLib
{
    /// <summary>
    /// Class to handle the MapleStory Custom Encryption routines
    /// </summary>
    public static class MapleCustomEncryption
    {
        /// <summary>
        /// Encrypt data using MapleStory's Custom Encryption
        /// </summary>
        /// <param name="data">data to encrypt</param>
        /// <returns>Encrypted data</returns>
        public static void Encrypt(byte[] data)
        {
            var size = data.Length;
            for (var i = 0; i < 3; i++)
            {
                byte a = 0;
                int j;
                byte c;
                for (j = size; j > 0; j--)
                {
                    c = data[size - j];
                    c = Rol(c, 3);
                    c = (byte) (c + j);
                    c ^= a;
                    a = c;
                    c = Ror(a, j);
                    c ^= 0xFF;
                    c += 0x48;
                    data[size - j] = c;
                }

                a = 0;
                for (j = data.Length; j > 0; j--)
                {
                    c = data[j - 1];
                    c = Rol(c, 4);
                    c = (byte) (c + j);
                    c ^= a;
                    a = c;
                    c ^= 0x13;
                    c = Ror(c, 3);
                    data[j - 1] = c;
                }
            }
        }

        /// <summary>
        /// Decrypt data using MapleStory's Custom Encryption
        /// </summary>
        /// <param name="data">data to decrypt</param>
        /// <returns>Decrypted data</returns>
        public static void Decrypt(byte[] data)
        {
            var size = data.Length;
            for (var i = 0; i < 3; i++)
            {
                byte a;
                byte b = 0;
                byte c;
                int j;
                for (j = size; j > 0; j--)
                {
                    c = data[j - 1];
                    c = Rol(c, 3);
                    c ^= 0x13;
                    a = c;
                    c ^= b;
                    c = (byte) (c - j); // Guess this is supposed to be right?
                    c = Ror(c, 4);
                    b = a;
                    data[j - 1] = c;
                }

                b = 0;
                for (j = size; j > 0; j--)
                {
                    c = data[size - j];
                    c -= 0x48;
                    c ^= 0xFF;
                    c = Rol(c, j);
                    a = c;
                    c ^= b;
                    c = (byte) (c - j); // Guess this is supposed to be right?
                    c = Ror(c, 3);
                    b = a;
                    data[size - j] = c;
                }
            }
        }

        /// <summary>
        /// Rolls a byte left
        /// </summary>
        /// <param name="val">input byte to roll</param>
        /// <param name="num">amount of bits to roll</param>
        /// <returns>The left rolled byte</returns>
        public static byte Rol(byte val, int num)
        {
            for (var i = 0; i < num; i++)
            {
                var highbit = (val & 0x80) != 0 ? 1 : 0;
                val <<= 1;
                val |= (byte) highbit;
            }

            return val;
        }

        /// <summary>
        /// Rolls a byte right
        /// </summary>
        /// <param name="val">input byte to roll</param>
        /// <param name="num">amount of bits to roll</param>
        /// <returns>The right rolled byte</returns>
        public static byte Ror(byte val, int num)
        {
            for (var i = 0; i < num; i++)
            {
                var lowbit = (val & 1) != 0 ? 1 : 0;
                val >>= 1;
                val |= (byte) (lowbit << 7);
            }

            return val;
        }
    }
}