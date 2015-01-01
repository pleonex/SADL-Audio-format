//
//  WaveBinaryConverter.cs
//
//  Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
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
	public class WaveBinaryConverter : IConverter<Wave>
	{
		public void Import(DataStream strIn, Wave format)
		{
			var reader = new DataReader(strIn);

			// Read RIFF header
			if (reader.ReadString(4) != Wave.MagicHeader) 
				throw new FormatException();

			reader.ReadUInt32();	// File size - 8

			if (reader.ReadString(4) != Wave.RiffFormat)
				throw new FormatException();

			// Sub-chunk 'fmt'
			if (reader.ReadString(4) != "fmt")
				throw new FormatException();

			reader.ReadUInt32();	// sub-chunk size
			format.Decoder       = reader.ReadUInt16() == 1 ? new PcmDecoder(format) : null;
			format.Channels      = reader.ReadUInt16();
			format.SampleRate    = reader.ReadInt32();
			format.ByteRate      = reader.ReadInt32();
			format.BlockAlign    = reader.ReadUInt16();
			format.BitsPerSample = reader.ReadUInt16();

			// Sub-chunk 'data'
			if (reader.ReadString(4) != "data")
				throw new FormatException();

			uint dataSize = reader.ReadUInt32();
			format.AudioStream = new DataStream(strIn, strIn.Position, dataSize);
		}

		public void Export(Wave format, DataStream strOut)
		{
			throw new NotImplementedException();
		}
	}
}

