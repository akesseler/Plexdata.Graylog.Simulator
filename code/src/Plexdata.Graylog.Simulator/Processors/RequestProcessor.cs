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
using Plexdata.Graylog.Simulator.Interfaces;
using Plexdata.Graylog.Simulator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Plexdata.Graylog.Simulator.Processors
{
    internal class RequestProcessor : IRequestProcessor
    {
        #region Public Events

        public event RequestCompletedDelegate RequestCompleted;

        #endregion

        #region Private Fields

        private readonly ISimulatorFactory factory = null;
        private readonly IGZipHelper zipper = null;
        private readonly IRequestQueue messages = null;
        private readonly IEncodingFacade encoding = null;

        private readonly Object interlock = null;
        private readonly IDictionary<Int64, GelfMessageChunk[]> pendings = null;

        #endregion

        #region Construction

        public RequestProcessor(ISimulatorFactory factory)
            : base()
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

            this.zipper = this.factory.Create<IGZipHelper>();
            this.messages = this.factory.Create<IRequestQueue>();
            this.encoding = this.factory.Create<IEncodingFacade>();

            this.interlock = new Object();
            this.pendings = new Dictionary<Int64, GelfMessageChunk[]>();

            this.messages.Enqueued += this.OnMessageEnqueued;
        }

        #endregion

        #region Public Methods

        public void Process(String request)
        {
            if (String.IsNullOrEmpty(request))
            {
                return;
            }

            this.messages.Enqueue(this.encoding.GetBuffer(request));
        }

        public void Process(Byte[] request)
        {
            if (!request?.Any() ?? true)
            {
                return;
            }

            this.messages.Enqueue(request);
        }

        #endregion

        #region Private Methods

        private void OnMessageEnqueued(Object sender, EventArgs args)
        {
            Byte[] request = this.messages.Dequeue();

            if (request is null)
            {
                return;
            }

            lock (this.interlock)
            {
                try
                {
                    if (GelfMessageChunk.IsGelfChunkFormat(request))
                    {
                        if (!GelfMessageChunk.TryParse(request, out GelfMessageChunk result))
                        {
                            this.Print("Message rejected because payload does not include some kind of GELF header.");
                            return;
                        }

                        this.InsertPendingMessage(result);
                        this.FinishPendingMessage(result);

                        return;
                    }

                    if (this.IsGZipFormat(request))
                    {
                        request = this.zipper.Decompress(request);
                    }

                    this.RaiseRequestCompleted(request);
                }
                catch (Exception exception)
                {
                    this.Error(exception);
                }
            }
        }

        private void InsertPendingMessage(GelfMessageChunk message)
        {
            if (!this.pendings.ContainsKey(message.MessageId))
            {
                this.pendings.Add(message.MessageId, new GelfMessageChunk[message.TotalSequences]);
            }

            this.pendings[message.MessageId][message.SequenceNumber] = message;
        }

        private void FinishPendingMessage(GelfMessageChunk message)
        {
            if (this.pendings[message.MessageId].Any(x => x is null))
            {
                return;
            }

            GelfMessageChunk[] messages = this.pendings[message.MessageId];

            this.pendings.Remove(message.MessageId);

            this.AssembleMessagePayload(messages);
        }

        private void AssembleMessagePayload(GelfMessageChunk[] messages)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                foreach (GelfMessageChunk current in messages)
                {
                    stream.Write(current.MessagePayload);
                }

                Byte[] payload = stream.ToArray();

                if (this.IsGZipFormat(payload))
                {
                    payload = this.zipper.Decompress(payload);
                }

                this.RaiseRequestCompleted(payload);
            }
        }

        private void RaiseRequestCompleted(Byte[] payload)
        {
            try
            {
                this.RequestCompleted?.Invoke(this, new RequestCompletedEventArgs(this.encoding.GetString(payload)));
            }
            catch (Exception exception)
            {
                this.Error(exception);
            }
        }

        private Boolean IsGZipFormat(Byte[] buffer)
        {
            // GZIP magic bytes: 1F 8B
            return buffer.Length >= 2 && buffer[0] == 0x1F && buffer[1] == 0x8B;
        }

        #endregion
    }
}
