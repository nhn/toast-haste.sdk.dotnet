/*
* Copyright 2016 NHN Entertainment Corp.
*
* NHN Entertainment Corp. licenses this file to you under the Apache License,
* version 2.0 (the "License"); you may not use this file except in compliance
* with the License. You may obtain a copy of the License at:
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using Haste.ByteBuffer;
using Haste.Data;
using System;

namespace Haste.Network
{
    internal class OutgoingCommand : IComparable<OutgoingCommand>, IDisposable
    {
        private short _headerLength;
        private IByteBuffer _buffer;
        private bool _disposed = false;

        internal OutgoingCommand(CommandType commandType, byte channel)
            : this(commandType, channel, null)
        {
        }

        internal OutgoingCommand(CommandType commandType, byte channel, byte[] payload)
            : this(commandType, channel, false, payload)
        {
        }

        internal OutgoingCommand(CommandType commandType, byte channel, bool encryted, byte[] payload)
        {
            ReliableSequenceNumber = UdpChannel.InitialSequenceNumber;
            UnreliableSequenceNumber = UdpChannel.InitialSequenceNumber;

            CommandType = commandType;
            
            Channel = channel;

            Payload = payload != null && payload.Length > 0 ? ByteBufferFactory.NewBuffer(payload) : ByteBufferFactory.NewBuffer();

            switch (CommandType)
            {
                case CommandType.Acknowledge:
                    _headerLength = (int)Lengths.AcknowledgeHeader;
                    Flags = CommandFlags.Unreliable;
                    break;

                case CommandType.Disconnect:
                    _headerLength = (int)Lengths.DisconnectHeader;
                    Flags = CommandFlags.Reliable;
                    break;

                case CommandType.Ping:
                    _headerLength = (int)Lengths.PingHeader;
                    Flags = CommandFlags.Reliable;
                    break;

                case CommandType.Reliable:
                    _headerLength = (int)Lengths.ReliableHeader;
                    Flags = encryted ? CommandFlags.Reliable | CommandFlags.Encrypted : CommandFlags.Reliable;
                    break;

                case CommandType.Unreliable:
                    _headerLength = (int)Lengths.UnreliableHeader;
                    Flags = encryted ? CommandFlags.Unreliable | CommandFlags.Encrypted : CommandFlags.Unreliable;
                    break;

                case CommandType.Fragmented:
                    _headerLength = (int)Lengths.FragmentHeader;
                    Flags = encryted ? CommandFlags.Reliable | CommandFlags.Encrypted : CommandFlags.Unreliable;
                    break;

                case CommandType.SNTP:
                    _headerLength = (int)Lengths.SntpHeader;
                    Flags = CommandFlags.Reliable;
                    break;

                default:
                    throw new ArgumentException(string.Format("Unknown CommandType: {0}", commandType));
            }

            Size = Payload.Count > 0 ? (short)(_headerLength + Payload.Count) : _headerLength;
        }

        internal byte Channel { get; private set; }

        internal CommandFlags Flags { get; private set; }

        internal bool IsEncryted
        {
            get
            {
                return (Flags & CommandFlags.Encrypted) == CommandFlags.Encrypted;
            }
        }

        internal bool IsReliable
        {
            get 
            {
                return (Flags & CommandFlags.Reliable) == CommandFlags.Reliable; 
            }
        }

        internal bool IsSequenceNumberNotAssigned
        {
            get { return ReliableSequenceNumber == UdpChannel.InitialSequenceNumber; }
        }

        internal IByteBuffer Payload { get; private set; }

        internal long ReliableSequenceNumber { get; set; }

        internal short Size { get; private set; }

        internal CommandType CommandType { get; private set; }

        internal long UnreliableSequenceNumber { get; set; }

        public int CompareTo(OutgoingCommand other)
        {
            if ((Flags & CommandFlags.Reliable) == CommandFlags.Reliable)
            {
                return (int)(ReliableSequenceNumber - other.ReliableSequenceNumber);
            }
            else
            {
                int num = (int)(ReliableSequenceNumber - other.ReliableSequenceNumber);
                return num != 0 ? num : (int)(UnreliableSequenceNumber - other.UnreliableSequenceNumber);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private byte[] Serialize()
        {
            if (_buffer != null)
                return _buffer.ToArray();

            _buffer = ByteBufferFactory.NewBuffer();

            _buffer.WriteByte((byte)CommandType);
            _buffer.WriteByte((byte)Channel);
            _buffer.WriteByte((byte)Flags);

            _buffer.WriteShort(Size);
            _buffer.WriteLong(ReliableSequenceNumber);

            switch (CommandType)
            {
                case CommandType.Unreliable:
                    _buffer.WriteLong(UnreliableSequenceNumber);
                    break;

                case CommandType.Fragmented:
                    _buffer.WriteLong(StartSequenceNumber);
                    _buffer.WriteShort(FragmentCount);
                    _buffer.WriteShort(FragmentNumber);
                    _buffer.WriteLong(TotalLength);
                    _buffer.WriteLong(FragmentOffset);
                    break;
            }

            if (Payload.Count > 0)
                _buffer.WriteBytes(Payload);

            return _buffer.ToArray();
        }

        /// <summary>
        /// Serialize to target buffer, and return new offset.
        /// </summary>
        /// <param name="targetBuffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal int Serialize(byte[] targetBuffer, int offset)
        {
            Buffer.BlockCopy(Serialize(), 0, targetBuffer, offset, Size);
            return offset + Size;
        }

        internal bool IsTimeout(uint currentTime, int sentCountAllowance)
        {
            if (currentTime - SentTime > RoundTripTimeout)
            {
                if (currentTime > SendingTimeout)
                    return true;

                if (SendAttempts < sentCountAllowance)
                    return false;
            }
            return true;
        }

        #region Fragments

        internal short FragmentCount { get; set; }

        internal short FragmentNumber { get; set; }

        internal long FragmentOffset { get; set; }

        internal long StartSequenceNumber { get; set; }

        internal long TotalLength { get; set; }

        #endregion Fragments

        #region Sending

        internal uint RoundTripTimeout { get; set; }

        internal uint RoundTripTimeoutLimit { get; set; }

        internal uint SendAttempts { get; set; }

        internal uint SendingTimeout { get; set; }

        internal uint SentTime { get; set; }

        #endregion Sending

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _disposed = true;
            }
        }
    }
}