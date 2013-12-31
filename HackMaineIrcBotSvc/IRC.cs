using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HackMaineIrcBot
{
    class Irc
    {
        public static string IRCServer = "irc.freenode.net";
        public static int Port = 6667;
        public static int SSLPort = 7000;
        public static string MainChannel = Program.TestMode ? "#mainehackerclub_test" : "#mainehackerclub";
        public static string Nick = Program.TestMode ? "MaineHackerBot_test" : "MaineHackerBot";
        public static string AuthUser = Program.TestMode ? "" : "MaineHackerBot";
        public static string AuthPass = "2379dsn238sHkbsf";
        public static string AuthServ = "NickServ@services.";
        public static string ChanServ = "ChanServ@services.";
        public static bool UseSSL = true;
        public static bool AutoInv = true;
        
        static string ChannelToPrintNoticesIn = null;

        private static List<string> Channels = new List<string>();

        static IrcConnection connection = new IrcConnection();
        private static IrcClient irc = new IrcClient();

        private static bool m_Enabled;
        public static bool Enabled
        {
            get
            {
                return m_Enabled;
            }
            set
            {
                if (irc == null)
                {
                    m_Enabled = false;
                    Console.WriteLine("IRC: Disabling... -null-");
                }
                else
                {
                    Console.WriteLine("IRC: {0}abling...", value ? "En" : "Dis");
                    m_Enabled = irc.AutoRetry = irc.AutoReconnect = irc.AutoJoinOnInvite = irc.AutoRejoin = value;
                    if (!value)
                    {
                        if (irc.IsConnected)
                        {
                            Thread t = new Thread(new ThreadStart(irc.Disconnect)); // Shard was hanging on disconnect, this should fix i hope.
                            t.IsBackground = true;
                            t.Start();
                        }
                    }
                    else
                        Connect();
                }
            }
        }

        public static void Restart() // Use new thread.
        {
            Thread t = new Thread(new ThreadStart
            (
                delegate
                {
                    if (Enabled /*&& irc.IsConnected*/)
                    {
                        Enabled = false;
                        Thread.Sleep(5000);
                        Enabled = true;
                    }
                }
            ));
            t.IsBackground = true;
            t.Start();
        }

        public static void Initialize()
        {
            IPEndPoint ep = ReadAddressConfig();

            //irc.OnReadLine += new ReadLineEventHandler(irc_OnReadLine);
            irc.ActiveChannelSyncing = true;

            irc.AutoRetryDelay = 120;

            irc.OnConnected += new EventHandler(Client_OnConnected);
            irc.OnChannelMessage += new IrcEventHandler(Client_OnChannelMessage);
            irc.OnQueryMessage += new IrcEventHandler(Client_OnQueryMessage);
            irc.OnDisconnected += new EventHandler(irc_OnDisconnected);
            irc.OnInvite += new InviteEventHandler(irc_OnInvite);
            irc.OnKick += new KickEventHandler(irc_OnKick);
            irc.OnJoin += new JoinEventHandler(irc_OnJoin);
            irc.OnPart += new PartEventHandler(irc_OnPart);
            irc.OnBan += new BanEventHandler(irc_OnBan);
            irc.OnQueryNotice += new IrcEventHandler(Client_OnQueryNotice);
            irc.OnError += new Meebey.SmartIrc4net.ErrorEventHandler(irc_OnError);
            irc.OnConnectionError += new EventHandler(irc_OnConnectionError);

            LoadChannels();

        }

        private static Dictionary<string, IPAddress> rescache = new Dictionary<string, IPAddress>(100);


        static void irc_OnBan(object sender, BanEventArgs e) { if (ChannelsContains(e.Channel)) ChannelsRemove(e.Channel); }
        static void irc_OnPart(object sender, PartEventArgs e) { if (ChannelsContains(e.Channel)) ChannelsRemove(e.Channel); }
        static void irc_OnKick(object sender, KickEventArgs e) { if (ChannelsContains(e.Channel)) ChannelsRemove(e.Channel); }

        static void irc_OnInvite(object sender, InviteEventArgs e)
        {
            if (AutoInv)
                Join(e.Channel);
        }

        static void irc_OnJoin(object sender, JoinEventArgs e)
        {
            try
            {
                if (irc.IsMe(e.Who))
                {
                    Console.WriteLine("IRC: Joined {0}", e.Channel);
                    if (!ChannelsContains(e.Channel) && !isMainChannel(e.Channel))
                        ChannelsAdd(e.Channel);
                }
                else if (isMainChannel(e.Channel))
                {
                    CheckWelcomeByNick(e.Who);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("IRC Exception in irc_OnJoin: {0}", ex.Message);
            }
        }

        private static void CheckWelcomeByNick(string p)
        {
           //TODO: Say something if appropriate
        }

        static bool ChannelEquals(string channelA, string channelB)
        {
            return Insensitive.Equals(channelA, channelB);
        }

        static void ChannelsAdd(string channel)
        {
            if (!ChannelsContains(channel))
                Channels.Add(channel.ToLower());
        }

        static bool ChannelsContains(string channel)
        {
            return Channels.Contains(channel.ToLower());
        }

        static bool ChannelsRemove(string channel)
        {
            return Channels.Remove(channel.ToLower());
        }

        const string SaveFolder = ".";
        const string SaveFileName = "IRC-Channels.txt";
        static string SavePath = Path.Combine(SaveFolder, SaveFileName);

        static void LoadChannels()
        {
            Console.WriteLine("IRC: Loading channels: ");

            Channels.Clear();

            if (Program.TestMode)
            {
                Console.Write("Canceled, test mode.");
            }
            else if (File.Exists(SavePath))
                try
                {
                    using (StreamReader sr = new StreamReader(SavePath))
                    {
                        string channel;
                        while ((channel = sr.ReadLine()) != null)
                        {
                            Console.Write(channel + " ");
                            ChannelsAdd(channel);
                        }
                        sr.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Loading channels failed: {0}", ex.Message);
                }
            Console.WriteLine();
        }

        static void SaveChannels()
        {
            try
            {
                if (!Directory.Exists(SaveFolder)) Directory.CreateDirectory(SaveFolder);
                using (StreamWriter tw = new StreamWriter(SavePath, false))
                {
                    foreach (string channel in Channels)
                        tw.WriteLine(channel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("IRC: Saving channels failed: {0}", ex.Message);
            }
        }

        //static void irc_OnReadLine(object sender, ReadLineEventArgs e)
        //{
        //    irc.Listen(false);
        //}

        private static void Connect()
        {
            if (irc.IsConnected || !Enabled)
                return;

            try
            {
                ThreadStart ConnectThread = null;
                if (UseSSL)
                    ConnectThread = delegate { DoSSLConnect(); };
                else
                    ConnectThread = delegate { DoConnect(); };
                Thread t = new Thread(ConnectThread);
                System.Diagnostics.Trace.WriteLine("Starting IRC.Connect Thread.");
                t.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("IRC Error in Connect: {0}", e.Message);
            }
        }

        private static void DoSSLConnect()
        {
            DoSSLConnect(IRCServer, SSLPort);
        }

        private static void DoConnect()
        {
            DoConnect(IRCServer, Port);
        }

        private static void DoSSLConnect(string address, int port)
        {
            try
            {
                Console.WriteLine("IRC Connecting SSL: {0}:{1}", address, port);
                if (!irc.IsConnected)
                {
                    
                    irc.UseSsl = true;
                    irc.Connect(address, port);
                }
            }
            catch (CouldNotConnectException e)
            {
                if (e.Message.IndexOf("Could not connect to:") == 0 && e.Message.IndexOf("machine actively refused") > 0)
                {
                    Console.WriteLine("IRC Connection Failure, reconnect in 15 seconds: SSL {0}abled.", (UseSSL = !UseSSL) ? "en" : "dis");
                    Enabled = false;
                    Timer.DelayCall(TimeSpan.FromSeconds(15), Enable);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("IRC Error in DoSSLConnect: {0}:{1}", e.GetType().ToString(), e.Message);
            }
        }

        private static void DoConnect(string address, int port)
        {
            try
            {
                Console.WriteLine("IRC Connecting: {0}:{1}", address, port);
                if (!irc.IsConnected)
                {
                    irc.UseSsl = false;
                    irc.Connect(address, port);
                }
            }
            catch (CouldNotConnectException e)
            {
                if (e.Message.IndexOf("Could not connect to:") == 0 && e.Message.IndexOf("machine actively refused") > 0)
                {
                    Console.WriteLine("IRC Connection Failure, reconnect in 15 seconds: SSL {0}abled.", (UseSSL = !UseSSL) ? "en" : "dis");
                    Enabled = false;
                    Timer.DelayCall(TimeSpan.FromSeconds(15), Enable);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("IRC Error in DoConnect: {0}:{1}", e.GetType().ToString(), e.Message);
            }
        }

        static void Client_OnConnected(object sender, EventArgs e)
        {
            //Client.WriteLine(Rfc2812.Nick(Nick), Priority.Critical);
            //Client.WriteLine(Rfc2812.User(Nick, 0, Server.Misc.ServerList.ServerName), Priority.Critical);
            irc.Login(Nick, AuthUser);
            Console.WriteLine("IRC: Connected {2}to {0}:{1}", irc.Address, irc.Port, irc.UseSsl ? "SSL " : "");
            irc.SendMessage(SendType.Message, AuthServ, string.Format("IDENTIFY {0} {1}", AuthUser, AuthPass));
            irc.WriteLine(Rfc2812.Mode("+x"));
            Console.WriteLine("IRC: Joining {0}", MainChannel);
            irc.WriteLine(Rfc2812.Join(MainChannel));
            Timer.DelayCall(TimeSpan.FromSeconds(15), JoinChannels);
            DoListen();
        }

        static void JoinChannels()
        {
            if (irc.IsConnected && Enabled)
            {
                int secs = 5;

                foreach (string channel in Channels)
                {
                    Timer.DelayCall<string>((secs += 5) * 4, Join, channel);
                }
            }
        }

        static int errorcount = 0;
        static void DoListen()
        {
            try
            {
                irc.Listen();
            }
            catch (Exception ex)
            {
                errorcount++;
                Console.WriteLine("IRC Error {0}: {1}", errorcount, ex.ToString());
                if (errorcount < 4)
                {
                    System.Diagnostics.Trace.WriteLine("Sleeping IRC Listen Thread.");
                    Thread.Sleep(5000);
                    try
                    {
                        DoListen();
                    }
                    catch
                    {
                        Enabled = false;
                    }
                }
                else
                {
                    irc.Disconnect();
                    if (Enabled) Timer.DelayCall(TimeSpan.FromMinutes(5.0), Connect);
                    //Thread.CurrentThread.Abort();
                }
            }
        }

        private static void Join(string channel)
        {
            if (irc.IsConnected && Enabled)
            {
                Console.WriteLine("IRC: Rejoining {0}", channel);
                irc.WriteLine(Rfc2812.Join(channel), Priority.Low);
            }
        }

        static void irc_OnDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine("IRC: Disconnected.");
            if (Enabled)
                Timer.DelayCall(TimeSpan.FromSeconds(20), Connect);
        }

        static void Enable()
        {
            try { Enabled = true; }
            catch { }
        }

        static void irc_OnError(object sender, Meebey.SmartIrc4net.ErrorEventArgs e)
        {
            Console.WriteLine("IRC Error: {0}", e.ErrorMessage);
        }

        static void irc_OnConnectionError(object sender, EventArgs e)
        {
            Console.WriteLine("IRC Connection Error.");
        }


        public static void BroadcastAllChannels(string message)
        {
            BroadcastAllChannels(message, false);
        }

        public static void BroadcastAllChannels(string message, bool SendToStaff)
        {
            if (irc == null || !irc.IsConnected || message.Length <= 0)
                return;

            string[] channels = irc.GetChannels();
            if (channels == null || channels.Length <= 0)
                return;

            foreach (string channel in channels)
                irc.SendMessage(SendType.Message, channel, message);
        }

        static char IRCNoticePrefix = '!';

        static void Client_OnQueryNotice(object sender, IrcEventArgs e)
        {
            if (e.Data.From.IndexOf(IRCNoticePrefix) > 0)
                ProcessNoticeMessage(e.Data);
        }

        static void ProcessNoticeMessage(IrcMessageData data)
        {
            if (!string.IsNullOrWhiteSpace(ChannelToPrintNoticesIn))
                irc.SendMessage(SendType.Message, ChannelToPrintNoticesIn, data.From + ": " + data.Message);

            if (data.From == AuthServ || data.From.IndexOf(IRCNoticePrefix) + 1 == data.From.IndexOf(AuthServ) && data.Message.Contains("You are now identified "))
                SendSelfInvitesOnLogin();
        }

        static void SendSelfInvitesOnLogin()
        {
            //TODO: send self invites
        }

        static DateTime NextQueryCommand = DateTime.MinValue;
        static void Client_OnQueryMessage(object sender, IrcEventArgs e)
        {
            if (NextQueryCommand > DateTime.Now)
                return;

            if (e.Data.MessageArray.Length >= 1 && e.Data.MessageArray[0] == "!list")
            {
                string[] chans = irc.GetChannels();
                if (chans.Length > 0)
                {
                    string channels = String.Join(", ", chans);
                    irc.SendMessage(SendType.Message, e.Data.Nick, channels);
                }
                else
                    irc.SendMessage(SendType.Message, e.Data.Nick, "No channels");
            }
            else if (e.Data.Message.StartsWith("!"))
            {
                if (!ParseCommand(e.Data.MessageArray, e.Data.Nick))
                    SendHelp(e.Data.Nick);
                NextQueryCommand = DateTime.Now.AddSeconds(5.0);
            }
            else
            {
                SendHelp(e.Data.Nick);
                NextQueryCommand = DateTime.Now.AddSeconds(5.0);
            }
        }

        static void SendHelp(string to)
        {
            irc.SendMessage(SendType.Message, to, "The current commands are !status !events !motd !vote !time !rules and !help. You must wait 15 seconds between commands.", Priority.Low);
        }

        static DateTime NextChannelCommand = DateTime.MinValue;

        static void Client_OnChannelMessage(object sender, IrcEventArgs e)
        {
            if (NextChannelCommand > DateTime.Now)
                return;

            if (e.Data.Message.StartsWith("!"))
            {
                if (ParseCommand(e.Data.MessageArray, e.Data.Channel))
                    NextChannelCommand = DateTime.Now.AddSeconds(15.0);
            }
        }

        static bool isMainChannel(string channel)
        {
            return ChannelEquals(channel, MainChannel);
        }


        static string removeCommandPrefixChar(string command)
        {
            return command.Remove(0, 1);
        }

        static bool ParseCommand(string[] MessageArray, string from)
        {
            if (MessageArray == null || MessageArray.Length <= 0 || !MessageArray[0].StartsWith("!"))
                return false;

            string command = MessageArray[0];
            if (command.Length < 2)
                return false;

            command = removeCommandPrefixChar(command);

            bool SentToChannel = true;

            switch (command)
            {
                //case "crash":
                //    throw new Exception("Test Exception");
                //    break;
                case "time":
                    irc.SendMessage(SendType.Message, from, string.Format("The server time is {0}", DateTime.Now), Priority.Low);
                    break;
                case "rules":
                    if (isMainChannel(from))
                        irc.SendMessage(SendType.Message, from, "Welcome. This channel is for civil discussion. Read Rules: http://forum.uosecondage.com/viewtopic.php?f=2&t=104", Priority.Low);
                    else
                        irc.SendMessage(SendType.Message, from, "I am a guest in this channel. Please consult with the channel operators for information on their rules.", Priority.Low);
                    break;
                case "help":
                    SendHelp(from);
                    break;
                case "status":
                    irc.SendMessage(SendType.Message, from, "Hello there", Priority.Low);
                    break;
                case "vote":
                    irc.SendMessage(SendType.Message, from, "Please vote for Second Age using the links at: http://www.uosecondage.com/Vote", Priority.Low);
                    break;
                case "motd":
                    string info = "There is not a Message Of The Day at this time.";
                    irc.SendMessage(SendType.Message, from, info, Priority.Low);
                    break;
                case "part":
                    {
                        if (MessageArray.Length == 2)
                        {
                            string chan = MessageArray[1];
                            if (chan.IndexOf('#') == 0)
                            {
                                if (ChannelsContains(chan))
                                {
                                    irc.WriteLine(Rfc2812.Part(chan), Priority.High);
                                    irc.SendMessage(SendType.Message, MainChannel, "Parting " + chan, Priority.Medium);
                                    ChannelsRemove(chan);
                                }
                                else
                                    irc.SendMessage(SendType.Message, MainChannel, "To the best of my knowledge, I'm not in that channel.", Priority.Medium);
                            }
                            else
                            {
                                irc.SendMessage(SendType.Message, MainChannel, "Invalid channel.", Priority.Medium);
                            }
                        }
                        else
                            irc.SendMessage(SendType.Message, MainChannel, "usage: !part <channel>", Priority.Medium);
                        SentToChannel = false;
                    }
                    break;
                case "join":
                    {
                        if (MessageArray.Length == 2)
                        {
                            string chan = MessageArray[1];
                            if (chan.IndexOf('#') == 0)
                            {
                                if (!ChannelsContains(chan))
                                {
                                    irc.WriteLine(Rfc2812.Join(chan), Priority.High);
                                    irc.SendMessage(SendType.Message, MainChannel, "Joining " + chan, Priority.Medium);
                                }
                                else
                                    irc.SendMessage(SendType.Message, MainChannel, "To the best of my knowledge, I'm already in that channel. Try !part first.", Priority.Medium);
                            }
                            else
                            {
                                irc.SendMessage(SendType.Message, MainChannel, "Invalid channel.", Priority.Medium);
                            }
                        }
                        else
                            irc.SendMessage(SendType.Message, MainChannel, "usage: !join <channel>", Priority.Medium);
                        SentToChannel = false;
                    }
                    break;
                case "chanban":
                case "chankick":
                    {
                        if (MessageArray.Length < 2)
                        {
                            irc.SendMessage(SendType.Message, MainChannel, string.Format("usage: !{0} <user> [channel] [reason]", command), Priority.Medium);
                        }
                        else
                        {
                            string channel = MainChannel;
                            string user = MessageArray[1];
                            string reason = "Please review channel rules.";
                            if (MessageArray.Length >= 3)
                            {
                                channel = MessageArray[2];
                            }
                            if (MessageArray.Length >= 4)
                            {
                                reason = string.Join(" ", MessageArray, 3, MessageArray.Length - 3);
                            }

                            if (command == "chanban")
                            {
                                try
                                {
                                    IrcUser u = irc.GetIrcUser(user);
                                    if (u != null)
                                        irc.RfcMode(string.Format("{0} +b :*!{1}@{2}", channel, u.Ident, u.Host));
                                    else
                                        irc.SendMessage(SendType.Message, MainChannel, string.Format("User {0} not found.", user), Priority.Medium);
                                }
                                catch (Exception e)
                                {
                                    irc.SendMessage(SendType.Message, MainChannel, string.Format("Exception:{0}", e.Message), Priority.Medium);
                                }
                            }

                            irc.RfcKick(channel, user, reason);
                        }
                    }
                    break;
                default:
                    SentToChannel = false;
                    break;
            }

            return SentToChannel;
        }

        public static string FormatTimeSpan(TimeSpan ts)
        {
            return String.Format("{0:D2}:{1:D2}", ts.Hours, ts.Minutes % 60);
        }

        private static IPEndPoint ReadAddressConfig()
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
