//
//  ConsoleProgressNotifier.cs
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
using System.Diagnostics;

namespace LibSADL
{
	public class ConsoleProgressNotifier : IProgressNotifier
	{
		Stopwatch watch;
		string message;
		int currentProgress;
		int posX;
		int posY;

		public ConsoleProgressNotifier(string message)
		{
			this.message = message;
			watch = new Stopwatch();
		}

		public void Reset()
		{
			currentProgress = 0;
			posX = Console.CursorLeft;
			posY = Console.CursorTop;
			Console.WriteLine(message, 0);

			if (watch.IsRunning)
				watch.Stop();

			watch.Start();
		}

		public void Update(int progress)
		{
			if (progress >= currentProgress + 5) {
				currentProgress = progress;

				Console.SetCursorPosition(posX, posY);
				Console.WriteLine(message, progress);
			}
		}

		public void Update(long current, long total)
		{
			Update((int)(100 * current / total));
		}

		public void End()
		{
			watch.Stop();
			Console.WriteLine("Done in {0}", watch.Elapsed);
		}
	}
}

