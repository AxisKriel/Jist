using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wolfje.Plugins.Jist.Framework {
	public class ScriptContainer {
		protected JistEngine jistParent;
		protected readonly MatchEvaluator blankEvaluator = new MatchEvaluator(str => "");
		public List<JistScript> Scripts { get; set; }

		public ScriptContainer(JistEngine parent)
		{
			this.jistParent = parent;
			this.Scripts = new List<JistScript>();
		}

		public void PreprocessScript(JistScript script)
		{
			if (script == null || string.IsNullOrEmpty(script.Script) == true) {
				return;
			}

			PreprocessComments(ref script);
			PreprocessRequires(ref script);
			PreprocessImports(ref script);
			PreprocessInlines(ref script);
		}

		/// <summary>
		/// Removes comments from a scripts content.
		/// </summary>
		protected void PreprocessComments(ref JistScript script)
		{
			if (script == null || string.IsNullOrEmpty(script.Script) == true) {
				return;
			}

			PreprocessorDirectives.multilineCommentRegex.Replace(script.Script, blankEvaluator);
			PreprocessorDirectives.singleLineCommentRegex.Replace(script.Script, blankEvaluator);

		}

		/// <summary>
		/// Processes @require directives.
		/// 
		/// @require directives indicate that a script uses functionality provided by a specified.net
		/// package in Jist marked with the JavascriptProvides attributre.  If Jist is run without that 
		/// package support, the script will not run.
		/// </summary>
		protected void PreprocessRequires(ref JistScript script)
		{
			if (script == null || string.IsNullOrEmpty(script.Script) == true
			|| PreprocessorDirectives.requiresRegex.IsMatch(script.Script) == false) {
				return;
			}

			foreach (Match match in PreprocessorDirectives.requiresRegex.Matches(script.Script)) {
				string[] packages = match.Groups[2].Value.Split(',');

				foreach (string package in packages) {
					string trimmedPackage = package.Trim().Replace("\"", "");
					if (string.IsNullOrEmpty(trimmedPackage) == true) {
						return;
					}

					script.PackageRequirements.Add(trimmedPackage);
					script.Script = script.Script.Replace(match.Value, string.Format("/** #pragma require \"{0}\" - DO NOT CHANGE THIS LINE **/\r\n", trimmedPackage));
				}
			}
		}

		protected void PreprocessImports(ref JistScript script)
		{
			//@import: tells the preprocessor to import the script into the engine, and load it.
			if (PreprocessorDirectives.importRegex.IsMatch(script.Script)) {
				foreach (Match match in PreprocessorDirectives.importRegex.Matches(script.Script)) {
					string scriptLocation = match.Groups[1].Value;

					//prevent cyclic references
					if (!scriptLocation.Equals(script.FilePathOrUri)) {
						jistParent.LoadScript(scriptLocation);

						string importedValue = string.Format("/** #pragma import \"{0}\" - Imported by engine - DO NOT CHANGE THIS LINE **/", scriptLocation);

						script.Script = script.Script.Replace(match.Value, importedValue);
					}
				}
			}
		}

		protected void PreprocessInlines(ref JistScript script)
		{
			//@inline: inline imports script content into another script.
			if (PreprocessorDirectives.inlineRegex.IsMatch(script.Script)) {
				foreach (Match match in PreprocessorDirectives.inlineRegex.Matches(script.Script)) {
					string scriptLocation = match.Groups[1].Value;

					//prevent cyclic references
					if (!scriptLocation.Equals(script.FilePathOrUri)) {
						JistScript inlineContent = jistParent.LoadScript(scriptLocation, false);
						script.Script = script.Script.Replace(match.Value, "");
						script.Script = script.Script.Insert(match.Index, string.Format("/** #pragma inline \"{0}\" **/\r\n{1}", scriptLocation, inlineContent.Script));
					}
				}
			}
		}
	}
}
