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
    internal class TcpSocket : IDisposable
    {
        private TcpPeer _peer;

        private int _state;

        internal TcpSocket(TcpPeer peer)
        {
        }

        internal bool Connect()
        {
            throw new NotImplementedException();
        }

        internal bool Disconnect()
        {
            throw new NotImplementedException();
        }

        internal bool Send(byte[] data, int length)
        {
            throw new NotImplementedException();
        }

        protected void ReceiveProc()
        {
            throw new NotImplementedException();
        }

        #region Dispose Pattern

        private bool _disposed = false;

        public void Dispose()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        #endregion Dispose Pattern
    }
}