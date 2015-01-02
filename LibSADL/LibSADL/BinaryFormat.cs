//
//  BinaryFormat.cs
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
	public class BinaryFormat : Format
	{
		public BinaryFormat()
		{
		}

		public BinaryFormat(DataStream stream)
		{
			Stream = stream;
		}

		public override string FormatName {
			get { return "base.binary"; }
		}

		public DataStream Stream { get; private set; }

		public override void Read(DataStream strIn)
		{
			Stream = new DataStream(strIn, 0, strIn.Length);
		}

		public override void Write(DataStream strOut)
		{
			Stream.WriteTo(strOut);
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
				Stream.Dispose();
		}
	}
}

