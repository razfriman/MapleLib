using System.Runtime.InteropServices;

namespace MapleLib.WzLib.NAudio.Wave.WaveFormats
{
    /// <summary>
    /// IMA/DVI ADPCM Wave Format
    /// Work in progress
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class ImaAdpcmWaveFormat : WaveFormat
    {
        short samplesPerBlock;

        /// <summary>
        /// parameterless constructor for Marshalling
        /// </summary>
        ImaAdpcmWaveFormat()
        {
        }

        /// <summary>
        /// Creates a new IMA / DVI ADPCM Wave Format
        /// </summary>
        /// <param name="sampleRate">Sample Rate</param>
        /// <param name="channels">Number of channels</param>
        /// <param name="bitsPerSample">Bits Per Sample</param>
        public ImaAdpcmWaveFormat(int sampleRate, int channels, int bitsPerSample)
        {
            waveFormatTag = WaveFormatEncoding.DviAdpcm; // can also be ImaAdpcm - they are the same
            this.sampleRate = sampleRate;
            this.channels = (short)channels;
            this.bitsPerSample = (short)bitsPerSample; // TODO: can be 3 or 4
            extraSize = 2;            
            blockAlign = 0; //TODO
            averageBytesPerSecond = 0; //TODO
            samplesPerBlock = 0; // TODO
        }
    }
}
