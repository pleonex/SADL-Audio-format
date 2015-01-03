//
//  ProcyonEncoder.cs
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
using Libgame.IO;

namespace LibSADL
{
	public class ProcyonEncoder : IEncoder
	{
		static readonly double[,] CoefficientTable = {
			{0.00,      0.00},		// 0x00, 0x00
			{0.9375,    0.00},		// 0x3C, 0x00
			{1.796875, -0.8125},	// 0x73, 0xCC
			{1.53125,  -0.859375},	// 0x62, 0xC9
			{1.90625,  -0.9375}		// 0x7A, 0xC4
		};

		public ProcyonEncoder(IEnumerable<short[,]> samples, Sadl format)
		{
			Samples = samples;
			Format  = format;
		}

		public string Name {
			get { return "procyon"; }
		}

		public IEnumerable<short[,]> Samples {
			get;
			private set;
		}

		public IProgressNotifier ProgressNotifier { get; set; }
		public int SamplesToEncode { get; set; }

		public Sadl Format {
			get;
			private set;
		}

		public void Run(DataStream strOut)
		{
			if (ProgressNotifier != null)
				ProgressNotifier.Reset();

			for (int i = 0; i < Format.Channels; i++) {
				Format.HistoricalValues[i, 0] = 0;
				Format.HistoricalValues[i, 1] = 0;
			}

			int samplesEncoded = 0;
			var samples = new SampleEnumerator(Samples);
			foreach (var sampleBlock in samples.Get(Format.SamplesPerChunk)) {
				// Mix chunk channels
				for (int i = 0; i < Format.Channels; i++)
					WriteChunk(sampleBlock, i, strOut);

				samplesEncoded += sampleBlock.GetLength(0);
				if (ProgressNotifier != null)
					ProgressNotifier.Update(samplesEncoded, SamplesToEncode);
			}

			if (ProgressNotifier != null)
				ProgressNotifier.End();
		}

		void WriteChunk(short[,] samples, int channel, DataStream strOut)
		{
			var writer = new DataWriter(strOut);

			byte scale;
			byte coefIdx;
			byte[] values = SearchBestParameters(samples, channel, out scale, out coefIdx);

			uint currentWord = 0;
			for (int i = 0; i < Format.SamplesPerChunk; i++) {
				if (i != 0 && i % 8 == 0) {
					currentWord ^= 0x80808080;
					writer.Write(currentWord);
					currentWord = 0;
				}

				byte val = (i >= values.Length) ? (byte)0 : (byte)(values[i] & 0xF);
				currentWord |= (uint)(val << (i * 4));
			}

			// Set the header
			byte header = (byte)((coefIdx << 4) | scale);
			currentWord |= (uint)(header << 24);

			// Write the last word
			currentWord ^= 0x80808080;
			writer.Write(currentWord);
		}

		byte EncodeSample(short sample, byte scale, double coef1, double coef2,
			int channel, out int diff)
		{
			// Undo last operation
			int value = (sample - 32) * 64;

			// Predic sample
			int prediction = (int)(Format.HistoricalValues[channel, 0] * coef1 +
				Format.HistoricalValues[channel, 1] * coef2);

			// Get the error from prediction
			int error = value - prediction;

			// Scale error
			int errorScaled = (int)Math.Floor((double)error / (1 << (6 + scale)));

			// Set the limit of 4-bit for the error
			byte result = (byte)(errorScaled & 0xF);

			// Go back to get the difference between this coded value and the original
			int errorApprox = (result >> 3 == 1) ? result - 0x10 : result;
			errorApprox <<= (6 + scale);

			// Update historical values
			Format.HistoricalValues[channel, 1] = Format.HistoricalValues[channel, 0];
			Format.HistoricalValues[channel, 0] = prediction + errorApprox;

			short sampleApprox = (short)(((prediction + errorApprox) / 64) + 32);
			diff = Math.Abs(sample - sampleApprox);

			return result;
		}

		byte[] SearchBestParameters(short[,] samples, int channel, out byte scale, out byte coefIdx)
		{
			// Default assignment
			scale   = 0;
			coefIdx = 0;

			int currentHist0 = Format.HistoricalValues[channel, 0];
			int currentHist1 = Format.HistoricalValues[channel, 1];
			int newHist0 = 0;
			int newHist1 = 0;

			const int numCoefficient = 5;
			const int numScales = 16;

			// Compute the difference between decoded and encoded and get best params
			byte[] encoded = null;
			int currentDiff = Int32.MaxValue;
			for (byte tempCoefIdx = 0; tempCoefIdx < numCoefficient; tempCoefIdx++) {
				for (byte tempScale = 0; tempScale < numScales; tempScale++) {
					Format.HistoricalValues[channel, 0] = currentHist0;
					Format.HistoricalValues[channel, 1] = currentHist1;

					// Get the difference for this values
					byte[] result;
					int difference = GetEncodingDifference(samples, channel, tempCoefIdx,
						tempScale, out result);

					// If it's better, set it
					if (difference < currentDiff) {
						currentDiff = difference;
						scale   = tempScale;
						coefIdx = tempCoefIdx;
						encoded = result;
						newHist0 = Format.HistoricalValues[channel, 0];
						newHist1 = Format.HistoricalValues[channel, 1];
					}

					// If it's the best as possible, set it
					if (difference == 0)
						return encoded;
				}
			}

			Format.HistoricalValues[channel, 0] = newHist0;
			Format.HistoricalValues[channel, 1] = newHist1;
			return encoded;
		}

		int GetEncodingDifference(short[,] samples, int channel, byte coefIdx, byte scale,
			out byte[] result)
		{
			int numSamples = samples.GetLength(0);
			result = new byte[numSamples];

			double coef1 = CoefficientTable[coefIdx, 0];
			double coef2 = CoefficientTable[coefIdx, 1];

			int difference = 0;
			for (int i = 0; i < numSamples; i++) {
				// Encode the original sample
				int diff;
				short sample = samples[i, channel];
				result[i] = EncodeSample(sample, scale, coef1, coef2, channel, out diff);

				difference += diff;
			}

			return difference;
		}
	}
}

