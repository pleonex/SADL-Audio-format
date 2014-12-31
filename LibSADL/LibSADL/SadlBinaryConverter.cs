//
//  SadlBinaryConverter.cs
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
using Libgame.IO;

namespace LibSADL
{
	public class SadlBinaryConverter : IConverter<Sadl>
	{
		public void Import(DataStream strIn, Sadl format)
		{
			var reader = new DataReader(strIn);

			string magicStamp = reader.ReadString(4);
			if (magicStamp != Sadl.MagicStamp)
				throw new FormatException();

			reader.ReadUInt32(); // Null

			// File info
			format.FileSize = reader.ReadUInt32();
			reader.ReadUInt32(); // Unknown (maybe some CRC?)
			reader.ReadUInt64(); // Null

			uint date = reader.ReadUInt32();
			format.Creation = new DateTime(
				(int)(date & 0xFFFF), 
				(int)((date >> 16) & 0xFF),
				(int)((date >> 24) & 0xFF))
			.AddMilliseconds(reader.ReadUInt32());

			format.FileName = reader.ReadString(0x10);

			// Audio info
			format.Unknown30 = reader.ReadByte();
			format.CanLoop   = reader.ReadByte() == 1;
			format.Channels  = reader.ReadByte();

			byte codecInfo = reader.ReadByte();
			format.Codec = (byte)(codecInfo >> 4);
			format.SampleRate = (codecInfo & 0xF) * 8000; // Only allowed 2 and 4
			reader.ReadByte(); // Probably reserved for future use

			format.Unknown35 = reader.ReadByte();
			format.Unknown36 = reader.ReadUInt16();
			format.Unknown38 = reader.ReadUInt16();

			format.ChunkSize           = reader.ReadUInt16();
			format.SamplesPerChunk     = reader.ReadUInt16();
			format.SamplesSizePerChunk = reader.ReadUInt16(); // SamplesPerChunk * 2

			format.DataSize     = reader.ReadUInt32();
			format.Unknown44    = reader.ReadUInt32();
			format.StartOffset  = reader.ReadUInt32();
			format.Unknown4C    = reader.ReadUInt32();
			format.Unknown50    = reader.ReadUInt32();
			format.LoopOffset   = reader.ReadUInt32();
			format.LoopDataSize = reader.ReadUInt32();
			reader.ReadUInt32(); // Null

			format.Unknown60 = reader.ReadByte();
			format.Unknown61 = reader.ReadByte();
			format.Unknown62 = reader.ReadByte();
			format.Unknown63 = reader.ReadByte();
			format.Unknown64 = reader.ReadByte();
			format.Unknown65 = reader.ReadByte();

			// For Procyon
			reader.Stream.Seek(0x80, SeekMode.Origin);
			format.Channel0LastChunk = reader.ReadBytes(0x10);
			format.Channel1LastChunk = reader.ReadBytes(0x10);

			// For IMA ADPCM
			reader.Stream.Seek(0x80, SeekMode.Origin);
			format.Channel0Historical0 = reader.ReadUInt16();
			format.Channel0Historical1 = reader.ReadUInt16();
			format.Channel1Historical0 = reader.ReadUInt16();
			format.Channel1Historical1 = reader.ReadUInt16();
		}

		public void Export(Sadl format, DataStream strOut)
		{
			throw new NotImplementedException();
		}
	}
}

