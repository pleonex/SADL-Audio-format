//
//  ICodec.cs
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
using Libgame.IO;
using System;

namespace LibSADL
{
	public abstract class Decoder
	{
		protected Decoder(Sadl format)
		{
			Format = format;
		}

		public Sadl Format { get; private set;}

		public abstract string Name { get; }

		public void Decode(DataStream strOut)
		{
			if (Format.Channels <= 0)
				return;

			var writer = new DataWriter(strOut);

			int progress = 0;
			uint decodedSize = 0;
			while (decodedSize < Format.AudioStream.Length) {
				// Calculate the size of the block
				int blockLen = 0x2000;
				if (decodedSize + blockLen > Format.AudioStream.Length)
					blockLen = (int)(Format.AudioStream.Length - decodedSize);

				// Decode
				var samples = new short[Format.Channels][];
				for (int i = 0; i < Format.Channels; i++) {
					Format.AudioStream.Seek(decodedSize, SeekMode.Origin);
					samples[i] = DecodeBlock(blockLen, i);
				}

				// And mix channels
				for (int s = 0; s < samples[0].Length; s++)
					for (int c = 0; c < Format.Channels; c++)
						writer.Write(samples[c][s]);

				// Increase decoded size
				decodedSize += (uint)blockLen;

				// Show progress
				int newProgress = (int)(100 * decodedSize / Format.AudioStream.Length);
				if (newProgress >= progress + 5) {
					progress = newProgress;
					Console.WriteLine("Decoded {0}%", progress);
				}
			}
		}

		public abstract short[] DecodeBlock(int blockSize, int channel);
	}
}

