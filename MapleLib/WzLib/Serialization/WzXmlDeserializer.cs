using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.Globalization;
using System.IO;
using System.Xml;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.WzLib.Serialization
{
    public class WzXmlDeserializer : ProgressingWzSerializer
    {
        public static NumberFormatInfo formattingInfo;

        private bool useMemorySaving;
        private byte[] iv;
        private WzImgDeserializer imgDeserializer = new WzImgDeserializer(false);

        public WzXmlDeserializer(bool useMemorySaving, byte[] iv)
        {
            this.useMemorySaving = useMemorySaving;
            this.iv = iv;
        }

        #region Public Functions

        public List<WzObject> ParseXML(string path)
        {
            var result = new List<WzObject>();
            var doc = new XmlDocument();
            doc.Load(path);
            var mainElement = (XmlElement) doc.ChildNodes[1];
            curr = 0;
            if (mainElement.Name == "xmldump")
            {
                total = CountImgs(mainElement);
                foreach (XmlElement subelement in mainElement)
                {
                    if (subelement.Name == "wzdir")
                    {
                        {
                            result.Add(ParseXMLWzDir(subelement));
                        }
                    }
                    else if (subelement.Name == "wzimg")
                    {
                        {
                            result.Add(ParseXMLWzImg(subelement));
                        }
                    }
                    else
                    {
                        {
                            throw new InvalidDataException("unknown XML prop " + subelement.Name);
                        }
                    }
                }
            }
            else if (mainElement.Name == "imgdir")
            {
                total = 1;
                result.Add(ParseXMLWzImg(mainElement));
                curr++;
            }
            else
            {
                {
                    throw new InvalidDataException("unknown main XML prop " + mainElement.Name);
                }
            }

            return result;
        }

        #endregion

        #region Internal Functions

        internal static int CountImgs(XmlElement element)
        {
            var result = 0;
            foreach (XmlElement subelement in element)
            {
                if (subelement.Name == "wzimg")
                {
                    {
                        result++;
                    }
                }
                else if (subelement.Name == "wzdir")
                {
                    {
                        result += CountImgs(subelement);
                    }
                }
            }

            return result;
        }

        internal WzDirectory ParseXMLWzDir(XmlElement dirElement)
        {
            var result = new WzDirectory(dirElement.GetAttribute("name"));
            foreach (XmlElement subelement in dirElement)
            {
                if (subelement.Name == "wzdir")
                {
                    {
                        result.AddDirectory(ParseXMLWzDir(subelement));
                    }
                }
                else if (subelement.Name == "wzimg")
                {
                    {
                        result.AddImage(ParseXMLWzImg(subelement));
                    }
                }
                else
                {
                    {
                        throw new InvalidDataException("unknown XML prop " + subelement.Name);
                    }
                }
            }

            return result;
        }

        internal WzImage ParseXMLWzImg(XmlElement imgElement)
        {
            var name = imgElement.GetAttribute("name");
            var result = new WzImage(name);
            foreach (XmlElement subelement in imgElement)
            {
                {
                    result.Properties.AddProperty(ParsePropertyFromXMLElement(subelement));
                }
            }

            result.Changed = true;
            if (useMemorySaving)
            {
                var path = Path.GetTempFileName();
                var wzWriter = new WzBinaryWriter(File.Create(path), iv);
                result.SaveImage(wzWriter);
                wzWriter.Close();
                result.Dispose();
                result = imgDeserializer.WzImageFromIMGFile(path, iv, name);
            }

            return result;
        }

        internal static WzImageProperty ParsePropertyFromXMLElement(XmlElement element)
        {
            switch (element.Name)
            {
                case "imgdir":
                    var sub = new WzSubProperty(element.GetAttribute("name"));
                    foreach (XmlElement subelement in element)
                    {
                        {
                            sub.AddProperty(ParsePropertyFromXMLElement(subelement));
                        }
                    }

                    return sub;

                case "canvas":
                    var canvas = new WzCanvasProperty(element.GetAttribute("name"));
                    if (!element.HasAttribute("basedata"))
                    {
                        {
                            throw new NoBase64DataException("no base64 data in canvas element with name " +
                                                            canvas.Name);
                        }
                    }

                    canvas.PngProperty = new WzPngProperty();
                    var pngstream = new MemoryStream(Convert.FromBase64String(element.GetAttribute("basedata")));
                    canvas.PngProperty.SetPNG((Bitmap) Image.FromStream(pngstream));
                    foreach (XmlElement subelement in element)
                    {
                        {
                            canvas.AddProperty(ParsePropertyFromXMLElement(subelement));
                        }
                    }

                    return canvas;

                case "int":
                    var compressedInt = new WzIntProperty(element.GetAttribute("name"),
                        int.Parse(element.GetAttribute("value"), formattingInfo));
                    return compressedInt;

                case "double":
                    var doubleProp = new WzDoubleProperty(element.GetAttribute("name"),
                        double.Parse(element.GetAttribute("value"), formattingInfo));
                    return doubleProp;

                case "null":
                    var nullProp = new WzNullProperty(element.GetAttribute("name"));
                    return nullProp;

                case "sound":
                    if (!element.HasAttribute("basedata") || !element.HasAttribute("basehead") ||
                        !element.HasAttribute("length"))
                    {
                        {
                            throw new NoBase64DataException("no base64 data in sound element with name " +
                                                            element.GetAttribute("name"));
                        }
                    }

                    var sound = new WzSoundProperty(element.GetAttribute("name"),
                        int.Parse(element.GetAttribute("length")),
                        Convert.FromBase64String(element.GetAttribute("basehead")),
                        Convert.FromBase64String(element.GetAttribute("basedata")));
                    return sound;

                case "string":
                    var stringProp = new WzStringProperty(element.GetAttribute("name"), element.GetAttribute("value"));
                    return stringProp;

                case "short":
                    var shortProp = new WzShortProperty(element.GetAttribute("name"),
                        short.Parse(element.GetAttribute("value"), formattingInfo));
                    return shortProp;

                case "long":
                    var longProp = new WzLongProperty(element.GetAttribute("name"),
                        long.Parse(element.GetAttribute("value"), formattingInfo));
                    return longProp;

                case "uol":
                    var uol = new WzUOLProperty(element.GetAttribute("name"), element.GetAttribute("value"));
                    return uol;

                case "vector":
                    var vector = new WzVectorProperty(element.GetAttribute("name"),
                        new WzIntProperty("x", Convert.ToInt32(element.GetAttribute("x"))),
                        new WzIntProperty("y", Convert.ToInt32(element.GetAttribute("y"))));
                    return vector;

                case "float":
                    var floatProp = new WzFloatProperty(element.GetAttribute("name"),
                        float.Parse(element.GetAttribute("value"), formattingInfo));
                    return floatProp;

                case "extended":
                    var convex = new WzConvexProperty(element.GetAttribute("name"));
                    foreach (XmlElement subelement in element)
                    {
                        {
                            convex.AddProperty(ParsePropertyFromXMLElement(subelement));
                        }
                    }

                    return convex;
            }

            throw new InvalidDataException("unknown XML prop " + element.Name);
        }

        #endregion
    }
}