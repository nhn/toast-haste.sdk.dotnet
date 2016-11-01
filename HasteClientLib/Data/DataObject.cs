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

using System.Collections;
using System.Collections.Generic;

namespace Haste.Data
{
    public class DataObject : IEnumerable
    {
        private readonly Dictionary<byte, DataWrapper> _container;

        public DataObject() : this(0)
        {
        }

        public DataObject(int capacity)
        {
            _container = new Dictionary<byte, DataWrapper>(capacity);
        }

        public int Count
        {
            get { return _container.Count; }
        }

        public IDictionary<byte, DataWrapper> Container
        {
            get { return _container; }
        }

        public bool ContainsKey(byte key)
        {
            return _container.ContainsKey(key);
        }

        public IEnumerator GetEnumerator()
        {
            return _container.GetEnumerator();
        }

        public bool GetValue<T>(byte key, out T value)
        {
            if (ContainsKey(key))
            {
                value = (T)_container[key].Value;
                return true;
            }

            value = default(T);
            return false;
        }

        public T GetValue<T>(byte key)
        {
            return (T)_container[key].Value;
        }

        public void SetBoolean(byte key, bool value)
        {
            _container[key] = new DataWrapper(DataType.Bool, value);
        }

        public void SetBooleans(byte key, bool[] value)
        {
            _container[key] = new DataWrapper(DataType.BoolArray, value);
        }

        public void SetByte(byte key, byte value)
        {
            _container[key] = new DataWrapper(DataType.Byte, value);
        }

        public void SetBytes(byte key, byte[] value)
        {
            _container[key] = new DataWrapper(DataType.ByteArray, value);
        }

        public void SetDataObject(byte key, DataObject value)
        {
            _container[key] = new DataWrapper(DataType.DataObject, value);
        }

        public void SetDouble(byte key, double value)
        {
            _container[key] = new DataWrapper(DataType.Double, value);
        }

        public void SetDoubles(byte key, double[] value)
        {
            _container[key] = new DataWrapper(DataType.DoubleArray, value);
        }

        public void SetFloat(byte key, float value)
        {
            _container[key] = new DataWrapper(DataType.Float, value);
        }

        public void SetFloats(byte key, float[] value)
        {
            _container[key] = new DataWrapper(DataType.FloatArray, value);
        }

        public void SetInt16(byte key, short value)
        {
            _container[key] = new DataWrapper(DataType.Int16, value);
        }

        public void SetInt16s(byte key, short[] value)
        {
            _container[key] = new DataWrapper(DataType.Int16Array, value);
        }

        public void SetInt32(byte key, int value)
        {
            _container[key] = new DataWrapper(DataType.Int32, value);
        }

        public void SetInt32s(byte key, int[] value)
        {
            _container[key] = new DataWrapper(DataType.Int32Array, value);
        }

        public void SetInt64(byte key, long value)
        {
            _container[key] = new DataWrapper(DataType.Int64, value);
        }

        public void SetInt64s(byte key, long[] value)
        {
            _container[key] = new DataWrapper(DataType.Int64Array, value);
        }

        public void SetString(byte key, string value)
        {
            _container[key] = new DataWrapper(DataType.String, value);
        }

        public void SetStrings(byte key, string[] value)
        {
            _container[key] = new DataWrapper(DataType.StringArray, value);
        }
    }
}