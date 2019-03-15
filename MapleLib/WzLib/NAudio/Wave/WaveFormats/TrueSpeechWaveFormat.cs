using System.IO;
using System.Runtime.InteropServices;

namespace MapleLib.WzLib.NAudio.Wave.WaveFormats
{
    /// <summary>
    /// DSP Group TrueSpeech
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class TrueSpeechWaveFormat : WaveFormat
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        short[] unknown;

        /// <summary>
        /// DSP Group TrueSpeech WaveFormat
        /// </summary>
        public TrueSpeechWaveFormat()
        {
            waveFormatTag = WaveFormatEncoding.DspGroupTrueSpeech;
            channels = 1;
            averageBytesPerSecond = 1067;
            bitsPerSample = 1;
            blockAlign = 32;
            sampleRate = 8000;

            extraSize = 32;
            unknown = new short[16];
            unknown[0] = 1;
            unknown[1] = 0xF0;
        }

        /// <summary>
        /// Writes this structure to a BinaryWriter
        /// </summary>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            foreach (var val in unknown)
            {
                writer.Write(val);
            }
        }
    }
}
