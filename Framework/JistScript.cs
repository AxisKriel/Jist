using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.Jist.Framework {
    public class JistScript {

        /// <summary>
        /// The location this script resides in.
        /// </summary>
        public string FilePathOrUri { get; set; }
        
        /// <summary>
        /// Gets or sets how many @import references there are for this script
        /// </summary>
        public int ReferenceCount { get; set; }

        /// <summary>
        /// Gets or sets the body of the script to be executed.
        /// </summary>
        public string Script { get; set; }

        public List<string> PackageRequirements { get; set; }

		public JistScript()
		{
            this.PackageRequirements = new List<string>();
        }
    }
}
