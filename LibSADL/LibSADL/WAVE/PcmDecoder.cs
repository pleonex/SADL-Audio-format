//
//  PcmDecoder.cs
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
	public class PcmDecoder : Decoder<Wave>
	{
		public PcmDecoder(Wave format, DataStream stream)
			: base(format, stream)
		{
		}

		public override string Name {
			get { return "PCM"; }
		}

		public override int Id {
			get { return 1; }
		}

		protected override short[,] DecodeBlock(int blockSize)
		{
			int bytesPerSample = Format.BitsPerSample / 8;
			int numSamples = (blockSize / bytesPerSample) / Format.Channels;
			var samples = new short[numSamples, Format.Channels];

			var reader = new DataReader(RawStream);
			for (int i = 0; i < numSamples; i++) {
				for (int c = 0; c < Format.Channels; c++)
					samples[i, c] = reader.ReadInt16();
			}

			return samples;
		}
	}
}

