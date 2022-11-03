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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Plexdata.Graylog.Simulator.Simulators
{
    internal class WebServerSimulator : ServerSimulator, IWebServer
    {
        #region Private Fields

        private readonly ISimulatorFactory factory = null;
        private readonly IRequestProcessor handler = null;
        private readonly IEncodingFacade encoding = null;

        private readonly HttpListener listener = null;

        #endregion

        #region Construction

        public WebServerSimulator(ISimulatorFactory factory, AddressFamily family, Int32 port)
            : base(family, port)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.handler = this.factory.Create<IRequestProcessor>();
            this.encoding = this.factory.Create<IEncodingFacade>();

            this.handler.RequestCompleted += this.OnProcessorRequestCompleted;

            this.listener = new HttpListener();
        }

        #endregion

        #region Public Methods

        public override void Start()
        {
            using (MethodTracer.Begin<IWebServer>())
            {
                this.Print("Starting WEB server for {0} on port {1}.", base.GetReadableFamily(), base.GetReadablePort());
                this.Print(ColorSwitch.AttentionColor, "ATTENTION: Only HTTP on 'localhost' is supported at the moment!");

                // HTTPS and something like "http://*:<port>/" is not supported for various
                // reasons. Therefore, this simple but working way is used instead.
                this.listener.Prefixes.Add($"http://localhost:{base.Port}/");

                Task.Factory
                    .StartNew(() => { this.Execute(); })
                    .ConfigureAwait(false);
            }
        }

        public override void Stop()
        {
            using (MethodTracer.Begin<IWebServer>())
            {
                this.Print("Shutting down WEB server for {0} on port {1}.", base.GetReadableFamily(), base.GetReadablePort());

                base.Stop();

                try
                {
                    this.listener.Stop();
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
                this.listener.Start();

                while (!base.IsTermination)
                {
                    HttpListenerContext context = this.listener.GetContext();

                    this.ProcessRequest(context);
                    this.HandleResponse(context);
                }
            }
            catch (Exception exception)
            {
                this.Error(exception);
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            using (MethodTracer.Begin<IWebServer>())
            {
                HttpListenerRequest request = context.Request;

                using (Stream stream = request.InputStream)
                {
                    using (StreamReader reader = new StreamReader(stream, request.ContentEncoding))
                    {
                        String message = reader.ReadToEnd();
                        this.Dump(this.encoding.GetBuffer(message), "WEB", base.GetReadableFamily(), base.GetReadablePort());
                        this.handler.Process(message);
                    }
                }
            }
        }

        private void HandleResponse(HttpListenerContext context)
        {
            using (HttpListenerResponse response = context.Response)
            {
                response.StatusCode = (Int32)HttpStatusCode.OK;
            }
        }

        private void OnProcessorRequestCompleted(Object sender, RequestCompletedEventArgs args)
        {
            base.RaiseMessageReceived($"Message received at WEB server for {base.GetReadableFamily()} on port {base.GetReadablePort()}.", args.Message);
        }

        #endregion
    }
}
