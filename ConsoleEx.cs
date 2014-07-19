using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.Jist {
    static class ConsoleEx {
        static readonly object __consoleWriteLock = new object();

		public static void WriteBar(PercentChangedEventArgs args)
		{
			StringBuilder output = new StringBuilder();
			int fillLen = 0;
			char filler = '#', spacer = ' ';

			output.Append(" ");
			for (int i = 0; i < 10; i++) {
				char c = i < args.Label.Length ? args.Label[i] : ' ';
				output.Append(c);
			}
			
			output.Append(" [");
			
			fillLen = Convert.ToInt32(((decimal)args.Percent / 100) * 60);

			for (int i = 0; i < 60; i++) {
				output.Append(i <= fillLen ? filler : spacer);
			}

			output.Append("] ");
			output.Append(args.Percent + "%");

			lock (__consoleWriteLock) {
				Console.Write("\r");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write(output.ToString());
				Console.ResetColor();
			}
		}
    }

    class PercentChangedEventArgs : EventArgs {
        public string Label { get; set; }
        public decimal Percent { get; set; }
    }
}
