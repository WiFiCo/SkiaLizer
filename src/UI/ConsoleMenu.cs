using System;
using System.Collections.Generic;

namespace SkiaLizer
{
	internal static class ConsoleMenu
	{
		public static int ShowMenu(string title, IList<string> options)
		{
			return ShowMenu(title, options, -1);
		}

		public static int ShowMenu(string title, IList<string> options, int specialIndex)
		{
			int selected = 0;
			bool done = false;
			while (!done)
			{
				Console.Clear();
				Console.WriteLine(title);
				for (int i = 0; i < options.Count; i++)
				{
					string keyLabel = GetKeyLabelForIndex(i);
					string line = $"[{keyLabel}] {options[i]}";
					if (i == selected)
					{
						Console.BackgroundColor = ConsoleColor.Green;
						Console.ForegroundColor = ConsoleColor.Black;
						Console.WriteLine(line);
						Console.ResetColor();
					}
					else if (i == specialIndex)
					{
						Console.ForegroundColor = ConsoleColor.Cyan;
						Console.WriteLine(line);
						Console.ResetColor();
					}
					else
					{
						Console.WriteLine(line);
					}
				}
				var key = Console.ReadKey(true).Key;

				// number keys 1-9
				if (key >= ConsoleKey.D1 && key <= ConsoleKey.D9)
				{
					int num = (int)(key - ConsoleKey.D0);
					if (num >= 1 && num <= options.Count) return num - 1;
				}
				// top-row 0 selects the 10th item if present
				if (key == ConsoleKey.D0 && options.Count >= 10)
				{
					return 9;
				}
				// numpad 0-9 (0 is the 10th item if present)
				if (key >= ConsoleKey.NumPad0 && key <= ConsoleKey.NumPad9)
				{
					int num = (int)(key - ConsoleKey.NumPad0);
					if (num >= 1 && num <= options.Count) return num - 1;
					if (num == 0 && options.Count >= 10) return 9;
				}
				// alphabet letters a..z for 11th+ (index 10 => 'a')
				if (key >= ConsoleKey.A && key <= ConsoleKey.Z)
				{
					int idx = 10 + (int)(key - ConsoleKey.A);
					if (idx >= 0 && idx < options.Count) return idx;
				}

				switch (key)
				{
					case ConsoleKey.UpArrow:
						selected = (selected - 1 + options.Count) % options.Count;
						break;
					case ConsoleKey.DownArrow:
						selected = (selected + 1) % options.Count;
						break;
					case ConsoleKey.Enter:
					case ConsoleKey.Spacebar:
						return selected;
					case ConsoleKey.Escape:
						return -1;
				}
			}
			return selected;
		}

		private static string GetKeyLabelForIndex(int index)
		{
			if (index < 9) return (index + 1).ToString();
			if (index == 9) return "0";
			int letterIndex = index - 10; // 10 => a
			char c = (char)('A' + letterIndex);
			return c.ToString();
		}
	}
}
