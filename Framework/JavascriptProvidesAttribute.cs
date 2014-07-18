using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.Jist.Framework {

    /// <summary>
    /// Indicates that this class provides functions for scripts that @require something
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class JavascriptProvidesAttribute : Attribute {
        public string PackageName { get; set; }

        public JavascriptProvidesAttribute(string PackageName) {
            this.PackageName = PackageName;
        }

    }
}
