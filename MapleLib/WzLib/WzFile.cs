using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MapleLib.Helper;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MapleLib.WzLib
{
    /// <summary>
    /// A class that contains all the information of a wz file
    /// </summary>
    public class WzFile : WzObject
    {
        public static readonly ILogger Log = LogManager.Log;

        #region Fields

        internal WzDirectory wzDir;
        internal uint versionHash;
        internal short version;
        internal byte[] WzIv;

        #endregion

        /// <summary>
        /// The parsed IWzDir after having called ParseWzDirectory(), this can either be a WzDirectory or a WzListDirectory
        /// </summary>
        public WzDirectory WzDirectory => wzDir;

        /// <summary>
        /// Name of the WzFile
        /// </summary>
        public override string Name { get; set; }

        /// <summary>
        /// The WzObjectType of the file
        /// </summary>
        public override WzObjectType ObjectType => WzObjectType.File;

        /// <summary>
        /// Returns WzDirectory[name]
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>WzDirectory[name]</returns>
        [JsonIgnore]
        public new WzObject this[string name] => WzDirectory[name];

        public WzHeader Header { get; set; }

        public short FileVersion { get; set; }

        public string FilePath { get; private set; }

        public WzMapleVersion MapleVersion { get; set; }

        [JsonIgnore]
        public override WzObject Parent
        {
            get => null;
            internal set { }
        }

        [JsonIgnore] public override WzFile WzFileParent => this;

        public override void Dispose()
        {
            if (wzDir.reader == null)
            {
                {
                    return;
                }
            }

            wzDir.reader.Close();
            Header = null;
            FilePath = null;
            Name = null;
            WzDirectory.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public WzFile(short gameVersion, WzMapleVersion version)
        {
            wzDir = new WzDirectory();
            Header = WzHeader.GetDefault();
            FileVersion = gameVersion;
            MapleVersion = version;
            WzIv = WzTool.GetIvByMapleVersion(version);
            wzDir.WzIv = WzIv;
        }

        /// <summary>
        /// Open a wz file from a file on the disk
        /// </summary>
        /// <param name="filePath">Path to the wz file</param>
        public WzFile(string filePath, WzMapleVersion version)
        {
            Name = Path.GetFileName(filePath);
            FilePath = filePath;
            FileVersion = -1;
            MapleVersion = version;
            if (version == WzMapleVersion.GetFromZlz)
            {
                var zlzStream = File.OpenRead(Path.Combine(Path.GetDirectoryName(filePath), "ZLZ.dll"));
                WzIv = WzKeyGenerator.GetIvFromZlz(zlzStream);
                zlzStream.Close();
            }
            else
            {
                {
                    WzIv = WzTool.GetIvByMapleVersion(version);
                }
            }
        }

        /// <summary>
        /// Open a wz file from a file on the disk
        /// </summary>
        /// <param name="filePath">Path to the wz file</param>
        public WzFile(string filePath, short gameVersion, WzMapleVersion version)
        {
            Name = Path.GetFileName(filePath);
            FilePath = filePath;
            FileVersion = gameVersion;
            MapleVersion = version;
            if (version == WzMapleVersion.GetFromZlz)
            {
                var zlzStream = File.OpenRead(Path.Combine(Path.GetDirectoryName(filePath), "ZLZ.dll"));
                WzIv = WzKeyGenerator.GetIvFromZlz(zlzStream);
                zlzStream.Close();
            }
            else
            {
                {
                    WzIv = WzTool.GetIvByMapleVersion(version);
                }
            }
        }

        /// <summary>
        /// Parses the wz file, if the wz file is a list.wz file, WzDirectory will be a WzListDirectory, if not, it'll simply be a WzDirectory
        /// </summary>
        public void ParseWzFile()
        {
            if (MapleVersion == WzMapleVersion.Generate)
            {
                {
                    throw new InvalidOperationException("Cannot call ParseWzFile() if WZ file type is GENERATE");
                }
            }

            ParseMainWzDirectory();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ParseWzFile(byte[] wzIv)
        {
            if (MapleVersion != WzMapleVersion.Generate)
            {
                {
                    throw new InvalidOperationException(
                        "Cannot call ParseWzFile(byte[] generateKey) if WZ file type is not GENERATE");
                }
            }

            WzIv = wzIv;
            ParseMainWzDirectory();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        internal void ParseMainWzDirectory()
        {
            if (FilePath == null)
            {
                Log.LogCritical("Path is null");
                return;
            }

            var reader = new WzBinaryReader(File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read), WzIv);

            Header = new WzHeader
            {
                Ident = reader.ReadString(4),
                FSize = reader.ReadUInt64(),
                FStart = reader.ReadUInt32(),
                Copyright = reader.ReadNullTerminatedString()
            };
            reader.ReadBytes((int) (Header.FStart - reader.BaseStream.Position));
            reader.Header = Header;
            version = reader.ReadInt16();
            if (FileVersion == -1)
            {
                for (var j = 0; j < short.MaxValue; j++)
                {
                    FileVersion = (short) j;
                    versionHash = GetVersionHash(version, FileVersion);
                    if (versionHash != 0)
                    {
                        reader.Hash = versionHash;
                        var position = reader.BaseStream.Position;
                        WzDirectory testDirectory;
                        try
                        {
                            testDirectory = new WzDirectory(reader, Name, versionHash, WzIv, this);
                            testDirectory.ParseDirectory();
                        }
                        catch
                        {
                            reader.BaseStream.Position = position;
                            continue;
                        }

                        var testImage = testDirectory.GetChildImages()[0];

                        try
                        {
                            reader.BaseStream.Position = testImage.Offset;
                            var checkByte = reader.ReadByte();
                            reader.BaseStream.Position = position;
                            testDirectory.Dispose();
                            switch (checkByte)
                            {
                                case 0x73:
                                case 0x1b:
                                {
                                    var directory = new WzDirectory(reader, Name, versionHash, WzIv, this);
                                    directory.ParseDirectory();
                                    wzDir = directory;
                                    return;
                                }
                            }

                            reader.BaseStream.Position = position;
                        }
                        catch
                        {
                            reader.BaseStream.Position = position;
                        }
                    }
                }

                throw new Exception(
                    "Error with game version hash : The specified game version is incorrect and WzLib was unable to determine the version itself");
            }

            {
                versionHash = GetVersionHash(version, FileVersion);
                reader.Hash = versionHash;
                var directory = new WzDirectory(reader, Name, versionHash, WzIv, this);
                directory.ParseDirectory();
                wzDir = directory;
            }
        }

        private static uint GetVersionHash(int encver, int realver)
        {
            var encryptedVersionNumber = encver;
            var versionNumber = realver;
            var versionNumberStr = versionNumber.ToString();
            var versionHash = versionNumberStr.Aggregate(0, (current, t) => 32 * current + t + 1);
            var a = (versionHash >> 24) & 0xFF;
            var b = (versionHash >> 16) & 0xFF;
            var c = (versionHash >> 8) & 0xFF;
            var d = versionHash & 0xFF;
            var decryptedVersionNumber = 0xff ^ a ^ b ^ c ^ d;

            return encryptedVersionNumber == decryptedVersionNumber ? Convert.ToUInt32(versionHash) : 0;
        }

        private void CreateVersionHash()
        {
            versionHash = 0;
            foreach (var ch in FileVersion.ToString())
            {
                versionHash = versionHash * 32 + (byte) ch + 1;
            }

            uint a = (versionHash >> 24) & 0xFF,
                b = (versionHash >> 16) & 0xFF,
                c = (versionHash >> 8) & 0xFF,
                d = versionHash & 0xFF;
            version = (byte) ~(a ^ b ^ c ^ d);
        }

        /// <summary>
        /// Saves a wz file to the disk, AKA repacking.
        /// </summary>
        /// <param name="path">Path to the output wz file</param>
        public void SaveToDisk(string path)
        {
            WzIv = WzTool.GetIvByMapleVersion(MapleVersion);
            CreateVersionHash();
            wzDir.SetHash(versionHash);
            var tempFile = Path.GetFileNameWithoutExtension(path) + ".TEMP";
            File.Create(tempFile).Close();
            wzDir.GenerateDataFile(tempFile);
            WzTool.StringCache.Clear();
            var totalLen = wzDir.GetImgOffsets(wzDir.GetOffsets(Header.FStart + 2));
            var wzWriter = new WzBinaryWriter(File.Create(path), WzIv) {Hash = versionHash};
            Header.FSize = totalLen - Header.FStart;
            for (var i = 0; i < 4; i++)
            {
                {
                    wzWriter.Write((byte) Header.Ident[i]);
                }
            }

            wzWriter.Write((long) Header.FSize);
            wzWriter.Write(Header.FStart);
            wzWriter.WriteNullTerminatedString(Header.Copyright);
            var extraHeaderLength = Header.FStart - wzWriter.BaseStream.Position;
            if (extraHeaderLength > 0)
            {
                wzWriter.Write(new byte[(int) extraHeaderLength]);
            }

            wzWriter.Write(version);
            wzWriter.Header = Header;
            wzDir.SaveDirectory(wzWriter);
            wzWriter.StringCache.Clear();
            var fs = File.OpenRead(tempFile);
            wzDir.SaveImages(wzWriter, fs);
            fs.Close();
            File.Delete(tempFile);
            wzWriter.StringCache.Clear();
            wzWriter.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ExportJson(Stream stream)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var sw = new StreamWriter(stream))
            {
                using (var writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, this);
                }
            }
        }

        /// <summary>
        /// Returns an array of objects from a given path. Wild cards are supported
        /// For example :
        /// GetObjectsFromPath("Map.wz/Map0/*");
        /// Would return all the objects (in this case images) from the sub directory Map0
        /// </summary>
        /// <param name="path">The path to the object(s)</param>
        /// <returns>An array of IWzObjects containing the found objects</returns>
        public List<WzObject> GetObjectsFromWildcardPath(string path)
        {
            if (path.ToLower() == Name.ToLower())
            {
                {
                    return new List<WzObject> {WzDirectory};
                }
            }

            if (path == "*")
            {
                var fullList = new List<WzObject>
                {
                    WzDirectory
                };

                fullList.AddRange(GetObjectsFromDirectory(WzDirectory));
                return fullList;
            }

            if (!path.Contains("*"))
            {
                {
                    return new List<WzObject> {GetObjectFromPath(path)};
                }
            }

            var seperatedNames = path.Split("/".ToCharArray());
            if (seperatedNames.Length == 2 && seperatedNames[1] == "*")
            {
                {
                    return GetObjectsFromDirectory(WzDirectory);
                }
            }

            var objList = new List<WzObject>();
            foreach (var img in WzDirectory.WzImages)
            {
                {
                    foreach (var spath in GetPathsFromImage(img, Name + "/" + img.Name))
                    {
                        {
                            if (StrMatch(path, spath))
                            {
                                {
                                    objList.Add(GetObjectFromPath(spath));
                                }
                            }
                        }
                    }
                }
            }

            foreach (var dir in wzDir.WzDirectories)
            {
                {
                    foreach (var spath in GetPathsFromDirectory(dir, Name + "/" + dir.Name))
                    {
                        {
                            if (StrMatch(path, spath))
                            {
                                {
                                    objList.Add(GetObjectFromPath(spath));
                                }
                            }
                        }
                    }
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            return objList;
        }

        public List<WzObject> GetObjectsFromRegexPath(string path)
        {
            if (path.ToLower() == Name.ToLower())
            {
                {
                    return new List<WzObject> {WzDirectory};
                }
            }

            var objList = new List<WzObject>();
            foreach (var img in WzDirectory.WzImages)
            {
                {
                    foreach (var spath in GetPathsFromImage(img, Name + "/" + img.Name))
                    {
                        {
                            if (Regex.Match(spath, path).Success)
                            {
                                {
                                    objList.Add(GetObjectFromPath(spath));
                                }
                            }
                        }
                    }
                }
            }

            foreach (var dir in wzDir.WzDirectories)
            {
                {
                    foreach (var spath in GetPathsFromDirectory(dir, Name + "/" + dir.Name))
                    {
                        {
                            if (Regex.Match(spath, path).Success)
                            {
                                {
                                    objList.Add(GetObjectFromPath(spath));
                                }
                            }
                        }
                    }
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            return objList;
        }

        public List<WzObject> GetObjectsFromDirectory(WzDirectory dir)
        {
            var objList = new List<WzObject>();
            foreach (var img in dir.WzImages)
            {
                objList.Add(img);
                objList.AddRange(GetObjectsFromImage(img));
            }

            foreach (var subdir in dir.WzDirectories)
            {
                objList.Add(subdir);
                objList.AddRange(GetObjectsFromDirectory(subdir));
            }

            return objList;
        }

        public List<WzObject> GetObjectsFromImage(WzImage img)
        {
            var objList = new List<WzObject>();
            foreach (var prop in img.Properties.WzProperties)
            {
                objList.Add(prop);
                objList.AddRange(GetObjectsFromProperty(prop));
            }

            return objList;
        }

        public static List<WzObject> GetObjectsFromProperty(WzImageProperty prop)
        {
            var objList = new List<WzObject>();
            switch (prop.PropertyType)
            {
                case WzPropertyType.Canvas:
                    foreach (var canvasProp in ((WzCanvasProperty) prop).WzProperties)
                    {
                        {
                            objList.AddRange(GetObjectsFromProperty(canvasProp));
                        }
                    }

                    objList.Add(((WzCanvasProperty) prop).PngProperty);
                    break;
                case WzPropertyType.Convex:
                    foreach (var exProp in ((WzConvexProperty) prop).WzProperties)
                    {
                        {
                            objList.AddRange(GetObjectsFromProperty(exProp));
                        }
                    }

                    break;
                case WzPropertyType.SubProperty:
                    foreach (var subProp in ((WzSubProperty) prop).WzProperties)
                    {
                        {
                            objList.AddRange(GetObjectsFromProperty(subProp));
                        }
                    }

                    break;
                case WzPropertyType.Vector:
                    objList.Add(((WzVectorProperty) prop).X);
                    objList.Add(((WzVectorProperty) prop).Y);
                    break;
                case WzPropertyType.Null:
                case WzPropertyType.Short:
                case WzPropertyType.Int:
                case WzPropertyType.Long:
                case WzPropertyType.Float:
                case WzPropertyType.Double:
                case WzPropertyType.String:
                case WzPropertyType.Sound:
                case WzPropertyType.UOL:
                case WzPropertyType.PNG:
                    break;
            }

            return objList;
        }

        internal List<string> GetPathsFromDirectory(WzDirectory dir, string curPath)
        {
            var objList = new List<string>();
            foreach (var img in dir.WzImages)
            {
                objList.Add(curPath + "/" + img.Name);
                objList.AddRange(GetPathsFromImage(img, curPath + "/" + img.Name));
            }

            foreach (var subdir in dir.WzDirectories)
            {
                objList.Add(curPath + "/" + subdir.Name);
                objList.AddRange(GetPathsFromDirectory(subdir, curPath + "/" + subdir.Name));
            }

            return objList;
        }

        internal List<string> GetPathsFromImage(WzImage img, string curPath)
        {
            var objList = new List<string>();
            foreach (var prop in img.Properties.WzProperties)
            {
                objList.Add(curPath + "/" + prop.Name);
                objList.AddRange(GetPathsFromProperty(prop, curPath + "/" + prop.Name));
            }

            return objList;
        }

        internal static List<string> GetPathsFromProperty(WzImageProperty prop, string curPath)
        {
            var objList = new List<string>();
            switch (prop.PropertyType)
            {
                case WzPropertyType.Canvas:
                    foreach (var canvasProp in ((WzCanvasProperty) prop).WzProperties)
                    {
                        objList.Add(curPath + "/" + canvasProp.Name);
                        objList.AddRange(GetPathsFromProperty(canvasProp, curPath + "/" + canvasProp.Name));
                    }

                    objList.Add(curPath + "/PNG");
                    break;
                case WzPropertyType.Convex:
                    foreach (var exProp in ((WzConvexProperty) prop).WzProperties)
                    {
                        objList.Add(curPath + "/" + exProp.Name);
                        objList.AddRange(GetPathsFromProperty(exProp, curPath + "/" + exProp.Name));
                    }

                    break;
                case WzPropertyType.SubProperty:
                    foreach (var subProp in ((WzSubProperty) prop).WzProperties)
                    {
                        objList.Add(curPath + "/" + subProp.Name);
                        objList.AddRange(GetPathsFromProperty(subProp, curPath + "/" + subProp.Name));
                    }

                    break;
                case WzPropertyType.Vector:
                    objList.Add(curPath + "/X");
                    objList.Add(curPath + "/Y");
                    break;
            }

            return objList;
        }

        public WzObject GetObjectFromPath(string path)
        {
            var separatedPath = path.Split("/".ToCharArray());
            if (separatedPath[0].ToLower() != wzDir.name.ToLower() && separatedPath[0].ToLower() !=
                wzDir.name.Substring(0, wzDir.name.Length - 3).ToLower())
            {
                {
                    return null;
                }
            }

            if (separatedPath.Length == 1)
            {
                {
                    return WzDirectory;
                }
            }

            WzObject curObj = WzDirectory;
            for (var i = 1; i < separatedPath.Length; i++)
            {
                if (curObj == null)
                {
                    return null;
                }

                switch (curObj.ObjectType)
                {
                    case WzObjectType.Directory:
                        curObj = ((WzDirectory) curObj)[separatedPath[i]];
                        continue;
                    case WzObjectType.Image:
                        curObj = ((WzImage) curObj)[separatedPath[i]];
                        continue;
                    case WzObjectType.Property:
                        switch (((WzImageProperty) curObj).PropertyType)
                        {
                            case WzPropertyType.Canvas:
                                curObj = ((WzCanvasProperty) curObj)[separatedPath[i]];
                                continue;
                            case WzPropertyType.Convex:
                                curObj = ((WzConvexProperty) curObj)[separatedPath[i]];
                                continue;
                            case WzPropertyType.SubProperty:
                                curObj = ((WzSubProperty) curObj)[separatedPath[i]];
                                continue;
                            case WzPropertyType.Vector:
                                if (separatedPath[i] == "X")
                                {
                                    return ((WzVectorProperty) curObj).X;
                                }

                                if (separatedPath[i] == "Y")
                                {
                                    return ((WzVectorProperty) curObj).Y;
                                }

                                return null;
                            default:
                                return null;
                        }
                }
            }

            return curObj;
        }

        internal bool StrMatch(string strWildCard, string strCompare)
        {
            if (strWildCard.Length == 0)
            {
                {
                    return strCompare.Length == 0;
                }
            }

            if (strCompare.Length == 0)
            {
                {
                    return false;
                }
            }

            if (strWildCard[0] == '*' && strWildCard.Length > 1)
            {
                return strCompare.Where((t, index) => StrMatch(strWildCard.Substring(1), strCompare.Substring(index)))
                    .Any();
            }

            if (strWildCard[0] == '*')
            {
                {
                    return true;
                }
            }

            if (strWildCard[0] == strCompare[0])
            {
                {
                    return StrMatch(strWildCard.Substring(1), strCompare.Substring(1));
                }
            }

            return false;
        }

        public override void Remove() => Dispose();
    }
}