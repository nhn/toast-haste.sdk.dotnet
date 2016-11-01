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
using System;

namespace Haste.Network
{
    internal class IncomingCommand : IComparable<IncomingCommand>
    {
        private byte[] _payload;

        public IncomingCommand(IByteBuffer buffer, uint localtime, uint servertime)
        {
            TimestampOfReceive = localtime;
            ServerSentTime = servertime;

            Type = (CommandType)buffer.ReadByte();
            Channel = buffer.ReadByte();

            CommandFlags flag = (CommandFlags)buffer.ReadByte();       //1
            IsEncrypted = (flag & CommandFlags.Encrypted) == CommandFlags.Encrypted;
            IsReliable = (flag & CommandFlags.Reliable) == CommandFlags.Reliable;

            CommandSize = (uint)buffer.ReadShort(); //4

            ReliableSequenceNumber = buffer.ReadLong();         //8

            byte[] payload = null;

            switch (Type)
            {
                case CommandType.Acknowledge:
                    {
                        AckReceivedReliableSequenceNumber = buffer.ReadLong();
                        long receivedSentTime = buffer.ReadLong();
                        AckReceivedSentTime = (uint)receivedSentTime;
                    }
                    break;
                case CommandType.Reliable:
                    {
                        payload = new byte[CommandSize - (uint)Lengths.ReliableHeader];
                    }
                    break;

                case CommandType.Unreliable:
                    {
                        UnreliableSequenceNumber = buffer.ReadLong();           //8
                        payload = new byte[CommandSize - (uint)Lengths.UnreliableHeader];
                    }
                    break;

                case CommandType.Fragmented:
                    {
                        StartSequenceNumber = buffer.ReadLong();
                        FragmentCount = buffer.ReadShort();
                        FragmentNumber = buffer.ReadShort();
                        TotalLength = buffer.ReadInt();
                        FragmentOffset = buffer.ReadInt();

                        FragmentsRemaining = FragmentCount;

                        payload = new byte[CommandSize - (uint)Lengths.FragmentHeader];
                    }
                    break;

                case CommandType.Disconnect:
                    {
                        payload = new byte[CommandSize - (uint)Lengths.DisconnectHeader];
                    }
                    break;

                default:
                    break;
            }

            if (payload != null)
            {
                buffer.ReadBytes(payload, 0, payload.Length);
                _payload = payload;
            }
        }

        internal long AckReceivedReliableSequenceNumber { get; private set; }

        internal uint AckReceivedSentTime { get; private set; }

        internal byte Channel { get; private set; }

        internal uint CommandSize { get; private set; }

        internal short FragmentCount { get; private set; }

        internal short FragmentNumber { get; private set; }

        internal int FragmentOffset { get; private set; }

        internal short FragmentsRemaining { get; set; }

        internal bool IsEncrypted { get; private set; }

        internal bool IsReliable { get; private set; }

        internal long ReliableSequenceNumber { get; private set; }

        internal uint ServerSentTime { get; private set; }

        internal long StartSequenceNumber { get; set; }

        internal uint TimestampOfReceive { get; private set; }

        internal int TotalLength { get; private set; }

        internal CommandType Type { get; private set; }

        internal long UnreliableSequenceNumber { get; private set; }

        public int CompareTo(IncomingCommand other)
        {
            if (IsReliable)
            {
                return (int)(ReliableSequenceNumber - other.ReliableSequenceNumber);
            }
            else
            {
                int num = (int)(ReliableSequenceNumber - other.ReliableSequenceNumber);
                return num != 0 ? num : (int)(UnreliableSequenceNumber - other.UnreliableSequenceNumber);
            }
        }

        internal byte[] GetPayload()
        {
            return _payload;
        }

        internal bool IsPayloadAvailable()
        {
            return _payload != null && _payload.Length != 0;
        }

        internal void SetPayload(byte[] payload)
        {
            _payload = payload;
        }
    }
}