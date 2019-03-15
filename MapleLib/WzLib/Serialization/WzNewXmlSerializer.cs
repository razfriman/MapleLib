using System.Collections.Generic;
using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.Serialization
{
    public class WzNewXmlSerializer : WzXmlSerializer
    {
        public WzNewXmlSerializer(int indentation, LineBreak lineBreakType)
            : base(indentation, lineBreakType)
        {
        }

        internal void DumpImageToXML(TextWriter tw, string depth, WzImage img)
        {
            var parsed = img.Parsed || img.Changed;
            if (!parsed)
            {
                {
                    img.ParseImage();
                }
            }

            Current++;
            tw.Write($"{depth}<wzimg name=\"{XmlUtil.SanitizeText(img.Name)}\">{lineBreak}");
            var newDepth = depth + indent;
            foreach (var property in img.Properties.WzProperties)
            {
                {
                    WritePropertyToXML(tw, newDepth, property);
                }
            }

            tw.Write($"{depth}</wzimg>");
            if (!parsed)
            {
                {
                    img.UnparseImage();
                }
            }
        }

        internal void DumpDirectoryToXML(TextWriter tw, string depth, WzDirectory dir)
        {
            tw.Write(depth + "<wzdir name=\"" + XmlUtil.SanitizeText(dir.Name) + "\">" + lineBreak);
            foreach (var subdir in dir.WzDirectories)
            {
                {
                    DumpDirectoryToXML(tw, depth + indent, subdir);
                }
            }

            foreach (var img in dir.WzImages)
            {
                {
                    DumpImageToXML(tw, depth + indent, img);
                }
            }

            tw.Write(depth + "</wzdir>" + lineBreak);
        }

        public void ExportCombinedXml(List<WzObject> objects, string path)
        {
            Total = 1;
            Current = 0;
            if (Path.GetExtension(path) != ".xml")
            {
                {
                    path += ".xml";
                }
            }

            foreach (var obj in objects)
            {
                if (obj is WzImage)
                {
                    {
                        Total++;
                    }
                }
                else if (obj is WzDirectory directory)
                {
                    {
                        Total += directory.CountImages();
                    }
                }
            }

            ExportBase64Data = true;
            TextWriter tw = new StreamWriter(path);
            tw.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" + lineBreak);
            tw.Write("<xmldump>" + lineBreak);
            foreach (var obj in objects)
            {
                if (obj is WzDirectory)
                {
                    {
                        DumpDirectoryToXML(tw, indent, (WzDirectory) obj);
                    }
                }
                else if (obj is WzImage)
                {
                    {
                        DumpImageToXML(tw, indent, (WzImage) obj);
                    }
                }
                else if (obj is WzImageProperty)
                {
                    {
                        WritePropertyToXML(tw, indent, (WzImageProperty) obj);
                    }
                }
            }

            tw.Write("</xmldump>" + lineBreak);
            tw.Close();
        }
    }
}