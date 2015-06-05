using Jint.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using Wolfje.Plugins.Jist.Framework;
using Wolfje.Plugins.Jist.stdlib;

namespace Wolfje.Plugins.Jist.stdlib {
    public class stdhook : stdlib_base {
        protected ArrayList jistHooks;

        public stdhook(JistEngine engine)
            : base(engine)
        {
            this.Provides = "hook";
            this.jistHooks = new ArrayList();
        }

        protected Guid AddHook<T>(JistHook<T> hook) where T : EventArgs
        {
            if (hook == null) {
                return Guid.Empty;
            }

            lock (jistHooks) {
                this.jistHooks.Add(hook);
            }

            return hook.HookID;
        }

        public void UnregisterAllHooks()
        {
            if (jistHooks == null) {
                return;
            }

            lock (jistHooks) {
                foreach (IDisposable item in jistHooks) {
                    item.Dispose();
                }
                jistHooks.Clear();
            }
        }


        [JavascriptFunction("jist_hook_register", "jist_hook")]
        public Guid RegisterHook(string hookName, JsValue func)
        {
            Guid hookId = Guid.Empty;

            switch (hookName.ToLower()) {
                case "on_chat":
                case "chat":
                    hookId = AddHook(new JistHook<ServerChatEventArgs>(this.engine,
                        TerrariaApi.Server.ServerApi.Hooks.ServerChat,
                        func));
                    break;
                case "on_join":
                    hookId = AddHook(new JistHook<JoinEventArgs>(this.engine,
                        TerrariaApi.Server.ServerApi.Hooks.ServerJoin,
                        func));
                    break;
                case "on_leave":
                    hookId = AddHook(new JistHook<LeaveEventArgs>(this.engine,
                        TerrariaApi.Server.ServerApi.Hooks.ServerLeave,
                        func));
                    break;
            }

            return hookId;
        }

        [JavascriptFunction("jist_hook_unregister", "jist_unhook")]
        public void UnregisterHook(Guid guid)
        {
            JistHook<EventArgs> hook;
            if (guid == Guid.Empty) {
                return;
            }

            lock (jistHooks) {
                if ((hook = jistHooks.OfType<JistHook<EventArgs>>().FirstOrDefault(i => i.HookID == guid)) == null) {
                    return;
                }
            }

            hook.Dispose();

            lock (jistHooks) {
                jistHooks.Remove(hook);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing) {
                UnregisterAllHooks();
            }
        }
    }

    public class JistHook<T> : IDisposable where T : EventArgs {
        protected HandlerCollection<T> collection;
        protected HookHandler<T> handler;
        protected TerrariaPlugin pluginInstance;
        protected bool enabled;

        public Guid HookID { get; protected set; }
        public JsValue Function { get; protected set; }

        public JistHook(JistEngine engine, HandlerCollection<T> collection, JsValue func)
        {
            this.HookID = new Guid();
            this.enabled = true;
            this.collection = collection;
            this.pluginInstance = engine.PluginInstance;
            this.handler = (args) => engine.CallFunction(func, this, args);

            collection.Register(engine.PluginInstance, handler);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                collection.Deregister(pluginInstance, this.handler);
            }
        }
    }
}
