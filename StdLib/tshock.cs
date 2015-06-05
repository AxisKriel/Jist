using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wolfje.Plugins.Jist.Framework;
using Wolfje.Plugins.Jist.Extensions;
using TShockAPI.DB;
using Jint.Native;
using Jint.Native.Object;
using TShockAPI;

namespace Wolfje.Plugins.Jist.stdlib {
	/// <summary>
	/// Tshock standard library.
	/// 
	/// Provides scripting API into the various mechanisms of TShock.
	/// </summary>
	public partial class tshock : stdlib_base {
		protected readonly Regex htmlColourRegex = new Regex(@"#([0-9a-f][0-9a-f])([0-9a-f][0-9a-f])([0-9a-f][0-9a-f])", RegexOptions.IgnoreCase);
		protected readonly Regex htmlColourRegexShort = new Regex(@"#([0-9a-f])([0-9a-f])([0-9a-f])", RegexOptions.IgnoreCase);
		protected readonly Regex rgbColourRegex = new Regex(@"((\d*),(\d*),(\d*))", RegexOptions.IgnoreCase);

		public tshock(JistEngine engine) : base(engine)
		{
			this.Provides = "tshock";
		}

		/// <summary>
		/// Gets a TShock region by its name.
		/// </summary>
		[JavascriptFunction("tshock_get_region")]
		public TShockAPI.DB.Region GetRegion(object region)
		{
			TShockAPI.DB.Region reg = null;
            
			if (region == null) {
				return null;
			}

			if (region is TShockAPI.DB.Region) {
				return region as TShockAPI.DB.Region;
			}

			if (region is string) {
				try {
					return TShockAPI.TShock.Regions.GetRegionByName(region as string);
				} catch {
					return null;
				}
			}
		
			return null;
		}

		[JavascriptFunction("tshock_player_regions")]
		public Region[] PlayerInRegions(object PlayerRef)
		{
			TShockAPI.TSPlayer player;
			List<Region> regionList = new List<Region>();

			if ((player = GetPlayer(PlayerRef)) == null) {
				return null;
			}

			foreach (Region region in TShockAPI.TShock.Regions.ListAllRegions(Terraria.Main.worldID.ToString())) {
				if (IsPlayerInRegion(player, region) == false) {
					continue;
				}
				regionList.Add(region);
			}

			return regionList.Count == 0 ? null : regionList.ToArray();
		}

		[JavascriptFunction("tshock_player_in_region")]
		public bool IsPlayerInRegion(object playerRef, object regionRef)
		{
			TShockAPI.TSPlayer player;
			Region region;

			if (playerRef == null
				|| regionRef == null
			    || (player = GetPlayer(playerRef)) == null
			    || (region = GetRegion(regionRef)) == null) {
				return false;
			}

			return region.InArea(player.TileX, player.TileY);
		}

		/// <summary>
		/// JS function: tshock_get_player(player) : TSPlayer
		/// 
		/// Retrieves a TShock player for use in scripts.
		/// </summary>
		[JavascriptFunction("get_player", "tshock_get_player")]
		public TShockAPI.TSPlayer GetPlayer(object PlayerRef)
		{
			TShockAPI.TSPlayer player;
			if (PlayerRef == null) {
				return null;
			}

			if (PlayerRef is TShockAPI.TSPlayer) {
				return PlayerRef as TShockAPI.TSPlayer;
			}

			if (PlayerRef is string) {
				string playerString = PlayerRef as string;

				if (playerString.Equals("server", StringComparison.CurrentCultureIgnoreCase)) {
					return TShockAPI.TSPlayer.Server;
				}

				if ((player = TShockAPI.TShock.Players.FirstOrDefault(i => i != null && i.Name == playerString.ToString())) != null) {
					return player;
				}

				return TShockAPI.TShock.Players.FirstOrDefault(i => i != null && i.UserAccountName == PlayerRef.ToString());
			}

			return null;
		}

		/// <summary>
		/// JS function: tshock_has_permission(player, permission) : boolean
		/// 
		/// Returns if a specified player has a specified permission based
		/// on their current TShock group.
		/// </summary>
		[JavascriptFunction("tshock_has_permission")]
		public bool PlayerHasPermission(object PlayerRef, object GroupName)
		{
			TShockAPI.TSPlayer returnPlayer = null;

			if ((returnPlayer = GetPlayer(PlayerRef)) == null || GroupName == null) {
				return false;
			}

			return returnPlayer.Group.HasPermission(GroupName.ToString());
		}

		/// <summary>
		/// JS function: tshock_group_exists(group) : boolean
		/// 
		/// Returns true if the TShock group exists in the TShock
		/// database and is available for use.
		/// </summary>
		[JavascriptFunction("group_exists", "tshock_group_exists")]
		public bool TShockGroupExists(object GroupName)
		{
			return TShockAPI.TShock.Groups.Count(i => i.Name.Equals(GroupName.ToString(), StringComparison.CurrentCultureIgnoreCase)) > 0;
		}

