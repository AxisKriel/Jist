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
        protected Timer highPrecisionTimer;
        private List<RecurringFunction> recurList;
        private List<RunAt> runAtList;
		private List<System.Threading.CancellationTokenSource> runAfterList;

        /*
         * Lock on this to prevent enumerator errors writing to
         * an enumerated object.
         */
        private readonly object __recurringLock = new object();
        private readonly object __runAtLock = new object();

		public stdtask(JistEngine engine) : base(engine)
		{
            this.highPrecisionTimer = new Timer(100);
			this.oneSecondTimer = new Timer(1000);
            this.oneSecondTimer.Elapsed += oneSecondTimer_Elapsed;
            this.highPrecisionTimer.Elapsed += highPrecisionTimer_Elapsed;
            this.oneSecondTimer.Start();
            this.recurList = new List<RecurringFunction>();
            this.runAtList = new List<RunAt>();
			this.runAfterList = new List<System.Threading.CancellationTokenSource>();
            this.highPrecisionTimer.Start();
		}

        private void highPrecisionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            double time = 0;
            System.Threading.Interlocked.Exchange(ref time, Terraria.Main.time);
            

            if (time > 0 && time < 200) {
                for (int i = 0; i < runAtList.Count; i++) {
                    RunAt at;
                    lock (__runAtLock) {
                        at = runAtList.ElementAtOrDefault(i);
                        if (at == null) {
                            continue;
                        }

                        at.ExecutedInIteration = false;
                    }
                }
            //    TShockAPI.Log.ConsoleInfo("* Execution iterators reset.");
            }
        }

        protected void oneSecondTimer_Elapsed(object sender, ElapsedEventArgs e)
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
                    func.ExecuteAndRecur();
                } catch (Exception ex) {
                    ScriptLog.ErrorFormat("recurring", "Error on recurring rule: " + ex.Message);
                }
            }

            for (int i = 0; i < runAtList.Count; i++) {
                RunAt at;
                lock (__runAtLock) {
                    at = runAtList.ElementAtOrDefault(i);
                }

                if (at == null
                    || engine == null
                    || Terraria.Main.time <= at.AtTime
                    || at.ExecutedInIteration == true) {
                    continue;
                }

                try {
                    engine.CallFunction(at.Func, at);
                } catch (Exception ex) {
                    ScriptLog.ErrorFormat("recurring", "Error on recurring rule: " + ex.Message);
                } finally {
                    at.ExecutedInIteration = true;
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
                lock (__recurringLock) {
                    this.recurList.Clear();
                }
                this.oneSecondTimer.Stop();
				this.oneSecondTimer.Dispose();

                this.highPrecisionTimer.Stop();
                this.highPrecisionTimer.Dispose();

				this.CancelRunAfters();
			}
		}

		/// <summary>
		/// Runs a javascript function after waiting.  Similar to setTimeout()
		/// </summary>
		[JavascriptFunction("run_after", "jist_run_after")]
		public void RunAfterAsync(int AfterMilliseconds, JsValue Func, params object[] args)
		{
			System.Threading.CancellationTokenSource source;

			lock (runAfterList) {
				source = new System.Threading.CancellationTokenSource();
				runAfterList.Add(source);
			}

			Action runAfterFunc = async () => {
				try {
					await Task.Delay(AfterMilliseconds, source.Token);
				} catch (TaskCanceledException) {
					return;
				}

				if (source.Token.IsCancellationRequested == true) {
					return;
				}

				try {
					engine.CallFunction(Func, null, args);
				} catch (TaskCanceledException) {
				}

				if (source.Token.IsCancellationRequested == false) {
					lock (runAfterList) {
						runAfterList.Remove(source);
					}
				}
			};

			Task.Factory.StartNew(runAfterFunc, source.Token);
		}

		internal void CancelRunAfters()
		{
			lock (runAfterList) {
				foreach (var source in runAfterList) {
					source.Cancel();
				}

				runAfterList.Clear();
			}
		}

        /// <summary>
        /// Adds a javascript function to be run every hours minutes and seconds specified.
        /// </summary>
        [JavascriptFunction("run_at", "jist_run_at")]
        public void AddAt(int Hours, int Minutes, Jint.Native.JsValue Func)
        {
            lock (__runAtLock) {
                runAtList.Add(new RunAt(Hours, Minutes, Func));
            }
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
    /// Holds a javascript function to be run every time
    /// the game hits a certain time in the terraria world.
    /// </summary>
    class RunAt {
        public Guid RunAtID { get; set; }
        public double AtTime { get; set; }
        public JsValue Func { get; set; }
        public bool Enabled { get; set; }
        public bool ExecutedInIteration { get; set; }

        public static double GetRawTime(int hours, int minutes)
        {
            decimal time = hours + minutes / 60.0m;
            time -= 4.50m;
            if (time < 0.00m)
                time += 24.00m;
            if (time >= 15.00m) {
                return (double)((time - 15.00m) * 3600.0m);
            } else {
                return (double)(time * 3600.0m);
            }
        }

        public RunAt(double AtTime, JsValue func)
        {
            this.RunAtID = new Guid();
            this.AtTime = AtTime;
            this.Func = func;
            this.Enabled = true;
        }

        public RunAt(int hours, int minutes, JsValue func)
        {
            this.RunAtID = new Guid();
            this.AtTime = GetRawTime(hours, minutes);
            this.Func = func;
            this.Enabled = true;
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
        public void ExecuteAndRecur()
        {
            if (DateTime.UtcNow < this.NextRunTime
                || Jist.JistPlugin.Instance == null) {
                    return;
            }

            try {
                JistPlugin.Instance.CallFunction(Function, this);
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
            this.NextRunTime = DateTime.UtcNow.Add(TimeSpan.FromSeconds(this.Seconds));
        }

        public override string ToString()
        {
			TimeSpan nextRunTime = this.NextRunTime.Subtract(DateTime.UtcNow);

			return string.Format("Task {0}: {1} secs, next in {2}", this.RecurrenceID, this.Seconds, nextRunTime.ToString(@"hh\:mm\:ss"));
        }
    }
}

