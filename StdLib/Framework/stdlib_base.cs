using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolfje.Plugins.Jist.stdlib {
	/// <summary>
	/// Provides the base functionality for exposing javascript library functions to Jist
	/// and Terraria.
	/// 
	/// Derive from this to implement a js library.
	/// </summary>
	public class stdlib_base : IDisposable, IStdLib {
		protected JistEngine engine;

		public string Provides { get; protected set; }

		public stdlib_base(JistEngine engine)
		{
			this.engine = engine;
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {
				
			}

			engine = null;
		}

		#endregion
	}
}
