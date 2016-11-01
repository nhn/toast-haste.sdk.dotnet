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

using System.Threading;
using Haste.Network;

namespace Haste
{
    public class NetStatistics
    {
        private int _crcErrorCount = 0;
        private int _highestRoundTripTimeVariance = 0;
        private int _incomingCommands = 0;

        private long _incomingBytes = 0;
        private int _incomingMtu = 0;

        private int _roundTripTime = 0;
        private int _lowestRoundTripTime = 0;
        private long _outgoingBytes = 0;
        private int _outgoingCommands = 0;
        private int _outgoingMtu = 0;
        private int _reliableCommandsRepeated = 0;
        private int _timestampOfLastAckReceived = 0;
        private int _timestampOfLastReceive = 0;
        private int _timestampOfLastServerSentTime = 0;
        private int _lastReceiveBytes = 0;

        public int CrcErrorCount { get { return _crcErrorCount; } }
        public int HighestRoundTripTimeVariance { get { return _highestRoundTripTimeVariance; } }
        public int IncomingCommands { get { return _incomingCommands; } }
        public int IncomingMtu { get { return _incomingMtu; } }
        public int RoundTripTime { get { return _roundTripTime; } }
        public int LowestRoundTripTime { get { return _lowestRoundTripTime; } }
        public long OutgoingBytes { get { return _outgoingBytes; } }
        public int OutgoingCommands { get { return _outgoingCommands; } }
        public int OutgoingMtu { get { return _outgoingMtu; } }
        public int ReliableCommandsRepeated { get { return _reliableCommandsRepeated; } }
        public int TimestampOfLastAckReceived { get { return _timestampOfLastAckReceived; } }
        public int TimestampOfLastReceive { get { return _timestampOfLastReceive; } }
        public int TimestampOfLastServerSentTime { get { return _timestampOfLastServerSentTime; } }
        public int LastReceiveBytes { get { return _lastReceiveBytes; } }

        internal void UpdateRoundTripTime(RoundTripTime roundTripTime)
        {
            Interlocked.Exchange(ref _roundTripTime, roundTripTime.MeanOfRoundTripTime);
            Interlocked.Exchange(ref _lowestRoundTripTime, roundTripTime.LowestRoundTripTime);
            Interlocked.Exchange(ref _highestRoundTripTimeVariance, roundTripTime.HighestRoundTripTimeVariance);
        }

        internal void ReceiveAck(uint timestampOfReceive)
        {
            Interlocked.Exchange(ref _timestampOfLastAckReceived, (int)timestampOfReceive);
        }

        internal void ReceiveBytes(int receiveBytes, uint receiveTime)
        {
            Interlocked.Exchange(ref _lastReceiveBytes, receiveBytes);
            Interlocked.Exchange(ref _timestampOfLastReceive, (int)receiveTime);
            Interlocked.Add(ref _incomingBytes, receiveBytes);
        }

        internal void ErrorCrc()
        {
            Interlocked.Increment(ref _crcErrorCount);
        }

        internal void ReceiveMtu(uint serverSentTime)
        {
            Interlocked.Increment(ref _incomingMtu);
            Interlocked.Exchange(ref _timestampOfLastServerSentTime, (int)serverSentTime);
        }

        internal void ReceiveIncomingCommand(int count)
        {
            Interlocked.Add(ref _incomingCommands, count);
        }

        internal void ResendCommand()
        {
            Interlocked.Increment(ref _reliableCommandsRepeated);
        }
    }
}