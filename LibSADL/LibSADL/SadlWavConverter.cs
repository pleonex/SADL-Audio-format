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
	public class SadlWavConverter : IConverter<Sadl>
	{
		public void Import(DataStream strIn, Sadl format)
		{
			throw new NotImplementedException();
		}

		public void Export(Sadl format, DataStream strOut)
		{
			// Decode samples
			var sampleStream = new DataStream(new MemoryStream(), 0, 0);
			format.Decoder.Decode(sampleStream);

			// Create wave file
			var wav = new Wave();
			wav.AudioStream   = sampleStream;
			wav.BitsPerSample = 16;
			wav.Channels      = format.Channels;
			wav.Decoder       = new PcmDecoder(wav);
			wav.SampleRate    = format.SampleRate;

			// Write file
			new WaveBinaryConverter().Export(wav, strOut);

			sampleStream.Dispose();
		}
	}
}

