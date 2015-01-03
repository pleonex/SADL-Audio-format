//
//  Procyon.cs
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
	public class ProcyonDecoder : Decoder<Sadl>
	{
		static readonly double[,] CoefficientTable = {
			{0.00,      0.00},		// 0x00, 0x00
			{0.9375,    0.00},		// 0x3C, 0x00
			{1.796875, -0.8125},	// 0x73, 0xCC
			{1.53125,  -0.859375},	// 0x62, 0xC9
			{1.90625,  -0.9375}		// 0x7A, 0xC4
		};

		public ProcyonDecoder(Sadl format, DataStream stream)
			: base(format, stream)
		{
		}

		public override string Name {
			get { return "Procyon"; }
		}

		public override int Id {
			get { return 0xB; }
		}

		protected override short[,] DecodeBlock(int blockSize)
		{
			// Get number of chunks in this block
			int numChunks = blockSize / Format.ChunkSize;

			// Get number of samples per channel in the block
			int numSamplesChannel = (numChunks * Format.SamplesPerChunk) / Format.Channels;

			// Decode the chunk for this channel
			var samples = new short[numSamplesChannel, Format.Channels];
			for (int i = 0; i < numChunks; i++) {
				// Decode and copy
				short[] chunkSamples = DecodeChunk(i % Format.Channels);

				int index = (i / Format.Channels) * Format.SamplesPerChunk;
				for (int j = 0; j < chunkSamples.Length; j++)
					samples[j + index, i % Format.Channels] = chunkSamples[j];
			}

			return samples;
		}

		short[] DecodeChunk(int channel)
		{
			var reader  = new DataReader(RawStream);

			// Get header
			RawStream.Seek(0xF, SeekMode.Current);
			sbyte header = reader.ReadSByte();
			header = (sbyte)(header ^ 0x80);
			RawStream.Seek(-0x10, SeekMode.Current);

			// ... get scale
			byte scale = (byte)(header & 0xF);

			// ... get coefficients
			int coefIdx = header >> 4;
			double coef1 = CoefficientTable[coefIdx, 0];
			double coef2 = CoefficientTable[coefIdx, 1];

			// Get all the samples in the chunk
			uint chunkData = 0;
			var samples = new short[Format.SamplesPerChunk];
			for (int i = 0; i < Format.SamplesPerChunk; i++, chunkData >>= 4) {
				if (i % 8 == 0)
					chunkData = reader.ReadUInt32() ^ 0x80808080;

				byte value = (byte)(chunkData & 0xF);
				samples[i] = DecodeSample(value, scale, coef1, coef2, channel);
			}

			return samples;
		}

		short DecodeSample(byte val, byte scale, double coef1, double coef2, int channel)
		{
			// Convert the value to signed ans scale
			int error = (val >> 3 == 1) ? val - 0x10 : val;
			error <<= (6 + scale);

			// Predict next sample
			int prediction = (int)(Format.HistoricalValues[channel, 0] * coef1 +
				Format.HistoricalValues[channel, 1] * coef2);

			// Adjust prediction
			int sample = prediction + error;

			// Update historical values
			Format.HistoricalValues[channel, 1] = Format.HistoricalValues[channel, 0];
			Format.HistoricalValues[channel, 0] = sample;

			// Gets the output avoiding saturation
			short output = (short)(sample / 64 + 32);

			return output;
		}
	}
}

