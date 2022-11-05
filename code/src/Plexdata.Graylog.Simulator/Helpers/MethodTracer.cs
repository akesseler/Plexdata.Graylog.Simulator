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
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Plexdata.Graylog.Simulator.Helpers
{
    internal sealed class MethodTracer : IDisposable
    {
        #region Private Fields

        private const String incoming = ">>>";
        private const String outgoing = "<<<";

        private readonly String caller = null;
        private readonly Stopwatch stopwatch = null;

        #endregion

        #region Construction

        private MethodTracer(Type caller, String member)
            : base()
        {
            this.caller = MethodTracer.GetCaller(caller, member);
            this.stopwatch = Stopwatch.StartNew();

            this.PrintMessage(MethodTracer.incoming);
        }

        #endregion

        #region Public Methods

        public static MethodTracer Begin<TType>([CallerMemberName] String member = null)
        {
            return new MethodTracer(typeof(TType), member);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Methods

        private void Dispose(Boolean disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (this.stopwatch.IsRunning)
            {
                this.stopwatch.Stop();
                this.PrintMessage(MethodTracer.outgoing, this.stopwatch.Elapsed);
            }
        }

        private static String GetCaller(Type caller, String member)
        {
            if (String.IsNullOrWhiteSpace(member))
            {
                member = "Unknown";
            }

            return $"{(caller is null ? member : $"{caller.Name}::{member}")}({Thread.CurrentThread.ManagedThreadId})";
        }

        private void PrintMessage(String direction)
        {
            this.PrintMessage(direction, null);
        }

        private void PrintMessage(String direction, TimeSpan? elapsed = null)
        {
            if (!Program.Settings.IsTrace) { return; }

            String message = elapsed is null
                ? String.Format("{0} {1}", direction, this.caller)
                : String.Format("{0} {1} {2}", direction, this.caller, elapsed);

            this.Print(ColorSwitch.TraceColor, message);
        }

        #endregion
    }
}
