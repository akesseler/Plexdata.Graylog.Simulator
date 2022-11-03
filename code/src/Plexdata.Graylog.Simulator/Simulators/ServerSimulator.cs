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
using System.Threading;
using System.Threading.Tasks;

namespace Plexdata.Graylog.Simulator.Simulators
{
    internal abstract class ServerSimulator : IServer
    {
        #region Public Events

        public event MessageReceivedDelegate MessageReceived;

        #endregion

        #region Private Fields

        private readonly ManualResetEvent interrupter = null;

        private readonly CancellationTokenSource termination = null;

        #endregion

        #region Construction

        protected ServerSimulator(AddressFamily family, Int32 port)
            : base()
        {
            this.Family = this.GetFamilyOrThrow(family);
            this.Port = this.GetPortOrThrow(port);

            this.interrupter = new ManualResetEvent(false);
            this.termination = new CancellationTokenSource();
        }

        #endregion

        #region Public Methods

        public abstract void Start();

        public virtual void Stop()
        {
            using (MethodTracer.Begin<IServer>())
            {
                try
                {
                    this.termination.Cancel();
                }
                catch (Exception exception)
                {
                    this.Error(exception);
                }

                try
                {
                    this.interrupter.Set();
                }
                catch (Exception exception)
                {
                    this.Error(exception);
                }
            }
        }

        #endregion

        #region Protected Properties

        protected AddressFamily Family { get; private set; }

        protected Int32 Port { get; private set; }

        protected Boolean IsTermination
        {
            get
            {
                return this.termination.IsCancellationRequested;
            }
        }

        protected ManualResetEvent Interrupter
        {
            get
            {
                return this.interrupter;
            }
        }

        protected CancellationToken Cancellator
        {
            get
            {
                return this.termination.Token;
            }
        }

        #endregion

        #region Protected Methods

        protected EndPoint GetEndPoint()
        {
            IPAddress address = this.Family == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any;

            return new IPEndPoint(address, this.Port);
        }

        protected String GetReadableFamily()
        {
            switch (this.Family)
            {
                case AddressFamily.InterNetwork:
                    return "IPv4";
                case AddressFamily.InterNetworkV6:
                    return "IPv6";
                case AddressFamily.Unspecified: // Necessary for WEB server (HTTP) support.
                    return "HTTP";
                default:
                    return "unknown";
            }
        }

        protected String GetReadablePort()
        {
            return this.Port.ToString();
        }

        protected void RaiseMessageReceived(String display, String message)
        {
            try
            {
                if (this.MessageReceived is null) { return; }

                Task.Factory
                    .StartNew(() => { this.MessageReceived.Invoke(this, new MessageReceivedEventArgs(display, message)); })
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.Error(exception);
            }
        }

        #endregion

        #region Private Methods

        private AddressFamily GetFamilyOrThrow(AddressFamily family)
        {
            switch (family)
            {
                case AddressFamily.InterNetwork:
                case AddressFamily.InterNetworkV6:
                case AddressFamily.Unspecified: // Necessary for WEB server (HTTP) support.
                    return family;
                default:
                    throw new ArgumentOutOfRangeException(nameof(family), "Address family must be either IPv4 or IPv6.");
            }
        }

        private Int32 GetPortOrThrow(Int32 port)
        {
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException(nameof(port), $"Parameter \"{nameof(port)}\" must be in range of [{IPEndPoint.MinPort}...{IPEndPoint.MaxPort}].");
            }

            return port;
        }

        #endregion
    }
}
