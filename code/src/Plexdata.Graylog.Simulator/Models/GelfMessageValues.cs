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
using System.Collections.Generic;

namespace Plexdata.Graylog.Simulator.Models
{
    internal class GelfMessageValues
    {
        #region Private Fields

        private const Char underscore = '_';
        private readonly List<GelfMessageValue> commonValues = null;
        private readonly List<GelfMessageValue> customValues = null;

        #endregion

        #region Construction

        public GelfMessageValues()
            : base()
        {
            this.commonValues = new List<GelfMessageValue>();
            this.customValues = new List<GelfMessageValue>();
        }

        #endregion

        #region Public Properties

        public IEnumerable<GelfMessageValue> CommonValues
        {
            get
            {
                return this.commonValues;
            }
        }

        public IEnumerable<GelfMessageValue> CustomValues
        {
            get
            {
                return this.customValues;
            }
        }

        #endregion

        #region Public Methods

        public void Append(String name, Object value)
        {
            this.Append(name, value?.ToString());
        }

        public void Append(String name, String value)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentOutOfRangeException(nameof(name), $"Parameter \"{nameof(name)}\" cannot be null or whitespace.");
            }

            value = value is null ? "null" : value;

            if (name.StartsWith(GelfMessageValues.underscore))
            {
                this.customValues.Add(new GelfMessageValue(name.TrimStart(GelfMessageValues.underscore), value));
            }
            else
            {
                this.commonValues.Add(new GelfMessageValue(name, value));
            }
        }

        #endregion
    }
}
