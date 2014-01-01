﻿using Meebey.SmartIrc4net;
using System;
using System.Collections;
using System.IO;
using System.Net;

namespace HackMaineIrcBot
{

    public class IrcBot
    {
        public static string IRCServer = "localhost";
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
            irc.OnQueryMessage += OnQueryMessage;
            irc.OnConnecting += irc_OnConnecting;
            irc.OnConnected += irc_OnConnected;

            Timer.DelayCall(() => { Enabled = true; });
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
                                irc.Connect(IRCServer, Port);
                        }
                    }
                }
            }
        }

        static void irc_OnConnected(object sender, EventArgs e)
        {
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

        static void OnQueryMessage(object sender, IrcEventArgs e)
        {
            if (Program.Debug)
                Console.WriteLine("<- Query: {0} | {1} | {2}", e.Data.From, e.Data.Channel ?? "null" , e.Data.Message);
    
            switch (e.Data.MessageArray[0])
            {
                case "join":
                    WriteLine(Rfc2812.Join(e.Data.MessageArray[1]), Priority.Low);
                    break;
                case "part":
                    WriteLine(Rfc2812.Part(e.Data.MessageArray[1]), Priority.Low);
                    break;
                case "say":
                    SendMessage(SendType.Message, e.Data.MessageArray[1], e.Data.MessageArray[2]);
                    break;
            }
        }

        static void Login(string nick, string realname)
        {
            if (Program.Debug)
                Console.WriteLine("-> Login: {0} | {1}", nick, realname);
           
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

        static void SendMessage(SendType type, string destination, string message)
        {
            SendMessage(type, destination, message, Priority.Medium);
        }

        static void SendMessage(SendType type, string destination, string message, Priority priority)
        {
            if (Program.Debug)
                Console.WriteLine("-> Message: {0} | {1} | {2} | {3}", type, destination, message, priority);

            irc.SendMessage(type, destination, message, priority);
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
