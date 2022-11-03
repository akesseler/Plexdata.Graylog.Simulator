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

using Plexdata.Graylog.Simulator.Events;
using Plexdata.Graylog.Simulator.Extensions;
using Plexdata.Graylog.Simulator.Helpers;
using Plexdata.Graylog.Simulator.Interfaces;
using System;
using System.IO;
using System.Net.Sockets;

namespace Plexdata.Graylog.Simulator.Simulators
{
    internal class TcpServerSimulator : ServerSimulator, ITcpServer
    {
        #region Private Fields

        private readonly ISimulatorFactory factory = null;
        private readonly IRequestProcessor handler = null;
        private readonly Socket socket = null;

        #endregion

        #region Construction

        public TcpServerSimulator(ISimulatorFactory factory, AddressFamily family, Int32 port)
            : base(family, port)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.handler = this.factory.Create<IRequestProcessor>();

            this.handler.RequestCompleted += this.OnProcessorRequestCompleted;

            this.socket = new Socket(base.Family, SocketType.Stream, ProtocolType.Tcp);
        }

        #endregion

        #region Public Methods

        public override void Start()
        {
            using (MethodTracer.Begin<ITcpServer>())
            {
                this.Print("Starting TCP server for {0} on port {1}.", base.GetReadableFamily(), base.GetReadablePort());

                this.socket.Bind(base.GetEndPoint());

                this.socket.Listen(10);

                this.socket.BeginAccept(new AsyncCallback(this.AcceptCallback), this);
            }
        }

        public override void Stop()
        {
            using (MethodTracer.Begin<ITcpServer>())
            {
                this.Print("Shutting down TCP server for {0} on port {1}.", base.GetReadableFamily(), base.GetReadablePort());

                base.Stop();

                try
                {
                    this.socket.Close();
                }
                catch (Exception exception)
                {
                    this.Error(exception);
                }
            }
        }

        #endregion

        #region Private Methods

        private void AcceptCallback(IAsyncResult caller)
        {
            if (base.IsTermination)
            {
                return;
            }

            try
            {
                Socket socket = this.socket.EndAccept(caller);

                this.socket.BeginAccept(new AsyncCallback(this.AcceptCallback), this);

                this.ProcessRequest(socket);
            }
            catch (Exception exception)
            {
                this.Error(exception);
            }
        }

        private void ProcessRequest(Socket socket)
        {
            using (MethodTracer.Begin<ITcpServer>())
            {
                MemoryStream payload = new MemoryStream();

                try
                {
                    Byte[] buffer = new Byte[1024];

                    while (socket.Connected && !base.IsTermination)
                    {
                        Int32 received = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

                        // The client closed the connection.
                        if (received < 1)
                        {
                            Byte[] request = payload.ToArray();
                            this.Dump(request, "TCP", base.GetReadableFamily(), base.GetReadablePort());
                            this.handler.Process(request);
                            break;
                        }

                        // Try find message zero termination.
                        Int32 termination = -1;

                        for (Int32 index = 0; index < received; index++)
                        {
                            if (buffer[index] == 0)
                            {
                                termination = index;
                                break;
                            }
                        }

                        if (termination != -1)
                        {
                            payload.Write(buffer, 0, termination);

                            Byte[] request = payload.ToArray();
                            this.Dump(request, "TCP", base.GetReadableFamily(), base.GetReadablePort());
                            this.handler.Process(request);

                            // Reuse already allocated stream memory!
                            payload.Position = 0;
                            payload.SetLength(0);

                            // Save outstanding partial message and continue.
                            termination++;
                            payload.Write(buffer, termination, received - termination);
                        }
                        else
                        {
                            // Save current partial message and continue.
                            payload.Write(buffer, 0, received);
                        }
                    }
                }
                finally
                {
                    payload.Dispose();
                    payload = null;
                }
            }
        }

        private void OnProcessorRequestCompleted(Object sender, RequestCompletedEventArgs args)
        {
            base.RaiseMessageReceived($"Message received at TCP server for {base.GetReadableFamily()} on port {base.GetReadablePort()}.", args.Message);
        }

        #endregion
    }
}
