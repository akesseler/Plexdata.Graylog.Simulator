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

using Plexdata.Graylog.Simulator.Extensions;
using Plexdata.Graylog.Simulator.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plexdata.Graylog.Simulator.Queuing
{
    internal class ObservableQueue<TValue> : IObservableQueue<TValue>
    {
        #region Public Events

        public event EventHandler Enqueued;
        public event EventHandler Dequeued;

        #endregion

        #region Private Fields

        private volatile Int32 count = 0;
        private readonly Object interlock = null;
        private readonly Queue<TValue> queue = null;

        #endregion

        #region Construction

        public ObservableQueue()
            : this(16)
        {
        }

        public ObservableQueue(Int32 capacity)
            : base()
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            this.count = 0;
            this.interlock = new Object();
            this.queue = new Queue<TValue>(capacity);
        }

        #endregion

        #region Public Properties

        public Int32 Count
        {
            get
            {
                return this.count;
            }
        }

        public Boolean IsEmpty
        {
            get
            {
                return this.count == 0;
            }
        }

        #endregion

        #region Public Methods

        public void Enqueue(TValue value)
        {
            if (value != null)
            {
                lock (this.interlock)
                {
                    this.queue.Enqueue(value);
                    this.count = this.queue.Count;
                }

                this.RaiseEnqueued();
            }
        }

        public TValue Dequeue()
        {
            TValue value = default(TValue);

            if (!this.IsEmpty)
            {
                lock (this.interlock)
                {
                    value = this.queue.Dequeue();
                    this.count = this.queue.Count;
                }

                this.RaiseDequeued();
            }

            return value;
        }

        public TValue[] DequeueAll()
        {
            List<TValue> values = new List<TValue>();

            if (!this.IsEmpty)
            {
                lock (this.interlock)
                {
                    while (this.queue.Count > 0)
                    {
                        values.Add(this.queue.Dequeue());
                    }

                    this.count = this.queue.Count;
                }

                this.RaiseDequeued();
            }

            return values.ToArray();
        }

        public TValue Peek()
        {
            TValue value = default(TValue);

            if (!this.IsEmpty)
            {
                lock (this.interlock)
                {
                    value = this.queue.Peek();
                }
            }

            return value;
        }

        public void Clear()
        {
            lock (this.interlock)
            {
                this.queue.Clear();
                this.count = this.queue.Count;
            }
        }

        public void Trim()
        {
            lock (this.interlock)
            {
                this.queue.TrimExcess();
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void RaiseEnqueued()
        {
            try
            {
                if (this.Enqueued is null) { return; }

                Task.Factory
                    .StartNew(() => { this.Enqueued.Invoke(this, EventArgs.Empty); })
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.Error(exception);
            }
        }

        protected virtual void RaiseDequeued()
        {
            try
            {
                if (this.Dequeued is null) { return; }

                Task.Factory
                    .StartNew(() => { this.Dequeued.Invoke(this, EventArgs.Empty); })
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.Error(exception);
            }
        }

        #endregion
    }
}

