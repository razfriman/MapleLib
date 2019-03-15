namespace MapleLib.WzLib.Serialization
{
    public interface IWzFileSerializer
    {
        void SerializeFile(WzFile file, string path);
    }
}