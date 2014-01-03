using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackMaineIrcBot.Irc.Hooks
{
    abstract class BaseIrcHook
    {
        protected abstract Task<bool> Handle(BaseIrcResponder responder, ChannelMessageEventArgs e);

        private IRCHookAttribute _attr;
        
        public string Name { get { return _attr.Name; } }
        public string Description { get { return _attr.Description; } }

        static List<BaseIrcHook> _registry = new List<BaseIrcHook>();
        public static IEnumerable<BaseIrcHook> Registry { get { return _registry; } }

        public static void Initialize()
        {
            RegisterAllHooks();
            IrcEvents.OnChannelMessage += IrcEvents_OnChannelMessage;
        }

        private static void IrcEvents_OnChannelMessage(ChannelMessageEventArgs e)
        {
            Task.Run(async () =>
            {
                foreach (var hook in Registry)
                    if (await hook.Handle(new ChannelResponder(e.Data.Channel), e))
                        break;
            });
        }

        private static void RegisterAllHooks()
        {
            Console.WriteLine("Registering IRC Hooks:");
            ReflectionUtils.RegisterObjects<BaseIrcHook, IRCHookAttribute>(RegisterSingleHook);
        }

        private static void RegisterSingleHook(Type t, object[] attrs)
        {
            BaseIrcHook hookobject = (BaseIrcHook)Activator.CreateInstance(t);
            hookobject._attr = ((IRCHookAttribute)attrs[0]);
            ConsoleUtils.WriteInfo("\t{0}: {1}", hookobject.Name, hookobject.Description);
            _registry.Add(hookobject);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    class IRCHookAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IRCHookAttribute(string Name = null, string Description = null)
        {
            this.Name = Name;
            this.Description = Description;
        }
    }
}
