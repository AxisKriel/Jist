using Jint.Native;
using Jint.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Wolfje.Plugins.Jist.Framework;
using Terraria;
using TShockAPI;

namespace Wolfje.Plugins.Jist.stdlib {
    public class std : stdlib_base {
        protected readonly Random randomGenerator = new Random();
        protected readonly object __rndLock = new object();
		protected readonly Regex csvRegex = new Regex("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");

        public std(JistEngine engine)
            : base(engine)
        {
        }

        /// <summary>
        /// JS API: random(from, to) : double
        /// tts
        /// Generates a random number between the ranges specified.
        /// </summary>
        [JavascriptFunction("random", "jist_random")]
        public double Random(double From, double To)
        {
            int from = Convert.ToInt32(From);
            int to = Convert.ToInt32(To);
            lock (__rndLock) {
                return randomGenerator.Next(from, to);
            }
        }

        /// <summary>
        /// Repeats the provided function n
        /// number of times.
        /// </summary>
        [JavascriptFunction("jist_repeat")]
        public void Repeat(int times, JsValue func)
        {
            for (int i = 0; i < times; i++) {
                engine.CallFunction(func, null, i);
            }
        }

    
		/// <summary>
		/// Execute the specified file.
		/// </summary>
		/// <param name="file">File.</param>
		[JavascriptFunction("jist_execute")]
		public void Execute(string file)
		{
			
		}

		[JavascriptFunction("jist_for_each_item")]
		public void ForEachItem(JsValue func)
		{

			for (var i = -48; i < Main.maxItemTypes; i++) {
				var item = TShock.Utils.GetItemById(i);

				if (item == null) {
					continue;
				}

				engine.CallFunction(func, null, i, item);
			}
		}


        /// <summary>
        /// Javascript function: jist_for_each_player(callback)
        /// 
        /// Invokes a callback function for every online player
        /// with the player object as the first parameter.
        /// </summary>
        [JavascriptFunction("jist_for_each_player")]
        public void ForEachOnlinePlayer(JsValue func)
        {
            foreach (var player in TShockAPI.TShock.Players) {
                if (player == null) {
                    continue;
                }

                engine.CallFunction(func, null, player);
            }
        }

		[JavascriptFunction("jist_for_each_command")]
		public void ForEachComand(JsValue Func)
		{
			var qCommands = from i in TShockAPI.Commands.ChatCommands
				let g = TShock.Groups.Where(gr => gr.HasPermission(i.Permissions.FirstOrDefault()))
			                select new {
								name = i.Name,
								permissions = string.Join(",", i.Permissions),
				gr = string.Join(" ", g.Select(asd => asd.Name))
							};

			foreach (var command in qCommands) {
				engine.CallFunction(Func, null, command);
			}
		}

		[JavascriptFunction("jist_file_append")]
		public void FileAppend(string fileName, string text)
		{
			System.IO.File.AppendAllText(fileName, text + "\r\n");
		}

		[JavascriptFunction("jist_file_delete")]
		public void FileDelete(string filePath)
		{
			System.IO.File.Delete(filePath);
		}

		[JavascriptFunction("jist_cwd")]
		public string CurrentWorkingDirectory()
		{
			return Environment.CurrentDirectory;
		}

		[JavascriptFunction("jist_file_read")]
		public void FileRead(string filePath, JsValue func)
		{
			int i = 0;
			bool hasChanged = false;
			string[] contents;

			foreach (string line in (contents = System.IO.File.ReadAllLines(filePath))) {
				string l = line.Trim();

				if (string.IsNullOrEmpty(line.Trim()) == true) {
					continue;
				}

				bool stop = false;

				try {
					engine.CallFunction(func, null, i, line.Trim(), stop);
				} catch {
					break;
				}

				if (line.Trim() != l) {
					hasChanged = true;
				}

				if (stop == true) {
					break;
				}

				i++;
			}

			if (hasChanged && contents != null) {
				System.IO.File.WriteAllLines(filePath, contents);
			}
		}

        /// <summary>
        /// JS function: jist_player_count()
        /// 
        /// Returns how many players are currently
        /// online.
        /// </summary>
        [JavascriptFunction("jist_player_count")]
        public int OnlinePlayerCount()
        {
            try {
                return TShockAPI.TShock.Players.Count(i => i != null);
            } catch {
                return -1;
            }
        }

		[JavascriptFunction("jist_file_read_lines")]
		public string[] FileReadLines(string path)
		{
			string[] lines;

			if (File.Exists(path) == false) {
				return null;
			}

			try {
				lines = File.ReadAllLines(path);
			} catch {
				return null;
			}

			return lines;
		}

		[JavascriptFunction("jist_parse_csv")]
		public string[] ReadCSV(string line)
		{
			MatchCollection matches;
			Match match;
			string[] lines;

			if (string.IsNullOrEmpty(line) == true
				|| (matches = csvRegex.Matches(line)) == null) {
				return null;
			}

			lines = new string[matches.Count];
			for (int i = 0; i < lines.Length; i++) {
				if ((match = matches[i]) == null) {
					continue;
				}

				lines[i] = match.Value;
			}

			return lines;
		}
    }
}
