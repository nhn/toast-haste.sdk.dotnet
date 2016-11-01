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
using System.Text;

namespace Haste.Data
{
    internal static partial class DataSerializer
    {
        public static void WriteDataObject(this IByteBuffer buffer, DataObject obj)
        {
            int count = (obj == null) ? 0 : obj.Container.Count;

            if (obj != null && obj.Container.Count > int.MaxValue)
            {
                throw new ArgumentException(string.Format("The number of elements in SendingData should be less than {0}", MaxLength));
            }

            buffer.WriteInt(count);

            if (obj != null)
            {
                foreach (var elem in obj.Container)
                {
                    WriteData(buffer, elem.Key, elem.Value);
                }
            }
        }
        
        private static void WriteData(IByteBuffer buffer, byte key, DataWrapper wrapper)
        {
            buffer.WriteByte(key);
            buffer.WriteByte((byte)wrapper.Type);

            switch (wrapper.Type)
            {
                case DataType.None:
                    break;

                case DataType.Byte:
                    {
                        buffer.WriteByte((byte)wrapper.Value);
                    }
                    break;

                case DataType.Bool:
                    {
                        bool value = (bool)wrapper.Value;
                        buffer.WriteByte(value ? (byte)1 : (byte)0);
                    }
                    break;

                case DataType.Int16:
                    {
                        buffer.WriteShort((short)wrapper.Value);
                    }
                    break;

                case DataType.Int32:
                    {
                        buffer.WriteInt((int)wrapper.Value);
                    }
                    break;

                case DataType.Int64:
                    {
                        buffer.WriteLong((long)wrapper.Value);
                    }
                    break;

                case DataType.Float:
                    {
                        buffer.WriteFloat((float)wrapper.Value);
                    }
                    break;

                case DataType.Double:
                    {
                        buffer.WriteDouble((double)wrapper.Value);
                    }
                    break;

                case DataType.String:
                    {
                        string value = (string)wrapper.Value;
                        byte[] bytes = Encoding.UTF8.GetBytes(value);

                        buffer.WriteInt(bytes.Length);
                        buffer.WriteBytes(bytes);
                    }
                    break;

                case DataType.ByteArray:
                    {
                        byte[] values = wrapper.Value as byte[];
                        buffer.WriteInt(values.Length);
                        buffer.WriteBytes(values, 0, values.Length);
                    }
                    break;

                case DataType.BoolArray:
                    {
                        bool[] values = wrapper.Value as bool[];
                        buffer.WriteInt(values.Length);
                        foreach (var elem in values)
                        {
                            buffer.WriteByte(elem ? (byte)1 : (byte)0);
                        }
                    }
                    break;

                case DataType.Int16Array:
                    {
                        short[] values = wrapper.Value as short[];
                        buffer.WriteInt(values.Length);
                        foreach (var elem in values)
                        {
                            buffer.WriteShort(elem);
                        }
                    }
                    break;

                case DataType.Int32Array:
                    {
                        int[] values = wrapper.Value as int[];
                        buffer.WriteInt(values.Length);
                        foreach (var elem in values)
                        {
                            buffer.WriteInt(elem);
                        }
                    }
                    break;

                case DataType.Int64Array:
                    {
                        long[] values = wrapper.Value as long[];
                        buffer.WriteInt(values.Length);
                        foreach (var elem in values)
                        {
                            buffer.WriteLong(elem);
                        }
                    }
                    break;

                case DataType.FloatArray:
                    {
                        float[] values = wrapper.Value as float[];
                        buffer.WriteInt(values.Length);
                        foreach (var elem in values)
                        {
                            buffer.WriteFloat(elem);
                        }
                    }
                    break;

                case DataType.DoubleArray:
                    {
                        double[] values = wrapper.Value as double[];
                        buffer.WriteInt(values.Length);
                        foreach (var elem in values)
                        {
                            buffer.WriteDouble(elem);
                        }
                    }
                    break;

                case DataType.StringArray:
                    {
                        string[] values = wrapper.Value as string[];

                        buffer.WriteInt(values.Length);

                        foreach (var elem in values)
                        {
                            byte[] bytes = Encoding.UTF8.GetBytes(elem);
                            buffer.WriteInt(bytes.Length);
                            buffer.WriteBytes(bytes, 0, bytes.Length);
                        }
                    }
                    break;

                case DataType.DataObject:
                    {
                        WriteDataObject(buffer, wrapper.Value as DataObject);
                    }
                    break;

                default:
                    throw new Exception("Unknown Error occurs");
            }
        }
    }
}