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

namespace Haste.Data
{
    public enum DataType : byte
    {
        None = 0,
        Byte,
        Bool,
        Int16,
        Int32,
        Int64,
        Float,
        Double,
        String,
        
        ByteArray,
        BoolArray,
        Int16Array,
        Int32Array,
        Int64Array,
        FloatArray,
        DoubleArray,
        StringArray,
        
        DataObject,
    }

    public enum MessageType : byte
    {
        None = 0,
        InitialRequest,
        InitialResponse,
        RequestMessage,
        ResponseMessage,
        EventMessage,
    }

    public enum ReturnCode : short
    {
        //Negative Return Code
        UnknownError = short.MinValue,

        InvalidRequest,
        FailToConnectToAuthServer,
        FailToAutenticate,
        FailToCreateRoom,
        FailToJoinRoom,
        RoomNotExist,
        PeerAlreadyJoined,
        InvalidRecvGroup,
        FailToCacheData,
        RoomNotJoined,
        InvalidActorNr,

        //Positive Return Code
        Ok = 1,

        SuccessToCreateRoom,
        SuccessToJoinRoom,
    }
}