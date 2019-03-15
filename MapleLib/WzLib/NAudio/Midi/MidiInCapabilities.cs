using System;
using System.Runtime.InteropServices;
using MapleLib.WzLib.NAudio.Wave.MmeInterop;

namespace MapleLib.WzLib.NAudio.Midi
{
    /// <summary>
    /// MIDI In Device Capabilities
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MidiInCapabilities
    {
        /// <summary>
        /// wMid
        /// </summary>
        UInt16 manufacturerId;
        /// <summary>
        /// wPid
        /// </summary>
        UInt16 productId;
        /// <summary>
        /// vDriverVersion
        /// </summary>
        UInt32 driverVersion;
        /// <summary>
        /// Product Name
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxProductNameLength)]
        string productName;
        /// <summary>
        /// Support - Reserved
        /// </summary>
        Int32 support;

        private const int MaxProductNameLength = 32;

        /// <summary>
        /// Gets the manufacturer of this device
        /// </summary>
        public Manufacturers Manufacturer => (Manufacturers)manufacturerId;

        /// <summary>
        /// Gets the product identifier (manufacturer specific)
        /// </summary>
        public int ProductId => productId;

        /// <summary>
        /// Gets the product name
        /// </summary>
        public string ProductName => productName;
    }
}
