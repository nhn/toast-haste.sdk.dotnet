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
        private static DataWrapper ReadData(IByteBuffer buffer)
        {
            DataWrapper wrapper = new DataWrapper();
            wrapper.Type = (DataType)buffer.ReadByte();

            switch (wrapper.Type)
            {
                case DataType.None:
                    break;

                case DataType.Byte:
                    {
                        wrapper.Value = buffer.ReadByte();
                    }
                    break;

                case DataType.Bool:
                    {
                        byte value = buffer.ReadByte();
                        wrapper.Value = value > 0 ? true : false;
                    }
                    break;

                case DataType.Int16:
                    {
                        wrapper.Value = buffer.ReadShort();
                    }
                    break;

                case DataType.Int32:
                    {
                        wrapper.Value = buffer.ReadInt();
                    }
                    break;

                case DataType.Int64:
                    {
                        wrapper.Value = buffer.ReadLong();
                    }
                    break;

                case DataType.Float:
                    {
                        wrapper.Value = buffer.ReadFloat();
                    }
                    break;

                case DataType.Double:
                    {
                        wrapper.Value = buffer.ReadDouble();
                    }
                    break;

                case DataType.String:
                    {
                        int len = buffer.ReadInt();

                        var bytes = new byte[len];
                        buffer.ReadBytes(bytes, 0, len);
                        wrapper.Value = Encoding.UTF8.GetString(bytes, 0, len);
                    }
                    break;

                case DataType.ByteArray:
                    {
                        int len = buffer.ReadInt();

                        var bytes = new byte[len];
                        buffer.ReadBytes(bytes, 0, len);
                        wrapper.Value = bytes;
                    }
                    break;

                case DataType.BoolArray:
                    {
                        int len = buffer.ReadInt();

                        var bytes = new byte[len];
                        buffer.ReadBytes(bytes, 0, len);

                        bool[] values = new bool[len];

                        for (int i = 0; i < len; i++)
                        {
                            values[i] = bytes[i] > 0 ? true : false;
                        }

                        wrapper.Value = values;
                    }
                    break;

                case DataType.Int16Array:
                    {
                        int len = buffer.ReadInt();

                        short[] values = new short[len];

                        for (int i = 0; i < len; i++)
                        {
                            values[i] = buffer.ReadShort();
                        }

                        wrapper.Value = values;
                    }
                    break;

                case DataType.Int32Array:
                    {
                        int len = buffer.ReadInt();

                        int[] values = new int[len];

                        for (int i = 0; i < len; i++)
                        {
                            values[i] = buffer.ReadInt();
                        }

                        wrapper.Value = values;
                    }
                    break;

                case DataType.Int64Array:
                    {
                        int len = buffer.ReadInt();

                        long[] values = new long[len];

                        for (int i = 0; i < len; i++)
                        {
                            values[i] = buffer.ReadLong();
                        }

                        wrapper.Value = values;
                    }
                    break;

                case DataType.FloatArray:
                    {
                        int len = buffer.ReadInt();

                        float[] values = new float[len];

                        for (int i = 0; i < len; i++)
                        {
                            values[i] = buffer.ReadFloat();
                        }

                        wrapper.Value = values;
                    }
                    break;

                case DataType.DoubleArray:
                    {
                        int len = buffer.ReadInt();

                        double[] values = new double[len];

                        for (int i = 0; i < len; i++)
                        {
                            values[i] = buffer.ReadDouble();
                        }

                        wrapper.Value = values;
                    }
                    break;

                case DataType.StringArray:
                    {
                        int len = buffer.ReadInt();

                        string[] strs = new string[len];

                        for (int i = 0; i < len; i++)
                        {
                            int strLen = buffer.ReadInt();

                            byte[] strbytes = new byte[strLen];
                            buffer.ReadBytes(strbytes, 0, strLen);
                            strs[i] = Encoding.UTF8.GetString(strbytes, 0, strLen);
                        }

                        wrapper.Value = strs;
                    }
                    break;

                case DataType.DataObject:
                    wrapper.Value = ReadDataObject(buffer);
                    break;

                default:
                    throw new Exception("Invalid Data Type");
            }

            return wrapper;
        }

        private static DataObject ReadDataObject(IByteBuffer buffer)
        {
            DataObject obj = new DataObject();

            int count = buffer.ReadInt();

            for (int i = 0; i < count; i++)
            {
                byte key = buffer.ReadByte();
                obj.Container[key] = ReadData(buffer);
            }

            return obj;
        }
    }
}