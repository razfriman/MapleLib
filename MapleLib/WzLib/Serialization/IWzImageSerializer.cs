namespace MapleLib.WzLib.Serialization
{
    public interface IWzImageSerializer : IWzDirectorySerializer
    {
        void SerializeImage(WzImage img, string path);
    }
}