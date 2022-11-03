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

using Newtonsoft.Json.Linq;
using Plexdata.Graylog.Simulator.Interfaces;
using Plexdata.Graylog.Simulator.Models;
using System;

namespace Plexdata.Graylog.Simulator.Parsers
{
    internal class GelfMessageParser : IGelfMessageParser
    {
        #region Private Fields

        private const Char underscore = '_';

        #endregion

        #region Construction

        public GelfMessageParser()
            : base()
        {
        }

        #endregion

        #region Public Methods

        public GelfMessageValues Parse(String message)
        {
            if (String.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentOutOfRangeException(nameof(message), $"Parameter \"{nameof(message)}\" cannot be null or whitespace.");
            }

            GelfMessageValues result = new GelfMessageValues();

            foreach (JToken token in JObject.Parse(message).Children())
            {
                if (token is JProperty property)
                {
                    if (property.Value is JValue value)
                    {
                        result.Append(property.Name, value.Value);
                    }
                }
            }

            return result;
        }

        public Boolean TryParse(String message, out GelfMessageValues result)
        {
            result = null;

            try
            {
                result = this.Parse(message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
