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
    public class Logger
    {
        /// <summary>
        /// The current logging level.
        /// </summary>
        public static LogLevel Current = LogLevel.Info;

        /// <summary>
        /// Checks if Developer logging is enabled.
        /// </summary>
        public static bool IsDevEnabled
        {
            get
            {
                return Current <= LogLevel.Developer;
            }
        }

        /// <summary>
        /// Checks if debug logging is enabled.
        /// </summary>
        public static bool IsDebugEnabled
        {
            get
            {
                return Current <= LogLevel.Debug;
            }
        }

        /// <summary>
        /// Checks if info level logging is enabled.
        /// </summary>
        public static bool IsInfoEnabled
        {
            get
            {
                return Current <= LogLevel.Info;
            }
        }

        /// <summary>
        /// Checks if wanring level logging is enabled.
        /// </summary>
        public static bool IsWarnEnabled
        {
            get
            {
                return Current <= LogLevel.Warn;
            }
        }

        /// <summary>
        /// Checks if error logging is enabled.
        /// </summary>
        public static bool IsErrorEnabled
        {
            get
            {
                return Current <= LogLevel.Error;
            }
        }

        /// <summary>
        /// Checks if fatal logging is enabled.
        /// </summary>
        public static bool IsFatalEnabled
        {
            get
            {
                return Current <= LogLevel.Fatal;
            }
        }
    }
}
