namespace MapleLib.WzLib.Serialization
{
    public interface IWzDirectorySerializer : IWzFileSerializer
    {
        void SerializeDirectory(WzDirectory dir, string path);
    }
}