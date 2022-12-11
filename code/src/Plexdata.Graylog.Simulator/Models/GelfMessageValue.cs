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
using System.Globalization;

namespace Plexdata.Graylog.Simulator.Models
{
    internal class GelfMessageValue
    {
        #region Private Fields

        private Boolean plain = false;

        #endregion

        #region Construction

        public GelfMessageValue(String name, String value)
            : this(name, value, false)
        {
        }

        public GelfMessageValue(String name, String value, Boolean plain)
            : base()
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentOutOfRangeException(nameof(name), $"Parameter \"{nameof(name)}\" cannot be null or whitespace.");
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"Parameter \"{nameof(value)}\" cannot be null or whitespace.");
            }

            this.plain = plain;
            this.Name = name;
            this.Value = value;
        }

        #endregion

        #region Public Properties

        public String Name { get; private set; }

        public String Value { get; private set; }

        #endregion

        #region Public Methods

        public override String ToString()
        {
            if (this.plain)
            {
                String value = this.Value;

                if (this.Name == "level")
                {
                    value = this.MapLevel(value);
                }

                if (this.Name == "timestamp")
                {
                    value = this.MapTimestamp(value);
                }

                return $"{this.Name} = {value}";
            }

            return $"{this.Name} = '{this.Value}'";
        }

        #endregion
        
        #region Private Methods

        private String MapLevel(String value)
        {
            switch (value)
            {
                case "0": return "0 (Emergency)";
                case "1": return "1 (Alert)";
                case "2": return "2 (Critical)";
                case "3": return "3 (Error)";
                case "4": return "4 (Warning)";
                case "5": return "5 (Notice)";
                case "6": return "6 (Informational)";
                case "7": return "7 (Debug)";
                default: return value;
            }
        }

        private String MapTimestamp(String value)
        {
            try
            {
                Decimal timestamp = Decimal.Parse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo);

                Decimal integral = Math.Truncate(timestamp);
                Decimal fraction = (timestamp - integral) * 1000.0M;

                Decimal milliseconds = (integral * 1000.0M) + fraction;

                DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(milliseconds));

                return $"{value} ({offset:yyyy-MM-dd HH:mm:ss.fff} UTC)";
            }
            catch
            {
                return value;
            }
        }

        #endregion
    }
}
