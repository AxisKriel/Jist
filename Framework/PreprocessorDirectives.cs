using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

namespace Wolfje.Plugins.Jist.Framework {
    static class PreprocessorDirectives {
        public static readonly Regex multilineCommentRegex = new Regex(@"/\*[^*]*\*+(?:[^*/][^*]*\*+)*/");
        public static readonly Regex singleLineCommentRegex = new Regex(@"//.*?\r?\n");
        public static readonly Regex inlineRegex = new Regex("@inline \"(.*?)\";");
        public static readonly Regex importRegex = new Regex("@import \"(.*?)\";");
        public static readonly Regex requiresRegex = new Regex("@require(s?) (.*?);");
    }
}
