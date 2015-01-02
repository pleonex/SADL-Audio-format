//
//  WaveBinaryConverter.cs
//
//  Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
//       Specification from: https://ccrma.stanford.edu/courses/422/projects/WaveFormat/
//
//  Copyright (c) 2015 Benito Palacios Sánchez
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
	public class WaveBinaryConverter : IConverter<Wave, BinaryFormat>
	{
		public void Import(BinaryFormat bin, Wave format)
		{
			var reader = new DataReader(bin.Stream);

			// Read RIFF header
			if (reader.ReadString(4) != Wave.MagicHeader) 
				throw new FormatException();

			reader.ReadUInt32();	// File size - 8

			if (reader.ReadString(4) != Wave.RiffFormat)
				throw new FormatException();

			// Sub-chunk 'fmt'
			if (reader.ReadString(4) != "fmt ")
				throw new FormatException();

			reader.ReadUInt32();	// sub-chunk size
			ushort audioCodec = reader.ReadUInt16();
			format.Channels      = reader.ReadUInt16();
			format.SampleRate    = reader.ReadInt32();
			reader.ReadInt32();		// Byte rate
			reader.ReadUInt16();	// Full sample size
			format.BitsPerSample = reader.ReadUInt16();

			// Sub-chunk 'data'
			if (reader.ReadString(4) != "data")
				throw new FormatException();

			uint dataSize = reader.ReadUInt32();
			var audioStream = new DataStream(bin.Stream, bin.Stream.Position, dataSize);
			format.Decoder  = (audioCodec == 1) ? new PcmDecoder(format, audioStream) : null;
		}

		public void Export(Wave format, BinaryFormat bin)
		{
			var audioStream = format.Decoder.RawStream;

			var writer = new DataWriter(bin.Stream);
			writer.Write(Wave.MagicHeader);
			writer.Write((uint)(36 + audioStream.Length));
			writer.Write(Wave.RiffFormat);

			// Sub-chunk 'fmt'
			writer.Write("fmt ");
			writer.Write((uint)16);		// Sub-chunk size
			writer.Write((ushort)1);	// Audio format
			writer.Write((ushort)format.Channels);
			writer.Write(format.SampleRate);
			writer.Write(format.ByteRate);
			writer.Write((ushort)format.FullSampleSize);
			writer.Write((ushort)format.BitsPerSample);

			// Sub-chunk 'data'
			writer.Write("data");
			writer.Write((uint)(audioStream.Length));
			audioStream.WriteTo(bin.Stream);
		}
	}
}

