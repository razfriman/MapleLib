using System;

namespace MapleLib.WzLib
{
    public class WzImageResource : IDisposable
    {
        bool parsed;
        WzImage img;
        public WzImageResource(WzImage img)
        {
            this.img = img;
            this.parsed = img.Parsed;
            if (!parsed)
            {
                img.ParseImage();
            }
        }

        public void Dispose()
        {
            if (!parsed)
            {
                img.UnparseImage();
            }
        }
    }
}
