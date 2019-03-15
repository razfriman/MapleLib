using System;
using MapleLib.WzLib.NAudio.Wave.WaveFormats;
using MapleLib.WzLib.NAudio.Wave.WaveOutputs;

namespace MapleLib.WzLib.NAudio.Wave.WaveProviders
{
    /// <summary>
    /// Utility class to intercept audio from an IWaveProvider and
    /// save it to disk
    /// </summary>
    public class WaveRecorder : IWaveProvider, IDisposable
    {
        private WaveFileWriter writer;
        private IWaveProvider source;

        /// <summary>
        /// Constructs a new WaveRecorder
        /// </summary>
        /// <param name="destination">The location to write the WAV file to</param>
        /// <param name="source">The Source Wave Provider</param>
        public WaveRecorder(IWaveProvider source, string destination)
        {
            this.source = source;
            this.writer = new WaveFileWriter(destination, source.WaveFormat);
        }

        /// <summary>
        /// Read simply returns what the source returns, but writes to disk along the way
        /// </summary>
        public int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = source.Read(buffer, offset, count);
            writer.Write(buffer, offset, bytesRead);
            return bytesRead;
        }

        /// <summary>
        /// The WaveFormat
        /// </summary>
        public WaveFormat WaveFormat => source.WaveFormat;

        /// <summary>
        /// Closes the WAV file
        /// </summary>
        public void Dispose()
        {
            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }
        }
    }
}
