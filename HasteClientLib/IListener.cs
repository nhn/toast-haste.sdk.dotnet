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

using Haste.Messages;

namespace Haste
{
    public interface IListener
    {
        /// <summary>
        /// Call when a peer was terminated.
        /// </summary>
        void OnClose();

        /// <summary>
        /// Called when a peer received a event message.
        /// </summary>
        /// <param name="eventData"></param>
        void OnEventMessage(EventMessage eventData);

        /// <summary>
        /// Called when a peer received a response message.
        /// </summary>
        /// <param name="response"></param>
        void OnResponseMessage(ResponseMessage response);

        /// <summary>
        /// Called when a peer changed status.
        /// </summary>
        void OnStatusChanged(StatusCode statusCode, string message);
    }
}