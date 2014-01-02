using Meebey.SmartIrc4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackMaineIrcBot.Irc.Commands
{
    class IrcCommandHandler
    {
        public const char commandPrefix = '.';

        static Dictionary<string, BaseIrcCommand> _registry = new Dictionary<string, BaseIrcCommand>();
        public static IEnumerable<KeyValuePair<string, BaseIrcCommand>> Registry { get { return _registry; } }

        public static void Initialize()
        {
            RegisterAllCommands();
            IrcEvents.OnChannelMessage += IrcEvents_OnChannelMessage;
            IrcEvents.OnQuery += IrcEvents_OnQuery;
        }

        static void IrcEvents_OnQuery(QueryEventArgs e)
        {
            BaseIrcCommand commandobject = FindCommand(e.Data);
            if (commandobject != null)
                commandobject.OnQueryMessage(e.Data.Nick, e.Data.Message, e.Data.MessageArray.Skip(1));
        }

        static void IrcEvents_OnChannelMessage(ChannelMessageEventArgs e)
        {
            BaseIrcCommand commandobject= FindCommand(e.Data);
            if(commandobject!=null)
                commandobject.OnChannelCommand(e.Data.Channel, e.Data.Message, e.Data.MessageArray.Skip(1));
        }

        private static BaseIrcCommand FindCommand(IrcMessageData data)
        {
            BaseIrcCommand commandobject = null;
            if (data.MessageArray.Length > 0)
            {
                string command = data.MessageArray[0];
                if (command.Length > 0 && command[0] == commandPrefix)
                {
                    command = command.Substring(1, command.Length - 1);
                    if (_registry.TryGetValue(command, out commandobject))
                        return commandobject;
                }
            }
            return null;
        }

        private static void RegisterAllCommands()
        {
            Console.WriteLine("Registering IRC commands:");
            List<Type> types = ReflectionUtils.FindInheritedTypes(typeof(BaseIrcCommand));
            foreach (var t in types)
            {
                object[] attrs = t.GetCustomAttributes(typeof(IRCCommandAttribute), false);
                if (attrs.Length > 0)
                {
                    RegisterSingleCommand(t, attrs);
                }

            }
        }

        private static void RegisterSingleCommand(Type t, object[] attrs)
        {
            foreach (IRCCommandAttribute attr in attrs)
            {
                string commandstring = attr.CommandString.ToLowerInvariant();
                if (_registry.ContainsKey(commandstring))
                    ConsoleUtils.WriteWarning("WARNING: Irc Command {0} registers to command '{1}' already defined by {2}. Skipping...", t.Name, commandstring, _registry[commandstring].GetType().Name);
                else
                {
                    BaseIrcCommand commandobject = (BaseIrcCommand)Activator.CreateInstance(t);
                    _registry[commandstring] = commandobject;
                    ConsoleUtils.WriteInfo("Irc Command '{0}' registered to {1}", commandstring, t.Name);
                }
            }
        }


    }
}
