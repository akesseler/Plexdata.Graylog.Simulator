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

using Plexdata.Graylog.Simulator.Interfaces;
using System;
using System.IO;
using System.IO.Compression;

namespace Plexdata.Graylog.Simulator.Helpers
{
    internal class GZipHelper : IGZipHelper
    {
        #region Public Methods

        public Byte[] Decompress(Byte[] source)
        {
            if (source is null)
            {
                return new Byte[0];
            }

            if (source.Length < 2)
            {
                return source;
            }

            // GZIP magic bytes: 1F 8B
            if (source[0] != 0x1F || source[1] != 0x8B)
            {
                return source;
            }

            const Int32 length = 4096;

            using (MemoryStream stream = new MemoryStream(source))
            {
                using (GZipStream zipper = new GZipStream(stream, CompressionMode.Decompress))
                {
                    Byte[] segment = new Byte[length];

                    using (MemoryStream result = new MemoryStream())
                    {
                        while (true)
                        {
                            Int32 count = zipper.Read(segment, 0, length);

                            if (count < 1) { break; }

                            result.Write(segment, 0, count);
                        }

                        return result.ToArray();
                    }
                }
            }
        }

        #endregion
    }
}