		/// <summary>
		/// JS function: tshock_group(groupName) : Group
		/// 
		/// Returns a TShock group object by it's name, or
		/// null if the group does not exist or there is a 
		/// database error.
		/// </summary>
		[JavascriptFunction("tshock_group")]
		public TShockAPI.Group TShockGroup(object Group)
		{
			TShockAPI.Group g = null;
			if (Group == null) {
				return null;
			}

			g = TShockAPI.TShock.Groups.FirstOrDefault(i => i.Name.Equals(Group.ToString(), StringComparison.CurrentCultureIgnoreCase));
			return g;
		}

		/// <summary>
		/// Javsacript function: tshock_exec(player, command) : boolean
		/// 
		/// Impersonates a player and executes the specified command
		/// as if it was comming from them, and returns true if the
		/// invoke of the command was successful, false if there was an
		/// error or an exception.
		/// 
		/// There are no permission checks on this method, so the command
		/// will unconditionally be executed, think of this as a super-
		/// admin mechanism.
		/// </summary>
		[JavascriptFunction("execute_command", "tshock_exec")]
		public bool ExecuteCommand(object Player, object Command)
		{
			TShockAPI.TSPlayer p = null;
			string commandToExecute = "";

			if ((p = GetPlayer(Player)) == null) {
				return false;
			}

			try {
				if (Command is List<string>) {
					List<string> cmdList = Command as List<string>;
					foreach (var param in cmdList.Skip(1)) {
						commandToExecute += " " + param;
					}
				} else if (Command is string) {
					commandToExecute = Command.ToString();
				}

				if (string.IsNullOrEmpty((commandToExecute = commandToExecute.Trim())) == true) {
					return false;
				}

				p.PermissionlessInvoke(commandToExecute);

				return true;
			} catch (Exception) {
				ScriptLog.ErrorFormat("tshock_exec", "The command \"{0}\" failed.", commandToExecute.Trim());
				return false;
			}
		}

		/// <summary>
		/// Javsacript function: tshock_exec_silent(player, command) : boolean
		/// 
		/// Impersonates a player and executes the specified command
		/// as if it was comming from them, and returns true if the
		/// invoke of the command was successful, false if there was an
		/// error or an exception.
		/// 
		/// There are no permission checks on this method, so the command
		/// will unconditionally be executed, think of this as a super-
		/// admin mechanism.
		/// </summary>
		[JavascriptFunction("tshock_exec_silent")]
		public bool ExecuteCommandSilent(object Player, object Command)
		{
			TShockAPI.TSPlayer p = null;
			string commandToExecute = "";

			if ((p = GetPlayer(Player)) == null) {
				return false;
			}

			try {
				if (Command is List<string>) {
					List<string> cmdList = Command as List<string>;
					foreach (var param in cmdList.Skip(1)) {
						commandToExecute += " " + param;
					}
				} else if (Command is string) {
					commandToExecute = Command.ToString();
				}

				if (string.IsNullOrEmpty((commandToExecute = commandToExecute.Trim())) == true) {
					return false;
				}

				p.PermissionlessInvoke(commandToExecute, true);

				return true;
			} catch (Exception) {
				ScriptLog.ErrorFormat("tshock_exec_silent", "The command \"{0}\" failed.", commandToExecute.Trim());
				return false;
			}
		}


