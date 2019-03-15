using System;

namespace MapleLib.WzLib.NAudio.Dmo
{
    /// <summary>
    /// DMO_PARTIAL_MEDIATYPE
    /// </summary>
    struct DmoPartialMediaType
    {
        Guid type;
        Guid subtype;

        public Guid Type
        {
            get => type;
            internal set => type = value;
        }

        public Guid Subtype
        {
            get => subtype;
            internal set => subtype = value;
        }
    }
}
