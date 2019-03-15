using System.Runtime.InteropServices;

namespace MapleLib.WzLib.NAudio.Wave.WaveFormats
{
    //http://svn.xiph.org/tags/vorbisacm_20020708/src/vorbisacm/vorbisacm.h
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack=2)]
    class OggWaveFormat : WaveFormat
    {
        //public short cbSize;
        public uint dwVorbisACMVersion;
	    public uint dwLibVorbisVersion;
    } 
}
