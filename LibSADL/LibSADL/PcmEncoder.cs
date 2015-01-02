//
//  PcmEncoder.cs
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
using System.Collections.Generic;

namespace LibSADL
{
	public class PcmEncoder : IEncoder
	{
		public PcmEncoder(IEnumerable<short[,]> samples, Wave format)
		{
			Samples = samples;
			Format  = format;
		}

		public string Name {
			get { return "pcm"; }
		}

		public IEnumerable<short[,]> Samples {
			get;
			private set;
		}

		public Wave Format {
			get;
			private set;
		}

		public void Run(DataStream strOut)
		{
			var writer = new DataWriter(strOut);

			// For each block of samples
			foreach (var sample in Samples) {
				// Mix channels
				for (int i = 0; i < sample.GetLength(0); i++)
					for (int c = 0; c < sample.GetLength(1); c++)
						writer.Write(sample[i, c]);
			}
		}
	}
}

