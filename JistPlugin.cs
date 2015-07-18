using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using System.Reflection;
using Wolfje.Plugins.Jist.Framework;

namespace Wolfje.Plugins.Jist {
	[ApiVersion(1, 19)]
	public class JistPlugin : TerrariaPlugin {
		public static JistEngine Instance { get; protected set; }
		public override string Author { get { return "Wolfje"; } }
		public override string Description { get { return "Javascript interpreted scripting for TShock"; } }
		public override string Name { get { return "Jist"; } }
		public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

		protected JistRestInterface _restInterface;

		/// <summary>
		/// Subscribe to this event to have the opportunity to load
		/// functions into the JavaScript engine when it reloads.
		/// </summary>
		public static event EventHandler<JavascriptFunctionsNeededEventArgs> JavascriptFunctionsNeeded;

		public JistPlugin(Terraria.Main game)
			: base(game)
		{
			Order = 1;
			Instance = new JistEngine(this);
			TShockAPI.Commands.ChatCommands.Add(new TShockAPI.Command("jist.cmd", TShockAPI_JistChatCommand, "jist"));

			ServerApi.Hooks.GameInitialize.Register(this, game_initialize);
		}

		private async void TShockAPI_JistChatCommand(TShockAPI.CommandArgs args)
		{
			if (args.Parameters.Count == 0) {
				//TODO: Print help
				return;
			}

			if (args.Parameters[0].Equals("dumpenv", StringComparison.CurrentCultureIgnoreCase)) {
				if (Instance == null) {
					return;
				}

				foreach (var property in Instance.DumpGlobalEnvironment().OrderBy(i => i.Key)) {
					args.Player.SendInfoMessage("{0}: {1}", property.Key,
						property.Value.Get.HasValue == false ? "undefined" : property.Value.Get.Value.ToString());
				}
			} else if (args.Parameters[0].Equals("dumptasks", StringComparison.CurrentCultureIgnoreCase)) {
				foreach (Wolfje.Plugins.Jist.stdlib.RecurringFunction recur in Instance.stdTask.DumpTasks().OrderBy(i => i.NextRunTime)) {
					args.Player.SendInfoMessage(recur.ToString());
				}
			} else if (args.Parameters[0].Equals("eval", StringComparison.CurrentCultureIgnoreCase)
			                    || args.Parameters[0].Equals("ev", StringComparison.CurrentCultureIgnoreCase)
			                    && (args.Parameters.Count > 1)) {
				args.Player.SendInfoMessage(Instance.Eval(args.Parameters[1]));
			} else if (args.Parameters[0].Equals("reload", StringComparison.CurrentCultureIgnoreCase)
			                    || args.Parameters[0].Equals("rl", StringComparison.CurrentCultureIgnoreCase)) {
				Instance.Dispose();
				Instance = null;
				Instance = new JistEngine(this);
				await Instance.LoadEngineAsync();
				args.Player.SendInfoMessage("Jist reloaded");
			}
		}

		internal static void RequestExternalFunctions()
		{
			JavascriptFunctionsNeededEventArgs args = new JavascriptFunctionsNeededEventArgs(Instance);
			if (JavascriptFunctionsNeeded != null) {
				JavascriptFunctionsNeeded(Instance, args);
			}
		}

		/// <summary>
		/// Entry point of TerrariaServerAPI
		/// </summary>
		public override void Initialize()
		{
		}

		void game_initialize(EventArgs args)
		{
			_restInterface = new JistRestInterface(this);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing) {
				Instance.Dispose();
			}
		}
	}
}
