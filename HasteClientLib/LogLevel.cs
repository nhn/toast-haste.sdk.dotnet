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

namespace Haste
{
    /// <summary>
    /// Control how verbose the network log messages are.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Show log messages with priority Developer and higher, this it the most verbose setting.
        /// </summary>
        Developer,
        /// <summary>
        /// Show log messages with priority Debug and higher.
        /// </summary>
        Debug,
        /// <summary>
        /// Show log messages with priority Info and higher. This is the default setting.
        /// </summary>
        Info,
        /// <summary>
        /// Show log messages with priority Warning and higher.
        /// </summary>
        Warn,
        /// <summary>
        /// Show log messages with priority Error and higher.
        /// </summary>
        Error,
        /// <summary>
        /// Show log messages with priority Fatal and higher. this is the least verbose setting.
        /// </summary>
        Fatal
    }
}
