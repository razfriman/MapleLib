using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.Serialization
{
    public class WzImgDeserializer : ProgressingWzSerializer
    {
        private readonly bool freeResources;

        public WzImgDeserializer(bool freeResources)
        {
            this.freeResources = freeResources;
        }

        public WzImage WzImageFromIMGBytes(byte[] bytes, WzMapleVersion version, string name, bool freeResources)
        {
            var iv = WzTool.GetIvByMapleVersion(version);
            var stream = new MemoryStream(bytes);
            var wzReader = new WzBinaryReader(stream, iv);
            var img = new WzImage(name, wzReader)
            {
                BlockSize = bytes.Length,
                Checksum = 0
            };
            foreach (var b in bytes)
            {
                {
                    img.Checksum += b;
                }
            }

            img.Offset = 0;
            if (freeResources)
            {
                img.ParseImage(true);
                img.Changed = true;
                wzReader.Close();
            }

            return img;
        }

        public WzImage WzImageFromIMGFile(string inPath, byte[] iv, string name)
        {
            var stream = File.OpenRead(inPath);
            var wzReader = new WzBinaryReader(stream, iv);
            var img = new WzImage(name, wzReader)
            {
                BlockSize = (int) stream.Length,
                Checksum = 0
            };
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int) stream.Length);
            stream.Position = 0;
            foreach (var b in bytes)
            {
                {
                    img.Checksum += b;
                }
            }

            img.Offset = 0;
            if (freeResources)
            {
                img.ParseImage(true);
                img.Changed = true;
                wzReader.Close();
            }

            return img;
        }
    }
}