﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HackMaineIrcBot
{
    public partial class IrcBotSvc : ServiceBase
    {
        private static MultiTextWriter m_MultiConOut;

        public const string GlobalServiceName = "MaineHackerBot";
        public IrcBotSvc()
        {
            InitializeComponent();
            CanPauseAndContinue = false;
            ServiceName = GlobalServiceName;
            CanHandlePowerEvent = false;
            CanHandleSessionChangeEvent = false;
            CanStop = true;

            this.EventLog.Source = ServiceName;

            string datecode = DateTime.Now.ToString("yyyyMMddHHmmss");
            string FileName = string.Format("{0}.log", datecode);
            string path = Path.Combine(Program.WorkingFolder, @"logs\console\");
            Directory.CreateDirectory(path);
            Console.SetOut(m_MultiConOut = new MultiTextWriter(Console.Out, new FileLogger(Path.Combine(path, FileName))));
        }

        public static void ServiceControlStop()
        {
            System.ServiceProcess.ServiceController Service;
            Service = new ServiceController(GlobalServiceName);
            if (Service == null)
                Console.WriteLine(" - Service Stop failed. Service is not installed.");
            else if (Service.Status == ServiceControllerStatus.Running)
            {
                Service.Stop();
                Console.WriteLine(" - Stop Signal Sent.");
            }
            else
                Console.WriteLine(" - Service Stop failed. Service is not running (" + Service.Status + ".");
        }

        public static bool IsRunning
        {
            get
            {
                System.ServiceProcess.ServiceController Service;
                Service = new ServiceController(GlobalServiceName);
                try
                {
                    return (Service != null && Service.Status == ServiceControllerStatus.Running);
                }
                catch
                {
                    return false;
                }
            }
        }

        public static void ServiceControlStart()
        {
            System.ServiceProcess.ServiceController Service;
            Service = new ServiceController(GlobalServiceName);
            if (Service == null)
                Console.WriteLine(" - Service Start failed. Service is not installed.");
            else if (Service.Status != ServiceControllerStatus.Running)
            {
                Service.Start();
                Console.WriteLine(" - Start Signal Sent.");
            }
            else
                Console.WriteLine(" - Service Start failed. Service is already running.");

        }

        public static void ServiceControlRestart()
        {
            System.ServiceProcess.ServiceController Service;
            Service = new ServiceController(GlobalServiceName);
            System.Diagnostics.EventLog.WriteEntry(GlobalServiceName, " - Service Restart attempt.");
            Console.WriteLine("Service Restart attempt...");
            if (Service == null)
                Console.WriteLine(" - Service Restart failed. Service is not installed.");
            else if (Service.Status == ServiceControllerStatus.Running)
            {
                Service.Stop();
                Console.WriteLine(" - Stop Signal Sent.");

                DateTime Timeout = DateTime.Now.AddSeconds(30);
                while (Timeout > DateTime.Now && Service.Status != ServiceControllerStatus.Stopped)
                {
                    Thread.Sleep(500);
                    Service.Refresh();
                }

                if (Service.Status == ServiceControllerStatus.Stopped)
                {
                    Service.Start();
                    Console.WriteLine(" - Start Signal Sent.");
                }
                else
                {
                    Console.WriteLine(" - Service Restart failed. Service did not stop. Status:" + Service.Status.ToString());
                    System.Diagnostics.EventLog.WriteEntry(GlobalServiceName, " - Service Restart failed. Service did not stop.");
                }
            }
            else
            {
                Console.WriteLine(" - Service Restart failed. Service not running.");
                System.Diagnostics.EventLog.WriteEntry(GlobalServiceName, " - Service Restart failed. Service not running.");
            }
        }

        private Thread workerThread;
        protected override void OnStart(string[] args)
        {
            Console.WriteLine("Service Starting...");

            workerThread = new Thread(new ThreadStart(Program.Run));
            System.Diagnostics.Trace.WriteLine("Starting Service Worker Thread.");
            workerThread.Start();

            //myServiceStatus.currentState = (int)State.SERVICE_RUNNING;
            //SetServiceStatus(handle, myServiceStatus);
        }

        protected override void OnStop()
        {
            Console.WriteLine("Service Stopping...");
            Program.Kill(false);
        }

        protected override void OnPause()
        {
            Console.WriteLine("Service OnPause...");
            Program.Pause();
        }
        protected override void OnContinue()
        {
            Console.WriteLine("Service OnContinue...");
            Program.Continue();
        }
    }
}
