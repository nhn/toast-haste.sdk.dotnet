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

using System.Net;
using Haste.Data;
using Haste.ByteBuffer;
using Haste.Security;

namespace Haste.Network
{
    class ConnectingBroker : INetworkBroker
    {
        private UdpPeer _peer;

        public int SentCount { get; set; }

        public int SeningTime { get; set; }

        private UdpSocket _udpSocket;

        public ConnectingBroker(UdpSocket udpSocket, UdpPeer peer)
        {
            SeningTime = (int)EnvironmentTimer.GetTickCount();

            _peer = peer;
            _udpSocket = udpSocket;
            SentCount = 0;
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return _peer.RemoteEndPoint; }
        }

        public void OnReceive(IByteBuffer buffer)
        {
            if (_udpSocket.State != SocketState.Connecting)
            {
                Log(LogLevel.Error, "Invalid Network Status {0} in connecting phase", _udpSocket.State);
                _udpSocket.Dispose();
                return;
            }

            CommandType type = (CommandType)buffer.ReadByte();

            if (type != CommandType.ConnectResponse)
            {
                Log(LogLevel.Error, "Received a invalid CommandType {0} in connecting phase", type);
                _udpSocket.Dispose();
                return;
            }

            int peerID       = buffer.ReadInt();   //4
            byte serverKeyLength = buffer.ReadByte();
            byte[] serverKey = new byte[serverKeyLength];
            buffer.ReadBytes(serverKey);
            int port         = buffer.ReadInt();   //4
            long sendingTime  = buffer.ReadLong();   //8
            long responseTime = buffer.ReadLong();   //8
            long crc          = buffer.ReadLong();   //8

            buffer.SeekWriteIndex(-(int)Lengths.Crc);
            buffer.WriteLong(0);

            long calc = buffer.CalculateCrc();

            if (crc != calc)
            {
                Log(LogLevel.Error, "Invalid crc recv[{0}] calc[{1}]", crc, calc);
                return;
            }

            _peer.Init(peerID, port, new BigInteger(serverKey), sendingTime, responseTime);
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            _peer.Log(level, format, args);
        }

        public byte[] CreateConnectCommand(
            short channelCount, 
            short mtu,
            int protoVerison,
            int disconnectionTimeout,
            bool isCrcEnabled,
            BigInteger publicKey, long sendingTime)
        {
            var buffer = ByteBufferFactory.NewBuffer();
            buffer.WriteByte((byte)CommandType.Connect);
            buffer.WriteInt(protoVerison);            //4
            buffer.WriteLong(sendingTime);             //8
            buffer.WriteShort(channelCount);            //2
            buffer.WriteShort(mtu);                     //2
            buffer.WriteInt(disconnectionTimeout);    //4
            buffer.WriteShort((short)(isCrcEnabled == true ? 1 : 0)); //2
            var publicKeyBytes = publicKey.ToByteArray();
            buffer.WriteByte((byte)publicKeyBytes.Length);
            buffer.WriteBytes(publicKeyBytes);   //4
            buffer.WriteLong(0);           //8 crc space

            var bytes = buffer.ToArray();

            if (isCrcEnabled)
            {
                long crc = bytes.CalculateCrc(bytes.Length);
                int offset = bytes.Length - (int)Lengths.Crc;
                ByteWrite.SetLong(bytes, ref offset, crc);
            }
            
            SentCount ++;

            return bytes;
        }
    }
}
