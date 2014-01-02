using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackMaineIrcBot.Irc.Hooks
{
    abstract class BaseIrcHook
    {
        protected abstract bool Handle(ChannelMessageEventArgs e);

        private static string _name;
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    object[] attrs = GetType().GetCustomAttributes(typeof(IRCHookAttribute), false);
                    if (attrs.Length > 0)
                        _name = ((IRCHookAttribute)attrs[0]).Name;
                    if (_name == null)
                        _name = GetType().Name;
                }
                return _name;
            }
        }

        private static string _description;
        public string Description
        {
            get
            {
                if (_description == null)
                {
                    object[] attrs = GetType().GetCustomAttributes(typeof(IRCHookAttribute), false);
                    if (attrs.Length > 0)
                        _description = ((IRCHookAttribute)attrs[0]).Description;
                    if (_name == null)
                        _description = string.Empty;
                }
                return _name;
            }
        }

        static List<BaseIrcHook> _registry = new List<BaseIrcHook>();
        public static IEnumerable<BaseIrcHook> Registry { get { return _registry; } }

        public static void Initialize()
        {
            RegisterAllHooks();
            IrcEvents.OnChannelMessage += IrcEvents_OnChannelMessage;
        }

        private static void IrcEvents_OnChannelMessage(ChannelMessageEventArgs e)
        {
            foreach (var hook in Registry)
                if (hook.Handle(e))
                    break;
        }

        private static void RegisterAllHooks()
        {
            Console.WriteLine("Registering IRC Hooks:");
            List<Type> types = ReflectionUtils.FindInheritedTypes(typeof(BaseIrcHook));
            types.Sort(new TypePriorityComparer());
            foreach (var t in types)
            {
                object[] attrs = t.GetCustomAttributes(typeof(IRCHookAttribute), false);
                if (attrs.Length > 0)
                {
                    BaseIrcHook hookobject = (BaseIrcHook)Activator.CreateInstance(t);
                    _registry.Add(hookobject);
                }
            }
        }
    }

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
