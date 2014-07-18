using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Wolfje.Plugins.Jist.Extensions {
	public static class TSPlayerExtensions {

		/// <summary>
		/// Sends a message with the specified format and colour to a player.
		/// </summary>
		public static void SendMessageFormat(this TSPlayer Player, Color Colour, string MessageFormat, params object[] args)
		{
			Player.SendMessage(string.Format(MessageFormat, args), Colour);
		}

		/// <summary>
		/// This is a copy of TShocks handlecommand method, sans the permission checks
		/// </summary>
		public static bool PermissionlessInvoke(this TShockAPI.TSPlayer player, string text)
		{
			IEnumerable<TShockAPI.Command> cmds;
			List<string> args;
			string cmdName;
			string cmdText;

			if (string.IsNullOrEmpty(text)) {
				return false;
			}

			cmdText = text.Remove(0, 1);
			args = typeof(TShockAPI.Commands).CallPrivateMethod<List<string>>(true, "ParseParameters", cmdText);

			if (args.Count < 1) {
				return false;
			}

			cmdName = args[0].ToLower();
			args.RemoveAt(0);
			cmds = TShockAPI.Commands.ChatCommands.Where(c => c.HasAlias(cmdName));

			if (Enumerable.Count(cmds) == 0) {
				if (player.AwaitingResponse.ContainsKey(cmdName)) {
					Action<TShockAPI.CommandArgs> call = player.AwaitingResponse[cmdName];
					player.AwaitingResponse.Remove(cmdName);
					call(new TShockAPI.CommandArgs(cmdText, player, args));
					return true;
				}
				player.SendErrorMessage("Invalid command entered. Type /help for a list of valid commands.");
				return true;
			}

			foreach (TShockAPI.Command cmd in cmds) {
				if (!cmd.AllowServer && !player.RealPlayer) {
					player.SendErrorMessage("You must use this command in-game.");
				} else {
					if (cmd.DoLog)
						TShockAPI.TShock.Utils.SendLogs(string.Format("{0} executed: /{1}.", player.Name, cmdText), Color.Red);
					cmd.RunWithoutPermissions(cmdText, player, args);
				}
			}

			return true;
		}

	}
}
