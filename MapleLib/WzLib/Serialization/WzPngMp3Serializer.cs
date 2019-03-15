using System.DrawingCore.Imaging;
using System.IO;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.WzLib.Serialization
{
    public class WzPngMp3Serializer : ProgressingWzSerializer, IWzImageSerializer, IWzObjectSerializer
    {
        //List<WzImage> imagesToUnparse = new List<WzImage>();
        private string outPath;

        public void SerializeObject(WzObject obj, string outPath)
        {
            //imagesToUnparse.Clear();
            total = 0;
            curr = 0;
            this.outPath = outPath;
            Directory.CreateDirectory(outPath);

            if (outPath.Substring(outPath.Length - 1, 1) != @"\")
            {
                {
                    outPath += @"\";
                }
            }

            total = CalculateTotal(obj);
            ExportRecursion(obj, outPath);
            /*foreach (WzImage img in imagesToUnparse)
                img.UnparseImage();
            imagesToUnparse.Clear();*/
        }

        public void SerializeFile(WzFile file, string path)
        {
            SerializeObject(file, path);
        }

        public void SerializeDirectory(WzDirectory file, string path)
        {
            SerializeObject(file, path);
        }

        public void SerializeImage(WzImage file, string path)
        {
            SerializeObject(file, path);
        }

        private static int CalculateTotal(WzObject currObj)
        {
            var result = 0;
            if (currObj is WzFile file)
            {
                {
                    result += file.WzDirectory.CountImages();
                }
            }
            else if (currObj is WzDirectory directory)
            {
                {
                    result += directory.CountImages();
                }
            }

            return result;
        }

        private void ExportRecursion(WzObject currObj, string exportOutPath)
        {
            if (currObj is WzFile file)
            {
                {
                    ExportRecursion(file.WzDirectory, exportOutPath);
                }
            }
            else if (currObj is WzDirectory directory)
            {
                exportOutPath += directory.Name + @"\";
                if (!Directory.Exists(exportOutPath))
                {
                    {
                        Directory.CreateDirectory(exportOutPath);
                    }
                }

                foreach (var subdir in directory.WzDirectories)
                {
                    {
                        ExportRecursion(subdir, exportOutPath + subdir.Name + @"\");
                    }
                }

                foreach (var subimg in directory.WzImages)
                {
                    {
                        ExportRecursion(subimg, exportOutPath + subimg.Name + @"\");
                    }
                }
            }
            else if (currObj is WzCanvasProperty canvasProperty)
            {
                var bmp = canvasProperty.PngProperty.GetPNG(false);
                var path = exportOutPath + canvasProperty.Name + ".png";
                bmp.Save(path, ImageFormat.Png);
                //curr++;
            }
            else if (currObj is WzSoundProperty soundProperty)
            {
                var path = exportOutPath + soundProperty.Name + ".mp3";
                soundProperty.SaveToFile(path);
            }
            else if (currObj is WzImage image)
            {
                exportOutPath += image.Name + @"\";
                if (!Directory.Exists(exportOutPath))
                {
                    {
                        Directory.CreateDirectory(exportOutPath);
                    }
                }

                var parse = image.Parsed || image.Changed;
                if (!parse)
                {
                    {
                        image.ParseImage();
                    }
                }

                foreach (var subprop in image.Properties.WzProperties)
                {
                    {
                        ExportRecursion(subprop, exportOutPath);
                    }
                }

                if (!parse)
                {
                    {
                        image.UnparseImage();
                    }
                }

                curr++;
            }
            else if (currObj is IPropertyContainer)
            {
                exportOutPath += currObj.Name + ".";
                foreach (var subprop in ((IPropertyContainer) currObj).WzProperties)
                {
                    {
                        ExportRecursion(subprop, exportOutPath);
                    }
                }
            }
            else if (currObj is WzUOLProperty property)
            {
                {
                    ExportRecursion(property.LinkValue, exportOutPath);
                }
            }
        }
    }
}