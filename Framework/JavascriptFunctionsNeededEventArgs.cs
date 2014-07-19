using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolfje.Plugins.Jist.Framework {
    public class JavascriptFunctionsNeededEventArgs {
        public JistEngine Engine { get; protected set; }

        public JavascriptFunctionsNeededEventArgs(JistEngine engine)
        {
            this.Engine = engine;
        }
    }
}
