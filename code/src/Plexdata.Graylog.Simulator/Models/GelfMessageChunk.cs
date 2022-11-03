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
using System.IO;
using System.Linq;
using System.Net;

namespace Plexdata.Graylog.Simulator.Models
{
    internal class GelfMessageChunk
    {
        #region Construction

        private GelfMessageChunk()
            : base()
        {
            this.MagicNumber = 0;
            this.MessageId = 0;
            this.SequenceNumber = 0;
            this.TotalSequences = 0;
            this.ReceivedTimestamp = DateTime.UtcNow;
            this.MessagePayload = new Byte[0];
        }

        #endregion

        #region Public Properties

        public Int16 MagicNumber { get; private set; }

        public Int64 MessageId { get; private set; }

        public Int32 SequenceNumber { get; private set; }

        public Int32 TotalSequences { get; private set; }

        public DateTime ReceivedTimestamp { get; private set; }

        public Byte[] MessagePayload { get; private set; }

        #endregion

        #region Public Methods

        public static Boolean IsGelfChunkFormat(Byte[] buffer)
        {
            // GELF chunk messages start with magic bytes: 1E 0F
            return buffer != null && buffer.Length >= 2 && buffer[0] == 0x1E && buffer[1] == 0x0F;
        }

        public static Boolean IsGelfChunkHeader(Byte[] buffer)
        {
            // Minimum GELF chunk message header length: 12 bytes
            return buffer != null && buffer.Length >= 12 && IsGelfChunkFormat(buffer);
        }

        public static Boolean TryParse(Byte[] source, out GelfMessageChunk message)
        {
            message = new GelfMessageChunk();

            if (!GelfMessageChunk.IsGelfChunkHeader(source))
            {
                return false;
            }

            try
            {
                using (MemoryStream stream = new MemoryStream(source))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        message.MagicNumber = IPAddress.NetworkToHostOrder(reader.ReadInt16());
                        message.MessageId = IPAddress.NetworkToHostOrder(reader.ReadInt64());
                        message.SequenceNumber = reader.ReadByte();
                        message.TotalSequences = reader.ReadByte();
                        message.MessagePayload = reader.ReadBytes(source.Length - (Int32)stream.Position);
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                message.Error(exception);
                return false;
            }
        }

        public override String ToString()
        {
            return $"{nameof(this.MagicNumber)}: {this.MagicNumber:X4}; " +
                   $"{nameof(this.MessageId)}: {this.MessageId:X16}; " +
                   $"{nameof(this.SequenceNumber)}: {this.SequenceNumber}; " +
                   $"{nameof(this.TotalSequences)}: {this.TotalSequences}; " +
                   $"{nameof(this.ReceivedTimestamp)}: {this.ReceivedTimestamp:u}; " +
                   $"{nameof(this.MessagePayload)}: " +
                   $"[Length={this.MessagePayload.Length}; Data={String.Join(" ", this.MessagePayload.Take(10).Select(x => x.ToString("X2")))} ...]";
        }

        #endregion
    }
}
