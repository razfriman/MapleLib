using MapleLib.WzLib.NAudio.Wave.WaveFormats;

namespace MapleLib.WzLib.NAudio.Wave.Compression
{
    /// <summary>
    /// ACM Format Tag
    /// </summary>
    public class AcmFormatTag
    {
        private AcmFormatTagDetails formatTagDetails;

        internal AcmFormatTag(AcmFormatTagDetails formatTagDetails)
        {
            this.formatTagDetails = formatTagDetails;                        
        }

        /// <summary>
        /// Format Tag Index
        /// </summary>
        public int FormatTagIndex => formatTagDetails.formatTagIndex;

        /// <summary>
        /// Format Tag
        /// </summary>
        public WaveFormatEncoding FormatTag => (WaveFormatEncoding)formatTagDetails.formatTag;

        /// <summary>
        /// Format Size
        /// </summary>
        public int FormatSize => formatTagDetails.formatSize;

        /// <summary>
        /// Support Flags
        /// </summary>
        public AcmDriverDetailsSupportFlags SupportFlags => formatTagDetails.supportFlags;

        /// <summary>
        /// Standard Formats Count
        /// </summary>
        public int StandardFormatsCount => formatTagDetails.standardFormatsCount;

        /// <summary>
        /// Format Description
        /// </summary>
        public string FormatDescription => formatTagDetails.formatDescription;
    }
}
