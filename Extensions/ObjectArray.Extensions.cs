using System;
using Jint.Native;

namespace Wolfje.Plugins.Jist {
	public static class ObjectArrayExtensions {
		public static JsValue[] ToJsValueArray(this object[] args, Jint.Engine jsEngine) {
			JsValue[] jsValues = new JsValue[args.Length];

			for (int i = 0; i < args.Length; i++) {
				jsValues[i] = JsValue.FromObject(jsEngine, args[i]);
			}

			return jsValues;
		}
	}
}

