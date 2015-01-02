//
//  SadlWavConverter.cs
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
using System.IO;

namespace LibSADL
{
	public class SadlWavConverter : IConverter<Sadl, Wave>
	{
		public void Import(Wave strIn, Sadl format)
		{
			throw new NotImplementedException();
		}

		public void Export(Sadl format, Wave wav)
		{
			var sampleStream = new DataStream(new MemoryStream(), 0, 0);

			// Create wave file
			wav.BitsPerSample = 16;
			wav.Channels      = format.Channels;
			wav.SampleRate    = format.SampleRate;
			wav.Decoder = new PcmDecoder(wav, sampleStream);

			// Encode samples
			var encoder = new PcmEncoder(format.Decoder.Run(), wav);
			encoder.Run(sampleStream);
		}
	}
}

