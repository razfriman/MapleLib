using System;
using MapleLib.WzLib.NAudio.Utils;
using MapleLib.WzLib.NAudio.Wave.WaveFormats;
using MapleLib.WzLib.NAudio.Wave.WaveOutputs;

namespace MapleLib.WzLib.NAudio.Wave.WaveProviders
{
    /// <summary>
    /// Converts 16 bit PCM to IEEE float, optionally adjusting volume along the way
    /// </summary>
    public class Wave16ToFloatProvider : IWaveProvider
    {
        private IWaveProvider sourceProvider;
        private readonly WaveFormat waveFormat;
        private volatile float volume;
        private byte[] sourceBuffer;

        /// <summary>
        /// Creates a new Wave16toFloatProvider
        /// </summary>
        /// <param name="sourceProvider">the source provider</param>
        public Wave16ToFloatProvider(IWaveProvider sourceProvider)
        {
            if (sourceProvider.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
            {
                throw new ArgumentException("Only PCM supported");
            }

            if (sourceProvider.WaveFormat.BitsPerSample != 16)
            {
                throw new ArgumentException("Only 16 bit audio supported");
            }

            waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sourceProvider.WaveFormat.SampleRate, sourceProvider.WaveFormat.Channels);

            this.sourceProvider = sourceProvider;
            this.volume = 1.0f;
        }

        /// <summary>
        /// Reads bytes from this wave stream
        /// </summary>
        /// <param name="destBuffer">The destination buffer</param>
        /// <param name="offset">Offset into the destination buffer</param>
        /// <param name="numBytes">Number of bytes read</param>
        /// <returns>Number of bytes read.</returns>
        public int Read(byte[] destBuffer, int offset, int numBytes)
        {
            var sourceBytesRequired = numBytes / 2;
            sourceBuffer = BufferHelpers.Ensure(sourceBuffer, sourceBytesRequired);
            var sourceBytesRead = sourceProvider.Read(sourceBuffer, offset, sourceBytesRequired);
            var sourceWaveBuffer = new WaveBuffer(sourceBuffer);
            var destWaveBuffer = new WaveBuffer(destBuffer);

            var sourceSamples = sourceBytesRead / 2;
            var destOffset = offset / 4;
            for (var sample = 0; sample < sourceSamples; sample++)
            {
                destWaveBuffer.FloatBuffer[destOffset++] = (sourceWaveBuffer.ShortBuffer[sample] / 32768f) * volume;
            }

            return sourceSamples * 4;
        }

        /// <summary>
        /// <see cref="IWaveProvider.WaveFormat"/>
        /// </summary>
        public WaveFormat WaveFormat => waveFormat;

        /// <summary>
        /// Volume of this channel. 1.0 = full scale
        /// </summary>
        public float Volume
        {
            get => volume;
            set => volume = value;
        }
    }
}
