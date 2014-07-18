using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using System.Reflection;

namespace Wolfje.Plugins.Jist {
	[ApiVersion(1,16)]
	public class JistPlugin : TerrariaPlugin {
		public static JistEngine Instance { get; protected set; }
		public JistPlugin(Terraria.Main game)
			: base(game)
		{
			Order = Int32.MaxValue;
			Instance = new JistEngine(this);
		}

		public override string Author {
			get {
				return "Wolfje";
			}
		}

		public override string Description {
			get {
				return "Javascript interpreted scripting for TShock";
			}
		}

		public override string Name {
			get {
				return "Jist";
			}
		}

		public override Version Version {
			get {
				return Assembly.GetExecutingAssembly().GetName().Version;
			}
		}

		/// <summary>
		/// Entry point of TerrariaServerAPI
		/// </summary>
		public override void Initialize()
		{
			Instance.LoadEngine();
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
