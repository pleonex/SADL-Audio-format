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
	public class SadlBinaryConverter : IConverter<Sadl, BinaryFormat>
	{
		public void Import(BinaryFormat bin, Sadl format)
		{
			var reader = new DataReader(bin.Stream);

			string magicStamp = reader.ReadString(4);
			if (magicStamp != Sadl.MagicStamp)
				throw new FormatException();

			reader.ReadUInt32(); // Null

			// File info
			format.FileSize  = reader.ReadUInt32();
			format.Unknown0C = reader.ReadUInt32();
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

			byte codecInfo    = reader.ReadByte();
			int codec = codecInfo >> 4;
			format.SampleRate = (codecInfo & 0x0F) * 8000; // Only allowed 2 and 4
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
			format.DataSize    -= format.StartOffset;	// Since it's include the header
			format.Unknown4C    = reader.ReadUInt32();
			format.Unknown50    = reader.ReadUInt32();
			format.LoopOffset   = reader.ReadUInt32();
			format.LoopDataSize = reader.ReadUInt32() - format.LoopOffset;
			reader.ReadUInt32(); // Null

			format.Unknown60 = reader.ReadByte();
			format.Unknown61 = reader.ReadByte();
			format.Unknown62 = reader.ReadByte();
			format.Unknown63 = reader.ReadByte();
			format.Unknown64 = reader.ReadByte();
			format.Unknown65 = reader.ReadByte();
			format.Unknown66 = reader.ReadUInt16();
			format.Unknown68 = reader.ReadByte();
			format.Unknonw69 = reader.ReadByte();

			// For Procyon
			reader.Stream.Seek(0x80, SeekMode.Origin);
			format.Channel0LastChunk = reader.ReadBytes(0x10);
			format.Channel1LastChunk = reader.ReadBytes(0x10);

			// For IMA ADPCM
			reader.Stream.Seek(0x80, SeekMode.Origin);
			format.HistoricalValues = new int[2, 2];
			format.HistoricalValues[0, 0] = reader.ReadUInt16();
			format.HistoricalValues[0, 1] = reader.ReadUInt16();
			format.HistoricalValues[1, 0] = reader.ReadUInt16();
			format.HistoricalValues[1, 1] = reader.ReadUInt16();

			var audioStream = new DataStream(bin.Stream, format.StartOffset, format.DataSize);
			format.Decoder  = (codec == 0xB) ? new ProcyonDecoder(format, audioStream) : null;
		}

		public void Export(Sadl format, BinaryFormat bin)
		{
			var writer = new DataWriter(bin.Stream);
			var audioStream = format.Decoder.RawStream;

			// Header
			writer.Write(Sadl.MagicStamp);
			writer.Write((uint)0x00);
			writer.Write((uint)(0x100 + audioStream.Length));
			writer.Write(format.Unknown0C);
			writer.Write((ulong)0x00);

			// File info
			DateTime date = format.Creation;
			writer.Write((ushort)date.Year);
			writer.Write((byte)date.Month);
			writer.Write((byte)date.Day);
			writer.Write((uint)
				((date.Hour * 3600 + date.Minute * 60 + date.Second) * 1000 + date.Millisecond));

			writer.Write(format.FileName, 0x10);

			// Audio info
			writer.Write(format.Unknown30);
			writer.Write((byte)(format.CanLoop ? 1 : 0));
			writer.Write((byte)format.Channels);

			byte codecInfo = (byte)(format.Decoder.Id << 4);
			codecInfo |= (byte)((format.SampleRate == 32000) ? 4 : 2);
			writer.Write(codecInfo);
			writer.Write((byte)0x00);

			writer.Write(format.Unknown35);
			writer.Write(format.Unknown36);
			writer.Write(format.Unknown38);

			writer.Write(format.ChunkSize);
			writer.Write(format.SamplesPerChunk);
			writer.Write(format.SamplesSizePerChunk);

			writer.Write((uint)(0x100 + audioStream.Length));
			writer.Write(format.Unknown44);
			writer.Write((uint)0x100);
			writer.Write(format.Unknown4C);
			writer.Write(format.Unknown50);
			writer.Write((uint)0x100);
			writer.Write((uint)(0x100 + audioStream.Length));
			writer.Write((uint)0x00);

			writer.Write(format.Unknown60);
			writer.Write(format.Unknown61);
			writer.Write(format.Unknown62);
			writer.Write(format.Unknown63);
			writer.Write(format.Unknown64);
			writer.Write(format.Unknown65);
			writer.Write(format.Unknown66);
			writer.Write(format.Unknown68);
			writer.Write(format.Unknonw69);

			writer.Write((ushort)0x00);
			writer.Write((uint)0x00);
			writer.Write((ulong)0x00);
			writer.Write((ulong)0x00);

			writer.Write(new byte[0x10]);
			writer.Write(new byte[0x10]);

			writer.Write(new byte[0x60]);

			audioStream.WriteTo(bin.Stream);
		}
	}
}

