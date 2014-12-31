﻿//
//  Sadl.cs
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
using Libgame;
using Libgame.IO;

namespace LibSADL
{
	public class Sadl : Format
	{
		IConverter<Sadl> binaryConverter;
		IConverter<Sadl> wavConverter;

		public static string MagicStamp { 
			get { return "sadl"; }
		}

		public override string FormatName {
			get { return "SADL.sadl"; }
		}

		public string FileName { get; set; }

		public int Channels { get; set; }

		public byte Codec      { get; set; }
		public int  SampleRate { get; set; }
		public byte ChunkSize  { get; set; }

		public byte[] Channel0LastChunk   { get; set; }
		public ushort Channel0Historical0 { get; set; }
		public ushort Channel0Historical1 { get; set; }
		public byte[] Channel1LastChunk   { get; set; }
		public ushort Channel1Historical0 { get; set; }
		public ushort Channel1Historical1 { get; set; }

		public bool HasLoop      { get; set; }
		public uint StartOffset  { get; set; }
		public uint LoopOffset   { get; set; }
		public uint DataSize     { get; set; }
		public uint LoopDataSize { get; set; }

		public byte Unknown1 { get; set; }
		public byte Unknown2 { get; set; }
		public byte Unknown3 { get; set; }
		public byte Unknown4 { get; set; }

		public override void Initialize(GameFile file, params object[] parameters)
		{
			this.binaryConverter = new SadlBinaryConverter();
			this.wavConverter    = new SadlWavConverter();
		}

		public override void Read(DataStream strIn)
		{
			this.binaryConverter.Import(strIn, this);
		}

		public override void Write(DataStream strOut)
		{
			this.binaryConverter.Export(this, strOut);
		}

		public override void Import(params DataStream[] strIn)
		{
			this.wavConverter.Import(strIn[0], this);
		}

		public override void Export(params DataStream[] strOut)
		{
			this.wavConverter.Export(this, strOut[0]);
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
		}
	}
}

