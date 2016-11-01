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

using System.Text;

namespace Haste
{
    internal static class Util
    {
        internal static string ToString(byte[] bs)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bs.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(",");
                }
                builder.Append((sbyte)(0xFF & bs[i]));
            }
            return builder.ToString();
        }
    }
}
