using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.Serialization
{
    public class WzImgSerializer : ProgressingWzSerializer, IWzImageSerializer
    {
        private static byte[] SerializeImageInternal(WzImage img)
        {
            var stream = new MemoryStream();
            var wzWriter = new WzBinaryWriter(stream, ((WzDirectory) img.parent).WzIv);
            img.SaveImage(wzWriter);
            var result = stream.ToArray();
            wzWriter.Close();
            return result;
        }

        private static void SerializeImageInternal(WzImage img, string outPath)
        {
            var stream = File.Create(outPath);
            var wzWriter = new WzBinaryWriter(stream, ((WzDirectory) img.parent).WzIv);
            img.SaveImage(wzWriter);
            wzWriter.Close();
        }

        public byte[] SerializeImage(WzImage img)
        {
            Total = 1;
            Current = 0;
            return SerializeImageInternal(img);
        }

        public void SerializeImage(WzImage img, string outPath)
        {
            Total = 1;
            Current = 0;
            if (Path.GetExtension(outPath) != ".img")
            {
                {
                    outPath += ".img";
                }
            }

            SerializeImageInternal(img, outPath);
        }

        public void SerializeDirectory(WzDirectory dir, string outPath)
        {
            Total = dir.CountImages();
            Current = 0;
            Directory.CreateDirectory(outPath);

            if (outPath.Substring(outPath.Length - 1, 1) != @"\")
            {
                {
                    outPath += @"\";
                }
            }

            foreach (var subdir in dir.WzDirectories)
            {
                {
                    SerializeDirectory(subdir, outPath + subdir.Name + @"\");
                }
            }

            foreach (var img in dir.WzImages)
            {
                {
                    SerializeImage(img, outPath + img.Name);
                }
            }
        }

        public void SerializeFile(WzFile f, string outPath)
        {
            SerializeDirectory(f.WzDirectory, outPath);
        }
    }
}