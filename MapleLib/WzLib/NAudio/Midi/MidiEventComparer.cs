using System;
using System.Collections.Generic;

namespace MapleLib.WzLib.NAudio.Midi
{
    /// <summary>
    /// Utility class for comparing MidiEvent objects
    /// </summary>
    public class MidiEventComparer : IComparer<MidiEvent>
    {
        #region IComparer<MidiEvent> Members

        /// <summary>
        /// Compares two MidiEvents
        /// Sorts by time, with EndTrack always sorted to the end
        /// </summary>
        public int Compare(MidiEvent x, MidiEvent y)
        {
            var xTime = x.AbsoluteTime;
            var yTime = y.AbsoluteTime;

            if (xTime == yTime)
            {
                // sort meta events before note events, except end track
                var xMeta = x as MetaEvent;
                var yMeta = y as MetaEvent;

                if (xMeta != null)
                {
                    if (xMeta.MetaEventType == MetaEventType.EndTrack)
                    {
                        xTime = Int64.MaxValue;
                    }
                    else
                    {
                        xTime = Int64.MinValue;
                    }
                }
                if (yMeta != null)
                {
                    if (yMeta.MetaEventType == MetaEventType.EndTrack)
                    {
                        yTime = Int64.MaxValue;
                    }
                    else
                    {
                        yTime = Int64.MinValue;
                    }
                }
            }
            return xTime.CompareTo(yTime);
        }

        #endregion
    }
}