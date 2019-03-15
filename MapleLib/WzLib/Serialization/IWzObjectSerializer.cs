namespace MapleLib.WzLib.Serialization
{
    public interface IWzObjectSerializer
    {
        void SerializeObject(WzObject file, string path);
    }
}