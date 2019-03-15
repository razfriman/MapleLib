namespace MapleLib.WzLib.Serialization
{
    public abstract class ProgressingWzSerializer
    {
        public int Total { get; protected set; }
        public int Current { get; protected set; }
    }
}