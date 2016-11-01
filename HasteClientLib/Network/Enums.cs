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

using System;

namespace Haste.Network
{
    [Flags]
    public enum CommandFlags : byte
    {
        Unreliable = 0x00,
        Reliable = 0x01,
        Encrypted = 0x02,
    }

    public enum CommandType : byte
    {
        None = 0,
        Acknowledge = 1,    
        Connect = 2,          
        ConnectResponse = 3, 
        Disconnect = 4,     
        Ping = 5,
        Reliable = 6,
        Unreliable = 7,
        Fragmented = 8,
        SNTP = 14,
    }

    public enum DisconnectReason : int
    {
        None              = 0,
        Timeout           = 0x01,
        ClientDisconnect  = 0x02,
        ServerUserLimit   = 0x03,
        ServerLogic       = 0x04,
        QueueFull         = 0x05,
        InvalidConnection = 0x06,
        InvalidDataFormat = 0x08,
        ApplicationStop   = 0x12,
        ConnectionFailed    = 0x13,
    }

    public enum Lengths : int
    {
        CommandType = 1,
        PeerId = 4,
        SentTime = 8,
        CommandCount = 2,
        Crc = 8,
        MtuHeader = CommandType + PeerId + SentTime + CommandCount + Crc,

        Type = 1,
        Channel = 1,
        Flags = 1,
        Size = 2,
        ReliableSequenceNumber = 8,
        UnreliableSequenceNumber = 8,

        StartSequenceNumber = 8,
        FragmentCount = 2,
        FragmentNumber = 2,
        TotalLength = 4,
        FragmentOffset = 4,

        AckReceivedReliableSequenceNumber = 8,
        AckReceivedSentTime = 8,

        MiniumHeader = Type + Channel + Flags + Size + ReliableSequenceNumber,
        ReliableHeader = MiniumHeader,
        
        ConnectHeader = ReliableHeader,
        VerifyConnectHeader = ReliableHeader,
        AcknowledgeHeader = MiniumHeader,

        PingHeader = ReliableHeader,
        DisconnectHeader = MiniumHeader,
        SntpHeader = ReliableHeader,
        UnreliableHeader = ReliableHeader + UnreliableSequenceNumber,
        FragmentHeader = ReliableHeader + StartSequenceNumber + FragmentCount + FragmentNumber + TotalLength + FragmentOffset,
    }

    public enum MtuSize : short
    {
        MinimumMtu = 476,
        DefaulTMtu = 1350,
        MaximumMtu = 3096,
    }

    public enum PeerID
    {
        MinimumPeerId = 0,
        MaximumPeerId = int.MaxValue
    }
}