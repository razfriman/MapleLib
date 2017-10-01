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
        /// <param name="pData">data to encrypt</param>
        /// <returns>Encrypted data</returns>
        public static void Encrypt(byte[] pData)
        {
            int size = pData.Length;
            int j;
            byte a, c;
            for (int i = 0; i < 3; i++)
            {
                a = 0;
                for (j = size; j > 0; j--)
                {
                    c = pData[size - j];
                    c = Rol(c, 3);
                    c = (byte)(c + j);
                    c ^= a;
                    a = c;
                    c = Ror(a, j);
                    c ^= 0xFF;
                    c += 0x48;
                    pData[size - j] = c;
                }
                a = 0;
                for (j = pData.Length; j > 0; j--)
                {
                    c = pData[j - 1];
                    c = Rol(c, 4);
                    c = (byte)(c + j);
                    c ^= a;
                    a = c;
                    c ^= 0x13;
                    c = Ror(c, 3);
                    pData[j - 1] = c;
                }
            }
        }

        /// <summary>
        /// Decrypt data using MapleStory's Custom Encryption
        /// </summary>
        /// <param name="pData">data to decrypt</param>
        /// <returns>Decrypted data</returns>
        public static void Decrypt(byte[] pData)
        {
            int size = pData.Length;
            int j;
            byte a, b, c;
            for (int i = 0; i < 3; i++)
            {
                a = 0;
                b = 0;
                for (j = size; j > 0; j--)
                {
                    c = pData[j - 1];
                    c = Rol(c, 3);
                    c ^= 0x13;
                    a = c;
                    c ^= b;
                    c = (byte)(c - j); // Guess this is supposed to be right?
                    c = Ror(c, 4);
                    b = a;
                    pData[j - 1] = c;
                }
                a = 0;
                b = 0;
                for (j = size; j > 0; j--)
                {
                    c = pData[size - j];
                    c -= 0x48;
                    c ^= 0xFF;
                    c = Rol(c, j);
                    a = c;
                    c ^= b;
                    c = (byte)(c - j); // Guess this is supposed to be right?
                    c = Ror(c, 3);
                    b = a;
                    pData[size - j] = c;
                }
            }
        }

        /// <summary>
        /// Rolls a byte left
        /// </summary>
        /// <param name="pVal">input byte to roll</param>
        /// <param name="pNum">amount of bits to roll</param>
        /// <returns>The left rolled byte</returns>
        public static byte Rol(byte pVal, int pNum)
        {
            int highbit;
            for (int i = 0; i < pNum; i++)
            {
                highbit = ((pVal & 0x80) != 0 ? 1 : 0);
                pVal <<= 1;
                pVal |= (byte)highbit;
            }
            return pVal;
        }

        /// <summary>
        /// Rolls a byte right
        /// </summary>
        /// <param name="pVal">input byte to roll</param>
        /// <param name="pNum">amount of bits to roll</param>
        /// <returns>The right rolled byte</returns>
        public static byte Ror(byte pVal, int pNum)
        {
            int lowbit;
            for (int i = 0; i < pNum; i++)
            {
                lowbit = ((pVal & 1) != 0 ? 1 : 0);
                pVal >>= 1;
                pVal |= (byte)(lowbit << 7);
            }
            return pVal;
        }
    }
}