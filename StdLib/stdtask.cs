using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Wolfje.Plugins.Jist.Framework;
using Jint.Native;

namespace Wolfje.Plugins.Jist {
	/// <summary>
	/// stdtask library.
	/// 
	/// Provides functionality to run functions asynchronously 
	/// after a certain period, or recurring over and over 
	/// again.
	/// </summary>
	class stdtask : stdlib.stdlib_base {
		protected Timer oneSecondTimer;

		public stdtask(JistEngine engine) : base(engine)
		{
			this.oneSecondTimer = new Timer(1000);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing == true) {
				this.oneSecondTimer.Dispose();
			}
		}

		public override void SubmitFunctions()
		{
			engine.CreateScriptFunctions(this.GetType(), this);
		}

		/// <summary>
		/// Runs a javascript function after waiting.  Similar to setTimeout()
		/// </summary>
		[JavascriptFunction("run_after", "jist_run_after")]
		public async void RunAfterAsync(double AfterMilliseconds, JsValue Func, params object[] args) {
			await Task.Delay((int)AfterMilliseconds);
			engine.CallFunction(Func, null, args);
		}
	}
}

