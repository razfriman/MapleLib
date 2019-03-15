using System;
using System.Linq;
using NAudio.Dsp;

namespace NAudio.Wave.SampleProviders
{
    /// <summary>
    /// Fully managed resampling sample provider, based on the WDL Resampler
    /// </summary>
    public class WdlResamplingSampleProvider : ISampleProvider
    {
        private readonly WdlResampler resampler;
        private readonly WaveFormat outFormat;
        private readonly ISampleProvider source;
        private readonly int channels;

        /// <summary>
        /// Constructs a new resampler
        /// </summary>
        /// <param name="source">Source to resample</param>
        /// <param name="newSampleRate">Desired output sample rate</param>
        public WdlResamplingSampleProvider(ISampleProvider source, int newSampleRate)
        {
            channels = source.WaveFormat.Channels;
            outFormat = WaveFormat.CreateIeeeFloatWaveFormat(newSampleRate, channels);
            this.source = source;

            resampler = new WdlResampler();
            resampler.SetMode(true, 2, false);
            resampler.SetFilterParms();
            resampler.SetFeedMode(false); // output driven
            resampler.SetRates(source.WaveFormat.SampleRate, newSampleRate);
        }

        /// <summary>
        /// Reads from this sample provider
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            float[] inBuffer;
            int inBufferOffset;
            var framesRequested = count / channels;
            var inNeeded = resampler.ResamplePrepare(framesRequested, outFormat.Channels, out inBuffer, out inBufferOffset);
            var inAvailable = source.Read(inBuffer, inBufferOffset, inNeeded * channels) / channels;
            var outAvailable = resampler.ResampleOut(buffer, offset, inAvailable, framesRequested, channels);
            return outAvailable * channels;
        }

        /// <summary>
        /// Output WaveFormat
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return outFormat; }
        }
    }
}
