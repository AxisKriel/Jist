using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolfje.Plugins.Jist.Framework;
using Jint;
using Jint.Native;
using System.Linq.Expressions;
using System.IO;
using TerrariaApi.Server;
using Jint.Runtime.Descriptors;
using Jint.Runtime;
using Jint.Native.Json;

namespace Wolfje.Plugins.Jist {
	/// <summary>
	/// Jist Engine, provides the Javascript engine to TerrariaServer
	/// using the bundled Jint interpreter.
	/// </summary>
	public class JistEngine : IDisposable {
		protected JistPlugin plugin;
		protected Jint.Engine jsEngine;
		protected List<string> providedPackages;
		protected ScriptContainer scriptContainer;
        protected int totalLoadingItems = 0;
        protected int doneItems = 0;
        protected int oldPercent = 0;

        internal event EventHandler<PercentChangedEventArgs> PercentChanged;

		protected static string scriptsDir = Path.Combine(Environment.CurrentDirectory, "serverscripts");

		/*
		 * Standard library references.
		 * 
		 * These hold all the javascript functions that jist
		 * provides in its base packages.
		 */
		public stdlib.std stdLib;
        public stdlib.tshock stdTshock;
        public stdlib.stdtask stdTask;
        public stdlib.stdhook stdHook;

		public JistEngine(JistPlugin parent)
		{
			this.providedPackages = new List<string>();
			this.plugin = parent;
			this.scriptContainer = new ScriptContainer(this);
            ServerApi.Hooks.GamePostInitialize.Register(plugin, Game_PostInitialize);
            PercentChanged += (sender, args) => ConsoleEx.WriteBar(args);
		}

        public JistPlugin PluginInstance { get { return plugin; } }

        /// <summary>
        /// Occurs when TerrariaServer has loaded the map.
        /// </summary>
        protected async void Game_PostInitialize(EventArgs args)
        {
            await LoadEngineAsync();
        }

        protected void RaisePercentChangedEvent(string label)
        {
            PercentChangedEventArgs args = new PercentChangedEventArgs();
            double percentComplete = (double)++doneItems / totalLoadingItems * 100;

            if (oldPercent != (int)percentComplete) {
                args.Percent = (int)percentComplete;
                args.Label = label;

                if (PercentChanged != null) {
                    PercentChanged(this, args);
                }
                oldPercent = (int)percentComplete;
            }
        }

		public async Task LoadEngineAsync()
		{
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" * Jist is loading");
            Console.ResetColor();

			if (Directory.Exists(scriptsDir) == false) {
				try {
					Directory.CreateDirectory(scriptsDir);
				} catch {
					TShockAPI.Log.ConsoleError("jist load: Could not create serverscripts directory");
					return;
				}
			}

			totalLoadingItems = ScriptsCount() * 2 + 5;
            this.jsEngine = new Engine(o => o.AllowClr());
            RaisePercentChangedEvent("Engine");
            
            /*
             * Load the standard library collection.
             */
            await Task.Run(() => LoadLibraries());
            RaisePercentChangedEvent("Libraries");

            /*
             * Enumerate the libraries and ask them to submit
             * their functions to the javascript runtime. All
             * functions should be available before any scripts
             * load.
             */
            await Task.Run(() => CreateScriptFunctions());
            RaisePercentChangedEvent("Functions");

            /*
             * Load all scripts from disk, and preprocess them.
             * Result should be a reference-counted list of sc-
             * ripts that need to be executed in order.
             */
            await Task.Run(() => LoadScripts());
            RaisePercentChangedEvent("Scripts");

