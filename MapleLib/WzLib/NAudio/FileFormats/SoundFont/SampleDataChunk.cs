using System;
using System.IO;

namespace MapleLib.WzLib.NAudio.FileFormats.SoundFont 
{
	class SampleDataChunk 
	{
		private byte[] sampleData;
		public SampleDataChunk(RiffChunk chunk) 
		{
			var header = chunk.ReadChunkID();
			if(header != "sdta") 
			{
				throw new InvalidDataException(String.Format("Not a sample data chunk ({0})",header));
			}
			sampleData = chunk.GetData();
		}

		public byte[] SampleData
		{
			get
			{
				return sampleData;
			}
		}
	}

} // end of namespace