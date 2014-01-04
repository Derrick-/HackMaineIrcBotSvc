using Meebey.SmartIrc4net;
using System;
using System.Collections;
using System.IO;
using System.Net;

namespace HackMaineIrcBot.Irc
{

    public class IrcBot
    {
        public static string IRCServer = "irc.freenode.net";
        public static int Port = 6667;
        public static int SSLPort = 7000;
        public static string MainChannel = Program.TestMode ? "#mainehackerclub_test" : "#mainehackerclub";
        public static string Nick = Program.TestMode ? "MaineHackerBotTest" : "MaineHackerBot";
        public static string RealName = "Maine Hacker Club Bot";
        public static string AuthUser = Program.TestMode ? "" : "MaineHackerBot";
        public static string AuthPass = "sd734lsg%das!oufgew";
        public static string AuthServ = "NickServ@services.";
        public static string ChanServ = "ChanServ@services.";
        public static bool UseSSL = true;
        public static bool AutoInv = true;

        public static IPEndPoint FixedEndpoint { get; set; }

        private static IrcClient irc = new IrcClient();

        public static void Initialize()
        {
            FixedEndpoint = ReadAddressConfig();

            irc.SendDelay = 200;
            irc.AutoRetry = true;
            irc.ActiveChannelSyncing = true;
            irc.OnQueryMessage += irc_OnQueryMessage;
            irc.OnChannelMessage += irc_OnChannelMessage;
            irc.OnConnecting += irc_OnConnecting;
            irc.OnConnected += irc_OnConnected;
            irc.OnJoin += irc_OnJoin;
            irc.OnInvite += irc_OnInvite;
            irc.OnDisconnected += irc_OnDisconnected;
            irc.OnModeChange += irc_OnModeChange;
            Timer.DelayCall(() => { Enabled = true; });
        }

        static void irc_OnModeChange(object sender, IrcEventArgs e)
        {
            Console.WriteLine("Mode change: " + e.Data.Message);
        }

        static void irc_OnDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected.");
        }

        static void irc_OnInvite(object sender, InviteEventArgs e)
        {
            Console.WriteLine("Invited to " + e.Channel + " by " + e.Who);
        }

        static void irc_OnJoin(object sender, JoinEventArgs e)
        {
            Console.WriteLine("Joined " + e.Channel);
        }

        static void irc_OnConnecting(object sender, EventArgs e)
        {
           Console.WriteLine("Connecting...");
        }

        private static bool m_Enabled;
        public static bool Enabled
        {
            get
            {
                return m_Enabled;
            }
            set
            {
                {
                    Console.WriteLine("IRC: {0}abling...", value ? "En" : "Dis");
                    if (irc == null && value)
                        irc = new IrcClient();
                    if (irc != null)
                    {
                        irc.AutoRetry = irc.AutoReconnect = irc.AutoJoinOnInvite = irc.AutoRejoin = value;
                        if (value != m_Enabled)
                        {
                            m_Enabled = value;
                            if (!value && irc.IsConnected)
                                irc.Disconnect();
                            else if (!irc.IsConnected)
                                DoConnect();
                        }
                    }
                }
            }
        }

        private static void DoConnect()
        {
            irc.UseSsl = UseSSL;
            int port=UseSSL ? SSLPort : Port;
            Console.WriteLine("Connecting to {0} on port {1}{2}", IRCServer, port, UseSSL ? " using SSL" : "");
            irc.Connect(IRCServer, port);
        }

        static void irc_OnConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected.");
            Login(Nick, RealName);
            WriteLine(Rfc2812.Join(MainChannel), Priority.Low);
          
            DoListen();
        }

        static void DoListen()
        {
            if (Enabled && irc.IsConnected && !Program.Closing)
            {
                irc.Listen(false);
                Timer.DelayCall(DoListen);
            }
            else
                Enabled = false;
        }

        static void irc_OnChannelMessage(object sender, IrcEventArgs e)
        {
            if (Program.Debug)
                Console.WriteLine("<- Channel: {0} | {1} | {2}", e.Data.From, e.Data.Channel ?? "null", e.Data.Message);

            IrcEvents.InvokeOnChannelMessage(new ChannelMessageEventArgs(e.Data));
        }

        static void irc_OnQueryMessage(object sender, IrcEventArgs e)
        {
            if (Program.Debug)
                Console.WriteLine("<- Query: {0} | {1} | {2}", e.Data.From, e.Data.Channel ?? "null" , e.Data.Message);

            IrcEvents.InvokeOnQuery(new QueryEventArgs(e.Data));
        }

        static void Login(string nick, string realname)
        {
            if (Program.Debug)
                Console.WriteLine("-> Login: {0} | {1}", nick, realname);
            else
                Console.WriteLine("-> Login: {0}", nick);
           
            irc.Login(nick, realname);
        }

        void WriteLine(string data)
        {
            WriteLine(data, Priority.Medium);
        }

        static void WriteLine(string data, Priority priority)
        {
            if (Program.Debug)
                Console.WriteLine("-> Write: {0} | {1}", data, priority);

            irc.WriteLine(data, priority);
        }

        public static void SendMessage(SendType type, string destination, string message)
        {
            SendMessage(type, destination, message, Priority.Medium);
        }

        public static void SendMessage(SendType type, string destination, string message, Priority priority)
        {
            if (Program.Debug)
                Console.WriteLine("-> Message: {0} | {1} | {2} | {3}", type, destination, message, priority);

            try
            {
                irc.SendMessage(type, destination, message, priority);
            }
            catch (NotConnectedException)
            {
                ConsoleUtils.WriteWarning("IRC Message not sent: Not Connected!");
            }
        }

        static IPEndPoint ReadAddressConfig()
        {
            try
            {
                IPAddress ip;
                string line;
                using (StreamReader sr = new StreamReader("ipaddr.cfg"))
                    line = sr.ReadLine();
                if (!IPAddress.TryParse(line, out ip))
                {
                    return null;
                }
                else
                {
                    Console.WriteLine("IP Address read from ipaddr.cfg: {0}", line);
                    IPEndPoint ipe = new IPEndPoint(ip, 0);
                    return ipe;
                }
            }
            catch
            {
                Console.WriteLine("ipaddr.cfg not found.");
                return null;
            }
        }
    }
}

