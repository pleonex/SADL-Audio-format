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
using System.Collections.Generic;

namespace LibSADL
{
	public abstract class Decoder<T> : IDecoder
		where T : SoundFormat
	{
		protected Decoder(T format, DataStream stream)
		{
			Format    = format;
			RawStream = stream;
		}

		public T Format { get; private set;}

		public DataStream RawStream { get; private set; }
			
		public abstract string Name { get; }

		public abstract int Id { get; }

		public IEnumerable<short[,]> Run()
		{
			if (Format.Channels <= 0)
				yield break;

			RawStream.Seek(0, SeekMode.Origin);
			int progress = 0;
			uint decodedSize = 0;
			while (decodedSize < RawStream.Length) {
				// Calculate the size of the block
				int blockLen = 0x2000;
				if (decodedSize + blockLen > RawStream.Length)
					blockLen = (int)(RawStream.Length - decodedSize);

				// Decode
				yield return DecodeBlock(blockLen);

				// Increase decoded size
				decodedSize += (uint)blockLen;

				// Show progress
				int newProgress = (int)(100 * decodedSize / RawStream.Length);
				if (newProgress >= progress + 5) {
					progress = newProgress;
					Console.WriteLine("Decoded {0}%", progress);
				}
			}
		}

		protected abstract short[,] DecodeBlock(int blockSize);
	}
}

