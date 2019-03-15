using System.Runtime.InteropServices;
using MapleLib.WzLib.NAudio.CoreAudioApi.Interfaces;

namespace MapleLib.WzLib.NAudio.CoreAudioApi
{
    internal class AudioSessionNotification : IAudioSessionNotification
    {
        private AudioSessionManager parent;

        internal AudioSessionNotification(AudioSessionManager parent)
        {
            this.parent = parent;
        }

        [PreserveSig]
        public int OnSessionCreated(IAudioSessionControl newSession)
        {
            parent.FireSessionCreated(newSession);
            return 0;
        }
    }
}