            /*
             * Engine executes all scripts only once.  The are
             * responsible for setting themselves up in the JS
             * global environment when this happens, subscrib-
             * ing to hooks, enlisting aliases, etc.
             */
            await Task.Run(() => ExecuteScripts());
            RaisePercentChangedEvent("Execute");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" * Loaded {0} scripts", ScriptsCount());
            Console.ResetColor();
            Console.WriteLine();
		}
		
		/// <summary>
		/// Loads the standard library collection inside Jist, and binds all
		/// the functions in it to the running javascript engine.
		/// </summary>
		protected void LoadLibraries()
		{
			LoadLibrary((stdLib = new stdlib.std(this)));
			LoadLibrary((stdTshock = new stdlib.tshock(this)));
            LoadLibrary((stdTask = new stdlib.stdtask(this)));
            LoadLibrary((stdHook = new stdlib.stdhook(this)));
		}

		/// <summary>
		/// Causes the instance of stdlib_base to submit all
		/// its functions to the JS function cache.
		/// </summary>
		public void LoadLibrary(stdlib.stdlib_base lib)
		{
			if (lib == null) {
				return;
			}
            CreateScriptFunctions(lib.GetType(), lib);
		}

        protected int ScriptsCount()
        {
            try {
				return Directory.EnumerateFiles(scriptsDir, "*.js").Count();
            } catch {
                return 0;
            }
        }

		/// <summary>
		/// Loads all scripts from the serverscripts directory, and
		/// inserts them into the script container.
		/// </summary>
		protected void LoadScripts()
		{
			foreach (var file in Directory.EnumerateFiles(scriptsDir, "*.js")) {
				LoadScript(Path.GetFileName(file));
                RaisePercentChangedEvent("Scripts");
			}
		}

		/// <summary>
		/// Loads a script from file into a reference-counted object, and inserts it into
		/// the script container.
		/// 
		/// Scripts are /not/ executed by the javascript engine at this point.
		/// </summary>
		public JistScript LoadScript(string ScriptPath, bool IncreaseRefCount = true)
		{
			JistScript content;
			/*
			 * if this script has already been called for, return the called object 
			 * with an incremented ref count
			 */
			if (scriptContainer.Scripts.Count(i => 
				i.FilePathOrUri.Equals(ScriptPath, StringComparison.InvariantCultureIgnoreCase)) > 0) {
				content = scriptContainer.Scripts.FirstOrDefault(i => 
					i.FilePathOrUri.Equals(ScriptPath, StringComparison.InvariantCultureIgnoreCase));
				if (IncreaseRefCount) {
					content.ReferenceCount++;
				}
				return null;
			}

			content = new JistScript();
			content.FilePathOrUri = ScriptPath;
			content.ReferenceCount = 1;

			try {
				content.Script = File.ReadAllText(Path.Combine(scriptsDir, content.FilePathOrUri));
			} catch (Exception ex) {
				ScriptLog.ErrorFormat("engine", "Cannot load {0}: {1}", ScriptPath, ex.Message);
				return null;
			}

			/*
			 * Script must be added to the content list before preprocessing
			 * this is to prevent cyclic references between @imports, used as an include guard
			 */
			scriptContainer.PreprocessScript(content);
			scriptContainer.Scripts.Add(content);

			return content;
		}

        /// <summary>
        /// Executes a snippet of javascript in the
        /// running jist instance and returns any 
        /// result in JSON format.
        /// </summary>
        public string Eval(string snippet)
        {
            JsValue returnValue = default(JsValue);

            if (jsEngine == null || string.IsNullOrEmpty(snippet) == true) {
                return "undefined";
            }
            try {
                returnValue = jsEngine.GetValue(jsEngine.Execute(snippet).GetCompletionValue());
                if (returnValue.Type == Types.None
                    || returnValue.Type == Types.Null
                    || returnValue.Type == Types.Undefined) {
                    return "undefined";
                }
            } catch (Exception ex) {
                ScriptLog.ErrorFormat("eval", "Error executing snippet:" + ex.Message);
                return "undefined";
            }

            return TypeConverter.ToString(jsEngine.Json.Stringify(jsEngine.Json, 
                Arguments.From(returnValue, Undefined.Instance, "  ")));
        }

		/// <summary>
		/// Executes a script, and returns it's completion value.
		/// </summary>
		public JsValue ExecuteScript(JistScript script)
		{
			if (script == null || string.IsNullOrEmpty(script.Script) == true) {
				return JsValue.Undefined;
			}

			try {
				return jsEngine.Execute(script.Script).GetCompletionValue();
			} catch (Exception ex) {
				ScriptLog.ErrorFormat(script.FilePathOrUri, "Execution error: " + ex.Message);
				return JsValue.Undefined;
			}
 		}

		/// <summary>
		/// Enumerates all scripts in the the container, and execute the
		/// scripts that have the highest reference count first.
		/// </summary>
		protected void ExecuteScripts()
		{
			foreach (JistScript script in scriptContainer.Scripts.OrderByDescending(i => i.ReferenceCount)) {
				try {
					ExecuteScript(script);
                    RaisePercentChangedEvent("Execute");
				} catch (Exception ex) {
					ScriptLog.ErrorFormat(script.FilePathOrUri, "Execution error: " + ex.Message);
				}
			}
		}

        public IDictionary<string, PropertyDescriptor> DumpGlobalEnvironment()
        {
            return jsEngine.Global.Properties;
        }

		/// <summary>
		/// Creates Javascript function delegates for the type specified, and 
		/// optionally by the object instance.
		/// 
		/// Instance may be null, however only static methods will be created.
		/// </summary>
		public async Task CreateScriptFunctionsAsync(Type type, object instance)
		{
			await Task.Run(() => CreateScriptFunctions(type, instance));
		}

		/// <summary>
		/// Creates Javascript function delegates for the type specified, and
		/// optionally by the object instance.
		/// 
		/// Instance may be null, however only static methods will be regarded
		/// in this manner, since there is no 'this' pointer.
		/// </summary>
		public void CreateScriptFunctions(Type type, object instance)
		{
			Delegate functionDelegate = null;
			Type delegateSignature = null;
			string functionName = null;
			JavascriptFunctionAttribute jsAttribute = null;

			/*
			 * If the class provides functionality for scripts, 
			 * add it into the providedpackages array.
			 */
			foreach (JavascriptProvidesAttribute attrib in type.GetCustomAttributes(true).OfType<JavascriptProvidesAttribute>()) {
				if (!providedPackages.Contains(attrib.PackageName)) {
					providedPackages.Add(attrib.PackageName);
				}
			}

			/*
			 * look for JS methods in the type
			 */
			foreach (var jsFunction in type.GetMethods().Where(i => i.GetCustomAttributes(true).OfType<JavascriptFunctionAttribute>().Any())) {
				if (instance == null && jsFunction.IsStatic == false
                    || (jsAttribute = jsFunction.GetCustomAttributes(true).OfType<JavascriptFunctionAttribute>().FirstOrDefault()) == null) {
					continue;
				}

				foreach (string func in jsAttribute.FunctionNames) {
					functionName = func ?? jsFunction.Name;
					
					try {
						/*
						 * A delegate signature type matching every single parameter type, 
						 * and the return type must be appended as the very last item
						 * in the array.
						 */
						delegateSignature = Expression.GetDelegateType(jsFunction.GetParameters().Select(i => i.ParameterType)
							.Concat(new[] { jsFunction.ReturnType }).ToArray());

						if (instance != null) {
							functionDelegate = Delegate.CreateDelegate(delegateSignature, instance, jsFunction);
						} else {
							functionDelegate = Delegate.CreateDelegate(delegateSignature, jsFunction);
						}

						jsEngine.SetValue(functionName, functionDelegate);
					} catch (Exception ex) {
						ScriptLog.ErrorFormat("engine", "Error whilst creating javascript function for {0}: {1}",
							functionName, ex.ToString());
						continue;
					}
				}
			}
		}

		/// <summary>
		/// Loops through all types in the app domain and creates functions in 
		/// the engine for methods that have a JavascriptFunction attribute.
		/// </summary>
		protected void CreateScriptFunctions()
		{
            JistPlugin.RequestExternalFunctions();
            jsEngine.SetValue("alert", new Action<object>(Console.WriteLine));
		}

		/// <summary>
		/// Calls a javascript function encapsulated by JsValue
		/// and returns it's result.
		/// </summary>
		public JsValue CallFunction(JsValue function, object thisObject, params object[] args) {
			object t = thisObject ?? (object)this;
			try {
				return function.Invoke(JsValue.FromObject(jsEngine, t), args.ToJsValueArray(jsEngine));
			} catch (Exception ex) {
				ScriptLog.ErrorFormat("engine", "Error executing {0}: {1}",
					function.ToString(), ex.ToString());
			}

			return JsValue.Undefined;
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
            GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {
				this.stdLib.Dispose();
				this.stdTshock.Dispose();
                this.stdTask.Dispose();
                this.stdHook.Dispose();
                ServerApi.Hooks.GamePostInitialize.Deregister(plugin, Game_PostInitialize);
			}
		}

		#endregion
	}
}
