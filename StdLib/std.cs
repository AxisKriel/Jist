using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolfje.Plugins.Jist.Framework;

namespace Wolfje.Plugins.Jist.stdlib {
	class std : stdlib_base {
		protected readonly Random randomGenerator = new Random();
		protected readonly object __rndLock = new object();

		public std(JistEngine engine)
			: base(engine)
		{
			
		}

		public override void SubmitFunctions()
		{
			engine.CreateScriptFunctions(this.GetType(), this);
		}

		/// <summary>
		/// JS API: random(from, to) : double
		/// 
		/// Generates a random number between the ranges specified.
		/// </summary>
		[JavascriptFunction("random", "jist_random")]
		protected double Random(double From, double To)
		{
			int from = Convert.ToInt32(From);
			int to = Convert.ToInt32(To);
			lock (__rndLock) {
				return randomGenerator.Next(from, to);
			}
		}
	}
}
