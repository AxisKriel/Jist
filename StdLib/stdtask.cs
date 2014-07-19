using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Wolfje.Plugins.Jist.Framework;
using Jint.Native;

namespace Wolfje.Plugins.Jist.stdlib {
	/// <summary>
	/// stdtask library.
	/// 
	/// Provides functionality to run functions asynchronously 
	/// after a certain period, or recurring over and over 
	/// again.
	/// </summary>
	public class stdtask : stdlib_base {
        protected Timer oneSecondTimer;
        private List<RecurringFunction> recurList;

        /*
         * Lock on this to prevent enumerator errors writing to
         * an enumerated object.
         */
        static readonly object __recurringLock = new object();

		public stdtask(JistEngine engine) : base(engine)
		{
			this.oneSecondTimer = new Timer(1000);
            this.oneSecondTimer.Elapsed += oneSecondTimer_Elapsed;
            this.oneSecondTimer.Start();
            this.recurList = new List<RecurringFunction>();
		}

        protected async void oneSecondTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            for (int i = 0; i < recurList.Count; i++) {
                RecurringFunction func;
                lock (__recurringLock) {
                    func = recurList.ElementAtOrDefault(i);
                }

                if (func == null) {
                    continue;
                }

                try {
                    await func.ExecuteAndRecurAsync();
                } catch (Exception ex) {
                    ScriptLog.ErrorFormat("recurring", "Error on recurring rule: " + ex.Message);
                }
            }
        }

        internal List<RecurringFunction> DumpTasks()
        {
            return recurList;
        }

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing == true) {
                this.oneSecondTimer.Stop();
				this.oneSecondTimer.Dispose();
			}
		}

		/// <summary>
		/// Runs a javascript function after waiting.  Similar to setTimeout()
		/// </summary>
		[JavascriptFunction("run_after", "jist_run_after")]
		public async void RunAfterAsync(int AfterMilliseconds, JsValue Func, params object[] args) {
            await Task.Delay((int)AfterMilliseconds);
            engine.CallFunction(Func, null, args);
		}

        /// <summary>
        /// Adds a javascript function to be run every hours minutes and seconds specified.
        /// </summary>
        [JavascriptFunction("add_recurring", "jist_task_queue")]
        public void AddRecurring(int Hours, int Minutes, int Seconds, Jint.Native.JsValue Func)
        {
            lock (__recurringLock) {
                recurList.Add(new RecurringFunction(Hours, Minutes, Seconds, Func));
            }
        }
	}

    /// <summary>
    /// Holds a Javascript recurring function and how often it executes
    /// </summary>
    class RecurringFunction {
        /// <summary>
        /// Gets the Recurrence ID of this recurrence rule, so that it may
        /// be removed at a later time
        /// </summary>
        public Guid RecurrenceID { get; private set; }

        /// <summary>
        /// Returns the total number of seconds between recurring intervals
        /// </summary>
        public int Seconds { get; private set; }

        /// <summary>
        /// Returns the Javascript function that this recurring rule should 
        /// execute.
        /// </summary>
        public Jint.Native.JsValue Function { get; private set; }

        /// <summary>
        /// Gets the next date and time that this rule is due to run at.
        /// </summary>
        public DateTime NextRunTime { get; private set; }

        /// <summary>
        /// Creates a JavaScript recurring function, with the specified 
        /// hours minutes and seconds which runs the supplied function.
        /// </summary>
        public RecurringFunction(int Hours, int Minutes, int Seconds, Jint.Native.JsValue Func)
        {
            this.RecurrenceID = Guid.NewGuid();
            this.Function = Func;

            this.Seconds += Hours * 3600;
            this.Seconds += Minutes * 60;
            this.Seconds += Seconds;

            this.NextRunTime = DateTime.UtcNow.Add(TimeSpan.FromSeconds(this.Seconds));
            //Console.WriteLine("jist recurring: Recurring Func {0}: next run time {1}", this.RecurrenceID, this.NextRunTime);
        }

        /// <summary>
        /// Executes the function in this recurring rule if it's
        /// time to, and updates the next run time to the next
        /// recurance.
        /// </summary>
        public async Task ExecuteAndRecurAsync()
        {
            if (DateTime.UtcNow < this.NextRunTime
                || Jist.JistPlugin.Instance == null) {
                    return;
            }

            try {
                await Task.Factory.StartNew(() => {
                    JistPlugin.Instance.CallFunction(Function, this);
                });
            } catch (Exception ex) {
                ScriptLog.ErrorFormat("recurring", "Error occured on a recurring task function: " + ex.Message);
            } finally {
                Recur();
            }
        }

        /// <summary>
        /// Causes this recurring function to update it's next
        /// run time to the next interval it was created with.
        /// </summary>
        protected void Recur()
        {
            this.NextRunTime = NextRunTime.Add(TimeSpan.FromSeconds(this.Seconds));
        }

        public override string ToString()
        {
            return string.Format("Task {0}: {1} secs, next in {2}", this.RecurrenceID, this.Seconds, this.NextRunTime.Subtract(DateTime.UtcNow));
        }
    }
}

