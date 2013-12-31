using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Reflection;
using System.IO;
using System.Threading;

namespace HackMaineIrcBot
{
    static class Program
    {

        public static bool TestMode { get; set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        static WaitHandle[] waitHandles = new WaitHandle[] { new AutoResetEvent(false) };

        static void Main(string[] args)
        {
            if (args.Length == 1 &&
                (Insensitive.Equals(args[0], "-install") || Insensitive.Equals(args[0], "-uninstall")
                || Insensitive.Equals(args[0], "-restart") || Insensitive.Equals(args[0], "-stop")
                || Insensitive.Equals(args[0], "-start") || Insensitive.Equals(args[0], "-con")))
            {
                if (Insensitive.Equals(args[0], "-restart"))
                {
                    IrcBotSvc.ServiceControlRestart();
                }
                else if (Insensitive.Equals(args[0], "-stop"))
                {
                    IrcBotSvc.ServiceControlStop();
                }
                else if (Insensitive.Equals(args[0], "-start"))
                {
                    IrcBotSvc.ServiceControlStart();
                }
                else if (Insensitive.Equals(args[0], "-install"))
                {
                    ServiceInstaller si = new ServiceInstaller();
                    if (si.InstallService(IrcBotSvc.GlobalServiceName + " -service", IrcBotSvc.GlobalServiceName, IrcBotSvc.GlobalServiceName + " Service"))
                        Console.WriteLine("The JoinUOPoller service has been installed.");
                    else
                        Console.WriteLine("An error occurred during service installation.");
                }
                else if (Insensitive.Equals(args[0], "-uninstall"))
                {
                    ServiceInstaller si = new ServiceInstaller();
                    if (si.UnInstallService(IrcBotSvc.GlobalServiceName))
                        Console.WriteLine("The " + IrcBotSvc.GlobalServiceName + " service has been uninstalled. You may need to reboot to remove it completely.");
                    else
                        Console.WriteLine("An error occurred during service removal.");
                }
                else if (Insensitive.Equals(args[0], "-con"))
                {
                    Irc.Initialize();

                    while (Console.KeyAvailable) Console.ReadKey(false);
                    Irc.Enabled = true;
                    Console.WriteLine("Press any key to exit...");

                    ThreadPool.QueueUserWorkItem(new WaitCallback(GetKey), waitHandles[0]);
                    int index = WaitHandle.WaitAny(waitHandles);

                    Irc.Enabled = false;
                }

                return;
            }

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new IrcBotSvc()
			};
            ServiceBase.Run(ServicesToRun);
        }

        static void GetKey(Object state)
        {
            AutoResetEvent are = (AutoResetEvent)state;
            Console.ReadKey(false);
            are.Set();
        }

    }

    public class FileLogger : TextWriter, IDisposable
    {
        private string m_FileName;
        private bool m_NewLine;
        public const string DateFormat = "[MMM d HH:mm:ss]: ";

        public string FileName { get { return m_FileName; } }

        public FileLogger(string file)
            : this(file, false)
        {
        }

        public FileLogger(string file, bool append)
        {
            m_FileName = file;
            using (StreamWriter writer = new StreamWriter(new FileStream(m_FileName, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read)))
            {
                writer.WriteLine(">>>Logging started on {0}.", DateTime.Now.ToString("f")); //f = Tuesday, April 10, 2001 3:51 PM 
            }
            m_NewLine = true;
        }

        public override void Write(char ch)
        {
            using (StreamWriter writer = new StreamWriter(new FileStream(m_FileName, FileMode.Append, FileAccess.Write, FileShare.Read)))
            {
                if (m_NewLine)
                {
                    writer.Write(DateTime.Now.ToString(DateFormat));
                    m_NewLine = false;
                }
                writer.Write(ch);
            }
        }

        public override void Write(string str)
        {
            using (StreamWriter writer = new StreamWriter(new FileStream(m_FileName, FileMode.Append, FileAccess.Write, FileShare.Read)))
            {
                if (m_NewLine)
                {
                    writer.Write(DateTime.Now.ToString(DateFormat));
                    m_NewLine = false;
                }
                writer.Write(str);
            }
        }

        public override void WriteLine(string line)
        {
            using (StreamWriter writer = new StreamWriter(new FileStream(m_FileName, FileMode.Append, FileAccess.Write, FileShare.Read)))
            {
                if (m_NewLine)
                    writer.Write(DateTime.Now.ToString(DateFormat));
                writer.WriteLine(line);
                m_NewLine = true;
            }
        }

        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.Default; }
        }
    }

    public class MultiTextWriter : TextWriter
    {
        private List<TextWriter> m_Streams;

        public MultiTextWriter(params TextWriter[] streams)
        {
            m_Streams = new List<TextWriter>(streams);

            if (m_Streams.Count < 0)
                throw new ArgumentException("You must specify at least one stream.");
        }

        public void Add(TextWriter tw)
        {
            m_Streams.Add(tw);
        }

        public void Remove(TextWriter tw)
        {
            m_Streams.Remove(tw);
        }

        public override void Write(char ch)
        {
            for (int i = 0; i < m_Streams.Count; i++)
                m_Streams[i].Write(ch);
        }

        public override void WriteLine(string line)
        {
            for (int i = 0; i < m_Streams.Count; i++)
                m_Streams[i].WriteLine(line);
        }

        public override void WriteLine(string line, params object[] args)
        {
            WriteLine(String.Format(line, args));
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
}
