using System;
using System.DrawingCore.Imaging;
using System.Globalization;
using System.IO;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.WzLib.Serialization
{
    public abstract class WzXmlSerializer : ProgressingWzSerializer
    {
        protected string indent;
        protected string lineBreak;
        public static NumberFormatInfo formattingInfo;
        protected bool ExportBase64Data;

        protected static char[] amp = "&amp;".ToCharArray();
        protected static char[] lt = "&lt;".ToCharArray();
        protected static char[] gt = "&gt;".ToCharArray();
        protected static char[] apos = "&apos;".ToCharArray();
        protected static char[] quot = "&quot;".ToCharArray();

        static WzXmlSerializer()
        {
            formattingInfo = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberGroupSeparator = ","
            };
        }

        internal WzXmlSerializer(int indentation, LineBreak lineBreakType)
        {
            switch (lineBreakType)
            {
                case LineBreak.None:
                    lineBreak = "";
                    break;
                case LineBreak.Windows:
                    lineBreak = "\r\n";
                    break;
                case LineBreak.Unix:
                    lineBreak = "\n";
                    break;
            }

            var indentArray = new char[indentation];
            for (var i = 0; i < indentation; i++)
            {
                {
                    indentArray[i] = (char) 0x20;
                }
            }

            indent = new string(indentArray);
        }

        protected void WritePropertyToXML(TextWriter tw, string depth, WzImageProperty prop)
        {
            if (prop is WzCanvasProperty property3)
            {
                if (ExportBase64Data)
                {
                    var stream = new MemoryStream();
                    property3.PngProperty.GetPNG(false).Save(stream, ImageFormat.Png);
                    var pngbytes = stream.ToArray();
                    stream.Close();
                    tw.Write(string.Concat(depth, "<canvas name=\"", XmlUtil.SanitizeText(property3.Name),
                                 "\" width=\"", property3.PngProperty.Width, "\" height=\"",
                                 property3.PngProperty.Height, "\" basedata=\"", Convert.ToBase64String(pngbytes),
                                 "\">") + lineBreak);
                }
                else
                {
                    {
                        tw.Write(string.Concat(depth, "<canvas name=\"", XmlUtil.SanitizeText(property3.Name),
                                     "\" width=\"", property3.PngProperty.Width, "\" height=\"",
                                     property3.PngProperty.Height, "\">") + lineBreak);
                    }
                }

                var newDepth = depth + indent;
                foreach (var property in property3.WzProperties)
                {
                    {
                        WritePropertyToXML(tw, newDepth, property);
                    }
                }

                tw.Write(depth + "</canvas>" + lineBreak);
            }
            else if (prop is WzIntProperty property4)
            {
                tw.Write(string.Concat(depth, "<int name=\"", XmlUtil.SanitizeText(property4.Name), "\" value=\"",
                             property4.Value, "\"/>") + lineBreak);
            }
            else if (prop is WzDoubleProperty property5)
            {
                tw.Write(string.Concat(depth, "<double name=\"", XmlUtil.SanitizeText(property5.Name), "\" value=\"",
                             property5.Value, "\"/>") + lineBreak);
            }
            else if (prop is WzNullProperty property6)
            {
                tw.Write(depth + "<null name=\"" + XmlUtil.SanitizeText(property6.Name) + "\"/>" + lineBreak);
            }
            else if (prop is WzSoundProperty property7)
            {
                if (ExportBase64Data)
                {
                    {
                        tw.Write(string.Concat(new object[]
                        {
                            depth, "<sound name=\"", XmlUtil.SanitizeText(property7.Name), "\" length=\"",
                            property7.Length.ToString(), "\" basehead=\"", Convert.ToBase64String(property7.Header),
                            "\" basedata=\"", Convert.ToBase64String(property7.GetBytes(false)), "\"/>"
                        }) + lineBreak);
                    }
                }
                else
                {
                    {
                        tw.Write(depth + "<sound name=\"" + XmlUtil.SanitizeText(property7.Name) + "\"/>" + lineBreak);
                    }
                }
            }
            else if (prop is WzStringProperty property8)
            {
                var str = XmlUtil.SanitizeText(property8.Value);
                tw.Write(depth + "<string name=\"" + XmlUtil.SanitizeText(property8.Name) + "\" value=\"" + str +
                         "\"/>" + lineBreak);
            }
            else if (prop is WzSubProperty property9)
            {
                tw.Write(depth + "<imgdir name=\"" + XmlUtil.SanitizeText(property9.Name) + "\">" + lineBreak);
                var newDepth = depth + indent;
                foreach (var property in property9.WzProperties)
                {
                    {
                        WritePropertyToXML(tw, newDepth, property);
                    }
                }

                tw.Write(depth + "</imgdir>" + lineBreak);
            }
            else if (prop is WzShortProperty property10)
            {
                tw.Write(string.Concat(depth, "<short name=\"", XmlUtil.SanitizeText(property10.Name), "\" value=\"",
                             property10.Value, "\"/>") + lineBreak);
            }
            else if (prop is WzLongProperty long_prop)
            {
                tw.Write(string.Concat(depth, "<long name=\"", XmlUtil.SanitizeText(long_prop.Name), "\" value=\"",
                             long_prop.Value, "\"/>") + lineBreak);
            }
            else if (prop is WzUOLProperty property11)
            {
                tw.Write(depth + "<uol name=\"" + property11.Name + "\" value=\"" +
                         XmlUtil.SanitizeText(property11.Value) + "\"/>" + lineBreak);
            }
            else if (prop is WzVectorProperty property12)
            {
                tw.Write(string.Concat(depth, "<vector name=\"", XmlUtil.SanitizeText(property12.Name), "\" x=\"",
                             property12.X.Value, "\" y=\"", property12.Y.Value, "\"/>") + lineBreak);
            }
            else if (prop is WzFloatProperty property13)
            {
                var str2 = Convert.ToString(property13.Value, formattingInfo);
                if (!str2.Contains("."))
                {
                    {
                        str2 = str2 + ".0";
                    }
                }

                tw.Write(depth + "<float name=\"" + XmlUtil.SanitizeText(property13.Name) + "\" value=\"" + str2 +
                         "\"/>" + lineBreak);
            }
            else if (prop is WzConvexProperty property14)
            {
                tw.Write(depth + "<extended name=\"" + XmlUtil.SanitizeText(prop.Name) + "\">" + lineBreak);
                var newDepth = depth + indent;
                foreach (var property in property14.WzProperties)
                {
                    {
                        WritePropertyToXML(tw, newDepth, property);
                    }
                }

                tw.Write(depth + "</extended>" + lineBreak);
            }
        }
    }
}