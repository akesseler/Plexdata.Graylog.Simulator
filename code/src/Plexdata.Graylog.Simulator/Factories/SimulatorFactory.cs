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

using Plexdata.Graylog.Simulator.Facades;
using Plexdata.Graylog.Simulator.Helpers;
using Plexdata.Graylog.Simulator.Interfaces;
using Plexdata.Graylog.Simulator.Parsers;
using Plexdata.Graylog.Simulator.Processors;
using Plexdata.Graylog.Simulator.Queuing;
using Plexdata.Graylog.Simulator.Simulators;
using System;
using System.Net.Sockets;

namespace Plexdata.Graylog.Simulator.Factories
{
    internal class SimulatorFactory : ISimulatorFactory
    {
        #region Public Methods

        public TInterface Create<TInterface>()
        {
            if (typeof(TInterface) == typeof(IRequestProcessor))
            {
                return (TInterface)(IRequestProcessor)new RequestProcessor(this);
            }

            if (typeof(TInterface) == typeof(IGZipHelper))
            {
                return (TInterface)(IGZipHelper)new GZipHelper();
            }

            if (typeof(TInterface) == typeof(IRequestQueue))
            {
                return (TInterface)(IRequestQueue)new RequestQueue();
            }

            if (typeof(TInterface) == typeof(IEncodingFacade))
            {
                return (TInterface)(IEncodingFacade)new EncodingFacade();
            }

            if (typeof(TInterface) == typeof(IGelfMessageParser))
            {
                return (TInterface)(IGelfMessageParser)new GelfMessageParser();
            }

            throw new NotSupportedException();
        }

        public TInterface CreateServer<TInterface>(AddressFamily family, Int32 port)
        {
            if (typeof(TInterface) == typeof(IUdpServer))
            {
                return (TInterface)(IUdpServer)new UdpServerSimulator(this, family, port);
            }

            if (typeof(TInterface) == typeof(ITcpServer))
            {
                return (TInterface)(ITcpServer)new TcpServerSimulator(this, family, port);
            }

            if (typeof(TInterface) == typeof(IWebServer))
            {
                return (TInterface)(IWebServer)new WebServerSimulator(this, family, port);
            }

            throw new NotSupportedException();
        }

        #endregion
    }
}
