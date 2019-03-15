using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.Serialization
{
    public class WzClassicXmlSerializer : WzXmlSerializer, IWzImageSerializer
    {
        public WzClassicXmlSerializer(int indentation, LineBreak lineBreakType, bool exportbase64)
            : base(indentation, lineBreakType)
        {
            ExportBase64Data = exportbase64;
        }

        private void ExportXmlInternal(WzImage img, string path)
        {
            var parsed = img.Parsed || img.Changed;
            if (!parsed)
            {
                {
                    img.ParseImage();
                }
            }

            Current++;
            TextWriter tw = new StreamWriter(path);
            tw.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" + lineBreak);
            tw.Write($"<imgdir name=\"{XmlUtil.SanitizeText(img.Name)}\">{lineBreak}");
            foreach (var property in img.Properties.WzProperties)
            {
                {
                    WritePropertyToXML(tw, indent, property);
                }
            }

            tw.Write($"</imgdir>{lineBreak}");
            tw.Close();
            if (!parsed)
            {
                {
                    img.UnparseImage();
                }
            }
        }

        private void ExportDirXmlInternal(WzDirectory dir, string path)
        {
            Directory.CreateDirectory(path);

            if (path.Substring(path.Length - 1) != @"\")
            {
                {
                    path += @"\";
                }
            }

            foreach (var subdir in dir.WzDirectories)
            {
                {
                    ExportDirXmlInternal(subdir, path + subdir.name + @"\");
                }
            }

            foreach (var subimg in dir.WzImages)
            {
                {
                    ExportXmlInternal(subimg, path + subimg.Name + ".xml");
                }
            }
        }

        public void SerializeImage(WzImage img, string path)
        {
            Total = 1;
            Current = 0;
            if (Path.GetExtension(path) != ".xml")
            {
                {
                    path += ".xml";
                }
            }

            ExportXmlInternal(img, path);
        }

        public void SerializeDirectory(WzDirectory dir, string path)
        {
            Total = dir.CountImages();
            Current = 0;
            ExportDirXmlInternal(dir, path);
        }

        public void SerializeFile(WzFile file, string path)
        {
            SerializeDirectory(file.WzDirectory, path);
        }
    }
}