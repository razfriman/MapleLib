using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MapleLib.PacketLib
{
	public abstract class AbstractPacket
	{
		protected MemoryStream mBuffer;

		public byte[] ToArray()
		{
			return mBuffer.ToArray();
		}
	}
}