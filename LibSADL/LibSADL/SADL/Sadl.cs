//
//  Sadl.cs
//
//  Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
//
//  Copyright (c) 2014 Benito Palacios Sánchez
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using Libgame;
using Libgame.IO;

namespace LibSADL
{
	public class Sadl : SoundFormat
	{
		IConverter<Sadl, BinaryFormat> binaryConverter;
		IConverter<Sadl, Wave> wavConverter;

		public static string MagicStamp { 
			get { return "sadl"; }
		}

		public override string FormatName {
			get { return "sound.sadl"; }
		}

		public uint     FileSize { get; set; }
		public string   FileName { get; set; }
		public DateTime Creation { get; set; }
	
		public override IDecoder Decoder { get; set; }
		public override int SampleRate { get; set; }
		public override int Channels   { get; set; }

		public ushort  ChunkSize  { get; set; }
		public ushort  SamplesPerChunk { get; set; }
		public ushort  SamplesSizePerChunk { get; set; }

		public byte[] Channel0LastChunk { get; set; }
		public byte[] Channel1LastChunk { get; set; }
		public int[,] HistoricalValues  { get; set; }

		public bool CanLoop      { get; set; }
		public uint StartOffset  { get; set; }
		public uint LoopOffset   { get; set; }
		public uint DataSize     { get; set; }
		public uint LoopDataSize { get; set; }

		public byte   Unknown30 { get; set; }
		public byte   Unknown35 { get; set; }
		public ushort Unknown36 { get; set; }
		public ushort Unknown38 { get; set; }
		public uint   Unknown44 { get; set; }
		public uint   Unknown4C { get; set; }
		public uint   Unknown50 { get; set; }
		public byte   Unknown60 { get; set; }
		public byte   Unknown61 { get; set; }
		public byte   Unknown62 { get; set; }
		public byte   Unknown63 { get; set; }
		public byte   Unknown64 { get; set; }
		public byte   Unknown65 { get; set; }

		public override void Initialize(GameFile file, params object[] parameters)
		{
			base.Initialize(file, parameters);
			binaryConverter = new SadlBinaryConverter();
			wavConverter    = new SadlWavConverter();
		}

		public override void Read(DataStream strIn)
		{
			var bin = new BinaryFormat(strIn);
			binaryConverter.Import(bin, this);
		}

		public override void Write(DataStream strOut)
		{
			var bin = new BinaryFormat(strOut);
			binaryConverter.Export(this, bin);
		}

		public override void Import(params DataStream[] strIn)
		{
			var bin = new BinaryFormat(strIn[0]);
			var wav = new Wave();
			var wavBinConverter = new WaveBinaryConverter();
			wavBinConverter.Import(bin, wav);

			wavConverter.Import(wav, this);

			wav.Dispose();
		}

		public override void Export(params DataStream[] strOut)
		{
			var wav = new Wave();
			wavConverter.Export(this, wav);

			var bin = new BinaryFormat(strOut[0]);
			var wavBinConverter = new WaveBinaryConverter();
			wavBinConverter.Export(wav, bin);

			wav.Dispose();
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
			if (freeManagedResourcesAlso)
				Decoder.Dispose();
		}
	}
}

