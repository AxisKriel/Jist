using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.Jist {
    public class ScriptLog {

        static readonly object __lockSyncLock = new object();

        public static void PrintSuccess(string MessageFormat, params object[] args) {
            lock (__lockSyncLock) {
		int defaultWindowWidth = default(int);
                string s = string.Format(MessageFormat, args);

                ConsoleColor origColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;

		try {
			defaultWindowWidth = Console.WindowWidth;
                	Console.SetCursorPosition(defaultWindowWidth - s.Length - 3, Console.CursorTop);
		} catch {
			Console.Write('\t');
			defaultWindowWidth = 80;
		}

                Console.WriteLine(s);
                Console.ForegroundColor = origColour;
            }
        }

        public static void PrintError(string MessageFormat, params object[] args) {
            lock (__lockSyncLock) {
		int defaultWindowWidth = default(int);
                string s = string.Format(MessageFormat, args);

                ConsoleColor origColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

		try {
			defaultWindowWidth = Console.WindowWidth;
                	Console.SetCursorPosition(defaultWindowWidth - s.Length - 3, Console.CursorTop);
		} catch {
			Console.Write('\t');
			defaultWindowWidth = 80;
		}

		Console.WriteLine(s);
                Console.ForegroundColor = origColour;
            }
        }

        public static void InfoFormat(string ScriptName, string MessageFormat, params object[] args) {
            lock (__lockSyncLock) {
                ConsoleColor origColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[jist {0}] ", ScriptName);
                Console.ForegroundColor = origColour;
                Console.Write(MessageFormat, args);
            }
        }

        public static void InfoLineFormat(string ScriptName, string MessageFormat, params object[] args) {
            lock (__lockSyncLock) {
                ConsoleColor origColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[jist {0}] ", ScriptName);
                Console.ForegroundColor = origColour;
                Console.WriteLine(MessageFormat, args);
            }
        }

        public static void ErrorFormat(string ScriptName, string MessageFormat, params object[] args) {
            lock (__lockSyncLock) {
                ConsoleColor origColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[jist {0} error] ", ScriptName);
                Console.WriteLine(MessageFormat, args);
                Console.ForegroundColor = origColour;
            }
        }

    }
}
