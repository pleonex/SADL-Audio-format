//
//  SampleEnumerator.cs
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
using System.Collections.Generic;

namespace LibSADL
{
	public class SampleEnumerator
	{
		public SampleEnumerator(IEnumerable<short[,]> samples)
		{
			Samples = samples;
		}

		public IEnumerable<short[,]> Samples { get; set; }

		public IEnumerable<short[,]> Get(int size)
		{
			int requestSize = 0;
			int bufferIdx   = 0;
			short[,] buffer = null;

			foreach (var block in Samples) {
				// Now we have some block, we can now its length
				if (buffer == null) {
					buffer = new short[size, block.GetLength(1)];
					requestSize = size * block.GetLength(1);
				}

				// Copy the block into the buffer until it's empty
				int blockSize = block.GetLength(0) * block.GetLength(1);
				int blockIdx  = 0;
				do {
					int toCopy = (bufferIdx + blockSize < requestSize) ?
						blockSize : requestSize - bufferIdx;
					Array.Copy(block, blockIdx, buffer, bufferIdx, toCopy);

					bufferIdx += toCopy;
					blockIdx  += toCopy;
					blockSize -= toCopy;

					if (bufferIdx == requestSize) {
						bufferIdx = 0;
						yield return buffer;
					}
				} while (blockSize > 0);
			}

			if (buffer != null) {
				var tail = new short[bufferIdx / buffer.GetLength(1), buffer.GetLength(1)];
				Array.Copy(buffer, 0, tail, 0, bufferIdx);
				yield return tail;
			}
		}
	}
}