		/// <summary>
		/// Javascript function: tshock_change_group(player, group) : boolean
		/// 
		/// Changes the provided player's group to the new group specified, and
		/// returns true if the operation succeeded, or false if there was an
		/// error.
		/// </summary>
		[JavascriptFunction("change_group", "tshock_change_group")]
		public bool ChangeGroup(object Player, object Group)
		{
			TShockAPI.TSPlayer p = null;
			TShockAPI.DB.User u = new TShockAPI.DB.User();
			string g = "";

			if ((p = GetPlayer(Player)) == null) {
				return false;
			}

			if (Group is string) {
				g = Group as string;
			} else if (Group is TShockAPI.Group) {
				g = (Group as TShockAPI.Group).Name;
			}

			if (string.IsNullOrEmpty(g) == true) {
				return false;
			}

			try {
				u.Name = p.UserAccountName;
				TShockAPI.TShock.Users.SetUserGroup(u, g);
			} catch (Exception ex) {
				ScriptLog.ErrorFormat("tshock_change_group", "Group change failed: {0}", ex.Message);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Javscript function: tshock_msg(player, msg)
		/// 
		/// Sends a message to a specified player.
		/// </summary>
		[JavascriptFunction("msg", "tshock_msg")]
		public void Message(object PlayerRef, object Message)
		{
			TShockAPI.TSPlayer player;
			string msg = null;
			if ((player = GetPlayer(PlayerRef)) == null) {
				return;
			}

			msg = Message.ToString();
			if (string.IsNullOrEmpty(msg) == true) {
				return;
			}

			player.SendInfoMessage("{0}", Message);
		}

		/// <summary>
		/// Javascript function: tshock_msg_colour(colour, player, msg)
		/// 
		/// Sends a message to a specified player with the specified colour.
		/// 
		/// Colours may be in R,G,B or #html format.
		/// </summary>
		[JavascriptFunction("msg_colour", "tshock_msg_colour")]
		public void MessageWithColour(object Colour, object Player, object Message)
		{
			TShockAPI.TSPlayer ply = null;
			string msg = Message.ToString();
			Color c = ParseColour(Colour);

			if ((ply = GetPlayer(Player)) == null
			    || string.IsNullOrEmpty(msg) == true) {
				return;
			}

			ply.SendMessageFormat(c, "{0}", Message);
		}

		/// <summary>
		/// Javascript function: tshock_broadcast_colour(colour, msg)
		/// 
		/// Broadcasts a message to all players in the server with the specified
		/// colour.
		/// </summary>
		[JavascriptFunction("broadcast_colour", "tshock_broadcast_colour")]
		public void BroadcastWithColour(object Colour, object Message)
		{
			Color c = ParseColour(Colour);

			if (Message != null) {
				TShockAPI.TShock.Utils.Broadcast(Message.ToString(), c);
			}
		}

		/// <summary>
		/// Javascript function: tshock_broadcast(colour, msg)
		/// 
		/// Broadcasts a message top all players in the server.
		/// </summary>
		[JavascriptFunction("broadcast", "tshock_broadcast")]
		public void Broadcast(object Message)
		{
			BroadcastWithColour("#f00", Message);
		}

		/// <summary>
		/// javascript function: tshock_server()
		/// 
		/// Returns an instance of the tshock server
		/// player object.
		/// </summary>
		[JavascriptFunction("tshock_server")]
		public TShockAPI.TSPlayer ServerPlayer()
		{
			return TShockAPI.TSPlayer.Server;
		}

		[JavascriptFunction("tshock_create_npc")]
		public KeyValuePair<int, Terraria.NPC> CreateNPC(int x, int y, int type)
		{
			int index = Terraria.NPC.NewNPC(x, y, type);
			Terraria.NPC npc;

			if ((npc = Terraria.Main.npc.ElementAtOrDefault(index)) == null) {
				return new KeyValuePair<int, Terraria.NPC>(-1, null);
			}

			//Terraria.Main.npc[index].SetDefaults(npc.type, -1f);
			Terraria.Main.npc[index].SetDefaults(npc.name);
			//Terraria.Main.npcLifeBytes[index] = 4;

			return new KeyValuePair<int, Terraria.NPC>(index, npc);
		}

		[JavascriptFunction("tshock_clear_tile_in_range")]
		public Point ClearTileInRange(int x, int y, int rx, int ry)
		{
			Point p = new Point();

			TShock.Utils.GetRandomClearTileWithInRange(x, y, rx, ry, out p.X, out p.Y);

			return p;
		}

		[JavascriptFunction("tshock_set_team")]
		public void SetTeam(object player, int team)
		{
			TSPlayer p = GetPlayer(player);
			p.SetTeam(team);
		}

		[JavascriptFunction("tshock_warp_find")]
		public Warp FindWarp(string warp)
		{
			return TShock.Warps.Find(warp);
		}

		[JavascriptFunction("tshock_teleport_player")]
		public void WarpPlayer(object player, float x, float y)
		{
			TSPlayer p = GetPlayer(player);

			p.Teleport(x, y);
		}

		[JavascriptFunction("tshock_warp_player")]
		public void WarpPlayer(object player, Warp warp)
		{
			TSPlayer p = GetPlayer(player);
			p.Teleport(warp.Position.X * 16, warp.Position.Y * 16);
		}


		/// <summary>
		/// Parses colour from a range of input types and returns a strongly-
		/// typed Color structure if the conversion succeeded, false otherwise.
		/// </summary>
		protected Color ParseColour(object colour)
		{
			Color returnColor = Color.Yellow;

			if (colour != null) {
				if (colour is Color) {
					returnColor = (Color)colour;
				} else if (colour is string) {
					int r = 0, g = 0, b = 0;
					string colourString = colour as string;

					if (rgbColourRegex.IsMatch(colourString)) {
						Match rgbMatch = rgbColourRegex.Match(colourString);

						Int32.TryParse(rgbMatch.Groups[2].Value, out r);
						Int32.TryParse(rgbMatch.Groups[3].Value, out g);
						Int32.TryParse(rgbMatch.Groups[4].Value, out b);

					} else if (htmlColourRegex.IsMatch(colourString)) {
						Match htmlMatch = htmlColourRegex.Match(colourString);

						r = Convert.ToInt32(htmlMatch.Groups[1].Value, 16);
						g = Convert.ToInt32(htmlMatch.Groups[2].Value, 16);
						b = Convert.ToInt32(htmlMatch.Groups[3].Value, 16);
					} else if (htmlColourRegexShort.IsMatch(colourString)) {
						Match htmlMatch = htmlColourRegexShort.Match(colourString);

						r = Convert.ToInt32(htmlMatch.Groups[1].Value + htmlMatch.Groups[1].Value, 16);
						g = Convert.ToInt32(htmlMatch.Groups[2].Value + htmlMatch.Groups[2].Value, 16);
						b = Convert.ToInt32(htmlMatch.Groups[3].Value + htmlMatch.Groups[3].Value, 16);
					}

					returnColor = new Color(r, g, b);
				}
			}

			return returnColor;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing) {
				
			}
		}
	}
}
