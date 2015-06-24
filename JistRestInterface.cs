using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolfje.Plugins.Jist {
	public class JistRestInterface : IDisposable {
		protected JistPlugin _plugin;

		public JistRestInterface(JistPlugin plugin)
		{
			this._plugin = plugin;

			TShockAPI.TShock.RestApi.Register(new Rests.SecureRestCommand("/_jist/v1/send", rest_send, "jist.rest.send"));
		}

		private object rest_send(Rests.RestRequestArgs args)
		{
			string request;

			if (string.IsNullOrEmpty((request = args.Parameters["p"])) == true) {
				return new Rests.RestObject("500") {
					{ "response", "Parameter is null" }
				};
			}


			return new Rests.RestObject() {
				{ "response", JistPlugin.Instance.Eval(request) }
			};
		}


		~JistRestInterface()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {

			}
		}
	}
}
