/*
 * MIT License
 * 
 * Copyright (c) 2022 plexdata.de
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Plexdata.ArgumentParser.Exceptions;
using Plexdata.ArgumentParser.Extensions;
using Plexdata.Graylog.Simulator.Events;
using Plexdata.Graylog.Simulator.Extensions;
using Plexdata.Graylog.Simulator.Factories;
using Plexdata.Graylog.Simulator.Helpers;
using Plexdata.Graylog.Simulator.Interfaces;
using Plexdata.Graylog.Simulator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Plexdata.Graylog.Simulator
{
    internal class Program
    {
        #region Private Fields

        private static ProgramSettings settings = null;

        private readonly CancellationTokenSource blocker = null;
        private readonly List<IServer> servers = null;
        private readonly ISimulatorFactory factory = null;
        private readonly IGelfMessageParser parser = null;
        private readonly Object interlock = null;

        #endregion

        #region Public Methods

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Program program = new Program();
            program.Execute();
        }

        #endregion

        #region Construction

        private Program()
            : base()
        {
            AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledExceptionHandler;
            Console.CancelKeyPress += this.OnCancelKeyPressHandler;
            this.blocker = new CancellationTokenSource();
            this.servers = new List<IServer>();
            this.factory = new SimulatorFactory();

            this.parser = this.factory.Create<IGelfMessageParser>();

            this.interlock = new Object();

        }

        #endregion

        #region Public Properties

        public static ProgramSettings Settings
        {
            get
            {
                if (Program.settings is null)
                {
                    Program.settings = new ProgramSettings();

                    String[] options = Environment.CommandLine.Extract(true);

                    if (options?.Any() ?? false)
                    {
                        Program.settings.Process(options);
                        Program.settings.Validate();
                    }
                }

                return Program.settings;
            }
        }

        #endregion

        #region Private Methods

        private void Execute()
        {
            try
            {
                if (Program.Settings.IsHelp)
                {
                    this.PrintHelp();
                    return;
                }

                Console.WriteLine();

                this.PrintVersion();

                if (!this.Startup())
                {
                    return;
                }

                this.Print(ColorSwitch.AccentColor, "{0}Press CTRL+C to terminate program...{0}", Environment.NewLine);

                this.blocker.Token.WaitHandle.WaitOne();

                this.Shutdown();
            }
            catch (SupportViolationException exception)
            {
                this.Error(exception);
                this.PrintHelp();
            }
            catch (Exception exception)
            {
                this.Error(exception);
            }
        }

        private Boolean Startup()
        {
            using (MethodTracer.Begin<Program>())
            {
                IServer server;

                if (Program.Settings.IsUdpIPv4Type || Program.Settings.IsRunAllTypes)
                {
                    server = this.factory.CreateServer<IUdpServer>(AddressFamily.InterNetwork, Program.Settings.UdpIPv4Prot);
                    server.MessageReceived += this.OnServerMessageReceived;
                    server.Start();
                    this.servers.Add(server);
                }

                if (Program.Settings.IsUdpIPv6Type || Program.Settings.IsRunAllTypes)
                {
                    server = this.factory.CreateServer<IUdpServer>(AddressFamily.InterNetworkV6, Program.Settings.UdpIPv6Prot);
                    server.MessageReceived += this.OnServerMessageReceived;
                    server.Start();
                    this.servers.Add(server);
                }

                if (Program.Settings.IsTcpIPv4Type || Program.Settings.IsRunAllTypes)
                {
                    server = this.factory.CreateServer<ITcpServer>(AddressFamily.InterNetwork, Program.Settings.TcpIPv4Prot);
                    server.MessageReceived += this.OnServerMessageReceived;
                    server.Start();
                    this.servers.Add(server);
                }

                if (Program.Settings.IsTcpIPv6Type || Program.Settings.IsRunAllTypes)
                {
                    server = this.factory.CreateServer<ITcpServer>(AddressFamily.InterNetworkV6, Program.Settings.TcpIPv6Prot);
                    server.MessageReceived += this.OnServerMessageReceived;
                    server.Start();
                    this.servers.Add(server);
                }

                if (Program.Settings.IsWebHttpType || Program.Settings.IsRunAllTypes)
                {
                    server = this.factory.CreateServer<IWebServer>(AddressFamily.Unspecified, Program.Settings.WebHttpProt);
                    server.MessageReceived += this.OnServerMessageReceived;
                    server.Start();
                    this.servers.Add(server);
                }

                if (this.servers.Count < 1)
                {
                    this.Print(ColorSwitch.AttentionColor, "No server to run...");
                    this.PrintHelp();
                    return false;
                }

                return true;
            }
        }

        private void Shutdown()
        {
            using (MethodTracer.Begin<Program>())
            {
                foreach (IServer server in this.servers)
                {
                    server.Stop();
                }
            }
        }

        private void PrintHelp()
        {
            this.Print(Program.Settings.Generate());
            this.WaitForAnyKeyPress();
        }

        private void PrintVersion()
        {
            if (!Program.Settings.IsVersion)
            {
                return;
            }

            IEnumerable<Attribute> attributes = Assembly.GetEntryAssembly().GetCustomAttributes();

            this.Print("Graylog Simulator (Version {0}){2}{1}{2}",
                (attributes.Where(x => x is AssemblyInformationalVersionAttribute).First() as AssemblyInformationalVersionAttribute).InformationalVersion,
                (attributes.Where(x => x is AssemblyCopyrightAttribute).First() as AssemblyCopyrightAttribute).Copyright, Environment.NewLine);
        }

        private void OnUnhandledExceptionHandler(Object sender, UnhandledExceptionEventArgs args)
        {
            Exception error = (Exception)args.ExceptionObject;

            sender.Error(error, false);

            if (args.IsTerminating)
            {
                Environment.Exit(error.HResult);
            }
        }

        private void OnCancelKeyPressHandler(Object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;

            if (Program.Settings.IsHelp) { return; }

            this.Print("Program termination signaled. Shutting down...");

            this.blocker.Cancel();
        }

        private void OnServerMessageReceived(Object sender, MessageReceivedEventArgs args)
        {
            lock (this.interlock)
            {
                using (MethodTracer.Begin<Program>("MessageReceived"))
                {
                    this.Print(args.Display);

                    if (this.parser.TryParse(args.Message, out GelfMessageValues values))
                    {
                        this.Print("Common GELF message values:");
                        foreach (GelfMessageValue value in values.CommonValues)
                        {
                            this.Print($" * {value}");
                        }

                        this.Print("Custom GELF message values:");
                        foreach (GelfMessageValue value in values.CustomValues)
                        {
                            this.Print($" * {value}");
                        }
                    }
                    else
                    {
                        this.Print(ColorSwitch.WarningColor, "WARNING: Unable to convert message into a GELF message.");
                        this.Print(args.Message);
                    }
                }
            }
        }

        #endregion
    }
}
