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

using Plexdata.Converters.Factories;
using Plexdata.Graylog.Simulator.Helpers;
using System;
using System.Linq;
using System.Text;

namespace Plexdata.Graylog.Simulator.Extensions
{
    internal static class DisplayPrintExtension
    {
        #region Private Fields

        private static Object synchronizer = new Object();

        #endregion

        #region Public Methods

        public static void Print(this Object caller, String message)
        {
            caller.Print(ColorSwitch.DefaultColor, message, null);
        }

        public static void Print(this Object caller, ConsoleColor color, String message)
        {
            caller.Print(color, message, null);
        }

        public static void Print(this Object caller, String format, params Object[] arguments)
        {
            caller.Print(ColorSwitch.DefaultColor, format, arguments);
        }

        public static void Print(this Object _, ConsoleColor color, String format, params Object[] arguments)
        {
            if (String.IsNullOrWhiteSpace(format))
            {
                return;
            }

            String message = format;

            if (arguments?.Any() ?? false)
            {
                message = String.Format(format, arguments);
            }

            lock (DisplayPrintExtension.synchronizer)
            {
                using (new ColorSwitch(color))
                {
                    Console.WriteLine(message);
                }
            }
        }

        public static void Debug(this Object caller, String message)
        {
            if (!Program.Settings.IsDebug) { return; }

            caller.Print(message);
        }

        public static void Debug(this Object caller, ConsoleColor color, String message)
        {
            if (!Program.Settings.IsDebug) { return; }

            caller.Print(color, message);
        }

        public static void Debug(this Object caller, String format, params Object[] arguments)
        {
            if (!Program.Settings.IsDebug) { return; }

            caller.Print(format, arguments);
        }

        public static void Debug(this Object caller, ConsoleColor color, String format, params Object[] arguments)
        {
            if (!Program.Settings.IsDebug) { return; }

            caller.Print(color, format, arguments);
        }

        public static void Dump(this Object caller, Byte[] request, String server, String family, String port)
        {
            if (!Program.Settings.IsDebug) { return; }
            if (!request?.Any() ?? true) { return; }
            if (String.IsNullOrEmpty(server)) { return; }
            if (String.IsNullOrEmpty(family)) { return; }
            if (String.IsNullOrEmpty(port)) { return; }

            Int32 length = Math.Min(128, request.Length);

            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("Received {0} bytes of data at {1} server for {2} on port {3}.", request.Length, server, family, port);
            builder.AppendLine();
            builder.AppendFormat("Dump of first {0} bytes of received data.", length);
            builder.AppendLine();
            builder.Append(BinConverterFactory.CreateConverter().Convert(request, length));

            caller.Debug(ColorSwitch.DumpColor, builder.ToString());
        }

        public static void Error(this Object caller, Exception error)
        {
            caller.Error(ColorSwitch.ErrorColor, error, true);
        }

        public static void Error(this Object caller, Exception error, Boolean brief)
        {
            caller.Error(ColorSwitch.ErrorColor, error, brief);
        }

        public static void Error(this Object caller, ConsoleColor color, Exception error, Boolean brief)
        {
            if (error is null)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine(error);

            if (brief)
            {
                caller.Print(color, "Error: {0}", error.Message);
            }
            else
            {
                caller.Print(color, error.ToString(), null);
            }
        }

        public static void WaitForAnyKeyPress(this Object _)
        {
            using (new ColorSwitch(ColorSwitch.DefaultColor))
            {
                Console.Write("Hit any key to continue... ");
                Console.ReadKey();
                Console.WriteLine();
            }
        }

        #endregion
    }
}
