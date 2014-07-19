using Jint.Native;
using Jint.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolfje.Plugins.Jist.Framework;

namespace Wolfje.Plugins.Jist.stdlib {
	public class std : stdlib_base {
		protected readonly Random randomGenerator = new Random();
		protected readonly object __rndLock = new object();

		public std(JistEngine engine)
			: base(engine)
		{
        }

		/// <summary>
		/// JS API: random(from, to) : double
		/// 
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
        /// Javascript function: describe(object) : string
        /// 
        /// Prints handy debug information about the extent 
        /// of an object provided to it.
        /// 
        /// Only use in debugging scripts.
        /// </summary>
        [JavascriptFunction("dump", "describe")]
        public string Dump(JsValue value)
        {
            try {
                object valueObject = value.ToObject();
                return JsonConvert.SerializeObject(valueObject, Formatting.Indented);
            } catch {
                ScriptLog.ErrorFormat("describe", "Object cannot be described.");
            }

            return null;
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
	}
}
