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

using System;

namespace Plexdata.Graylog.Simulator.Helpers
{
    internal class ColorSwitch : IDisposable
    {
        #region Public Fields

        public static readonly ConsoleColor DefaultColor = ConsoleColor.Gray;
        public static readonly ConsoleColor TraceColor = ConsoleColor.DarkGray;
        public static readonly ConsoleColor AccentColor = ConsoleColor.Cyan;
        public static readonly ConsoleColor AttentionColor = ConsoleColor.Yellow;
        public static readonly ConsoleColor WarningColor = ConsoleColor.DarkYellow;
        public static readonly ConsoleColor ErrorColor = ConsoleColor.DarkRed;
        public static readonly ConsoleColor DumpColor = ConsoleColor.DarkCyan;

        #endregion

        #region Private Fields

        private readonly ConsoleColor original;

        #endregion

        #region Construction

        public ColorSwitch(ConsoleColor foreground)
            : this(foreground, ColorSwitch.DefaultColor)
        {
        }

        public ColorSwitch(ConsoleColor foreground, ConsoleColor original)
            : base()
        {
            this.original = original;
            this.SetColor(foreground);
        }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            this.SetColor(this.original);
        }

        #endregion

        #region Private Methods

        private void SetColor(ConsoleColor color)
        {
            try { Console.ForegroundColor = color; } catch { }
        }

        #endregion
    }
}
