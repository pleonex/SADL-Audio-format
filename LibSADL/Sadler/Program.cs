//
//  Program.cs
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
using System.IO;
using Libgame;
using Libgame.IO;
using LibSADL;

namespace Sadler
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length != 2)
				return;

			var stream = new DataStream(args[0], FileMode.Open, FileAccess.Read);
			var file   = new GameFile(Path.GetFileName(args[0]), stream);
			file.SetFormat(typeof(Sadl));
			file.Format.Read();
			((Sadl)file.Format).Decoder.ProgressNotifier = new ConsoleProgressNotifier("");

			PrintInfo((Sadl)file.Format);

			file.Format.Export(args[1]);
		}

		static void PrintInfo(Sadl format)
		{
			Console.WriteLine("# Audio info:");
			Console.WriteLine("\t* Name:        {0}", format.FileName);
			Console.WriteLine("\t* Date:        {0}", format.Creation);
			Console.WriteLine("\t* Codec:       {0}", format.Decoder.Name);
			Console.WriteLine("\t* Sample rate: {0}", format.SampleRate);
			Console.WriteLine("\t* Can loop?:   {0}", format.CanLoop);
		}
	}
}
