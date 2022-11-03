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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Plexdata.Graylog.Simulator.Simulators
{
    internal class UdpServerSimulator : ServerSimulator, IUdpServer
    {
        #region Private Fields

        private readonly ISimulatorFactory factory = null;
        private readonly IRequestProcessor handler = null;
        private readonly Byte[] buffer = null;
        private readonly Socket socket = null;
        private EndPoint remote = null;

        #endregion

        #region Construction

        public UdpServerSimulator(ISimulatorFactory factory, AddressFamily family, Int32 port)
            : base(family, port)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.handler = this.factory.Create<IRequestProcessor>();

            this.handler.RequestCompleted += this.OnProcessorRequestCompleted;

            this.socket = new Socket(base.Family, SocketType.Dgram, ProtocolType.Udp);

            // Maximum length of a UDP datagram is limited by a 16-bit length
            // field within the IP header. But here one byte more is allocated.
            // SEE: https://flylib.com/books/en/3.223.1.129/1/
            // SEE: https://referencesource.microsoft.com/#System/net/System/Net/Sockets/UDPClient.cs
            this.buffer = new Byte[0x10000];
        }

        #endregion

        #region Public Methods

        public override void Start()
        {
            using (MethodTracer.Begin<IUdpServer>())
            {
                this.Print("Starting UDP server for {0} on port {1}.", base.GetReadableFamily(), base.GetReadablePort());

                this.socket.Bind(base.GetEndPoint());

                Task.Factory
                    .StartNew(() => this.Execute(), base.Cancellator)
                    .ConfigureAwait(false);
            }
        }

        public override void Stop()
        {
            using (MethodTracer.Begin<IUdpServer>())
            {
                this.Print("Shutting down UDP server for {0} on port {1}.", base.GetReadableFamily(), base.GetReadablePort());

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

        private void Execute()
        {
            try
            {
                this.remote = base.GetEndPoint();

                while (!base.IsTermination)
                {
                    base.Interrupter.Reset();

                    this.socket.BeginReceiveFrom(
                        this.buffer, 0,
                        this.buffer.Length,
                        SocketFlags.None,
                        ref this.remote,
                        new AsyncCallback(this.ReceiveCallback),
                        this);

                    base.Interrupter.WaitOne();
                }
            }
            catch (Exception exception)
            {
                this.Error(exception);
            }
        }

        private void ReceiveCallback(IAsyncResult caller)
        {
            using (MethodTracer.Begin<IUdpServer>())
            {
                if (base.IsTermination)
                {
                    return;
                }

                try
                {
                    Int32 received = this.socket.EndReceiveFrom(caller, ref this.remote);

                    Byte[] request = this.CopyData(this.buffer, received);
                    this.Dump(request, "UDP", base.GetReadableFamily(), base.GetReadablePort());
                    this.handler.Process(request);

                    base.Interrupter.Set();
                }
                catch (Exception exception)
                {
                    this.Error(exception);
                }
            }
        }

        private Byte[] CopyData(Byte[] source, Int32 length)
        {
            Byte[] result = new Byte[length];

            Buffer.BlockCopy(source, 0, result, 0, length);

            return result;
        }

        private void OnProcessorRequestCompleted(Object sender, RequestCompletedEventArgs args)
        {
            base.RaiseMessageReceived($"Message received at UDP server for {base.GetReadableFamily()} on port {base.GetReadablePort()}.", args.Message);
        }

        #endregion
    }
}
