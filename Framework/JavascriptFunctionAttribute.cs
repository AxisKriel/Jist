using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.Jist.Framework {

    /// <summary>
    /// Indicates that this method should be exposed to the JS interpreter as a function
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class JavascriptFunctionAttribute : Attribute {
        string[] _functionNames;
        
        /// <summary>
        /// Returns the function name given in the attribute.  If null, the function takes the name of the method in which it was placed.
        /// </summary>
        public string[] FunctionNames {
            get {
                return _functionNames;
            }
        }

        /// <summary>
        /// Indicates that this method should be exposed to the JS intwaiterpreter as a function, with a custom function name
        /// </summary>
        /// <param name="FunctionName">The name of the Javascript function to create</param>
        public JavascriptFunctionAttribute(params string[] names) {
            _functionNames = names;
        }
    }
}
