﻿//
//  Wave.cs
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
using Libgame.IO;
using System;

namespace LibSADL
{
	public class Wave : SoundFormat
	{
		IConverter<Wave, BinaryFormat> converter = new WaveBinaryConverter();

		public override string FormatName {
			get { return "sound.wave";	}
		}

		public static string MagicHeader { get { return "RIFF"; } }
		public static string RiffFormat  { get { return "WAVE"; } }

		public override IDecoder Decoder { get; set; }
		public override int Channels     { get; set; }
		public override int SampleRate   { get; set; }

		public int BitsPerSample { get; set; }

		public int ByteRate { 
			get { return Channels * SampleRate * BitsPerSample / 8; }
		}

		public int FullSampleSize {
			get { return Channels * BitsPerSample / 8; }
		}

		public override void Read(DataStream strIn)
		{
			var bin = new BinaryFormat(strIn);
			converter.Import(bin, this);
		}

		public override void Write(DataStream strOut)
		{
			var bin = new BinaryFormat(strOut);
			converter.Export(this, bin);
		}

		public override void Import(params DataStream[] strIn)
		{
			throw new NotSupportedException();
		}

		public override void Export(params DataStream[] strOut)
		{
			throw new NotSupportedException();
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
			if (freeManagedResourcesAlso)
				Decoder.Dispose();
		}
	}
}

