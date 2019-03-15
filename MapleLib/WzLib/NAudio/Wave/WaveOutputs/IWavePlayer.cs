using System;
using MapleLib.WzLib.NAudio.Wave.WaveFormats;

// ReSharper disable once CheckNamespace
namespace MapleLib.WzLib.NAudio.Wave.WaveOutputs
{
    /// <summary>
    /// Represents the interface to a device that can play a WaveFile
    /// </summary>
    public interface IWavePlayer : IDisposable
    {
        /// <summary>
        /// Begin playback
        /// </summary>
        void Play();

        /// <summary>
        /// Stop playback
        /// </summary>
        void Stop();

        /// <summary>
        /// Pause Playback
        /// </summary>
        void Pause();

        /// <summary>
        /// Initialise playback
        /// </summary>
        /// <param name="waveProvider">The waveprovider to be played</param>
        void Init(IWaveProvider waveProvider);

        /// <summary>
        /// Current playback state
        /// </summary>
        PlaybackState PlaybackState { get; }

        /// <summary>
        /// The volume 
        /// 1.0f is full scale
        /// Note that not all implementations necessarily support volume changes
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Indicates that playback has gone into a stopped state due to 
        /// reaching the end of the input stream or an error has been encountered during playback
        /// </summary>
        event EventHandler<StoppedEventArgs> PlaybackStopped;
    }

    /// <summary>
    /// Interface for IWavePlayers that can report position
    /// </summary>
    public interface IWavePosition
    {
        /// <summary>
        /// Position (in terms of bytes played - does not necessarily translate directly to the position within the source audio file)
        /// </summary>
        /// <returns>Position in bytes</returns>
        long GetPosition();

        /// <summary>
        /// Gets a <see cref="WaveFormat"/> instance indicating the format the hardware is using.
        /// </summary>
        WaveFormat OutputWaveFormat { get; }
    }
}
