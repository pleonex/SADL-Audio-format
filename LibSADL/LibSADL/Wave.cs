//
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
using System;
using Libgame;
using Libgame.IO;

namespace LibSADL
{
	public class Wave : Format
	{
		IConverter<Wave> converter = new WaveBinaryConverter();

		public override string FormatName {
			get { return "sound.wave";	}
		}

		public static string MagicHeader { get { return "RIFF"; } }
		public static string RiffFormat  { get { return "WAVE"; } }

		public PcmDecoder Decoder { get; set; }

		public int Channels      { get; set; }
		public int SampleRate    { get; set; }
		public int ByteRate      { get; set; }
		public int BlockAlign    { get; set; }
		public int BitsPerSample { get; set; }

		public DataStream AudioStream { get; set; }

		public override void Read(DataStream strIn)
		{
			converter.Import(strIn, this);
		}

		public override void Write(DataStream strOut)
		{
			converter.Export(this, strOut);
		}

		public override void Import(params DataStream[] strIn)
		{
			converter.Import(strIn[0], this);
		}

		public override void Export(params DataStream[] strOut)
		{
			converter.Export(this, strOut[0]);
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
		}
	}
}

