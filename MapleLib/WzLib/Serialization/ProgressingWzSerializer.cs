namespace MapleLib.WzLib
{
    public abstract class ProgressingWzSerializer
    {
        protected int total;
        protected int curr;
        public int Total => total;
        public int Current => curr;
    }
}