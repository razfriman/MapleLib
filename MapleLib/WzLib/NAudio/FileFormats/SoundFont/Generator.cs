using System;

namespace MapleLib.WzLib.NAudio.FileFormats.SoundFont 
{
	/// <summary>
	/// Soundfont generator
	/// </summary>
	public class Generator 
	{
		private GeneratorEnum generatorType;
		private ushort rawAmount;
		private Instrument instrument;
		private SampleHeader sampleHeader;
		
		/// <summary>
		/// Gets the generator type
		/// </summary>
		public GeneratorEnum GeneratorType 
		{
			get => generatorType;
			set => generatorType = value;
		}
		
		/// <summary>
		/// Generator amount as an unsigned short
		/// </summary>
		public ushort UInt16Amount 
		{
			get => rawAmount;
			set => rawAmount = value;
		}
		
		/// <summary>
		/// Generator amount as a signed short
		/// </summary>
		public short Int16Amount 
		{
			get => (short) rawAmount;
			set => rawAmount = (ushort) value;
		}
		
		/// <summary>
		/// Low byte amount
		/// </summary>
		public byte LowByteAmount 
		{
			get => (byte) (rawAmount & 0x00FF);
			set 
			{
				rawAmount &= 0xFF00;
				rawAmount += value;
			}
		}
		
		/// <summary>
		/// High byte amount
		/// </summary>
		public byte HighByteAmount 
		{
			get => (byte) ((rawAmount & 0xFF00) >> 8);
			set 
			{
				rawAmount &= 0x00FF;
				rawAmount += (ushort) (value << 8);
			}
		}

		/// <summary>
		/// Instrument
		/// </summary>
		public Instrument Instrument
		{
			get => instrument;
			set => instrument = value;
		}

		/// <summary>
		/// Sample Header
		/// </summary>
		public SampleHeader SampleHeader
		{
			get => sampleHeader;
			set => sampleHeader = value;
		}

		/// <summary>
		/// <see cref="object.ToString"/>
		/// </summary>
		public override string ToString()
		{
			if(generatorType == GeneratorEnum.Instrument)
			{
				return String.Format("Generator Instrument {0}",instrument.Name);
			}
			else if(generatorType == GeneratorEnum.SampleID)
			{
				return String.Format("Generator SampleID {0}",sampleHeader);
			}
			else
			{
				return String.Format("Generator {0} {1}",generatorType,rawAmount);
			}
		}

	}
}