using ByteBuffer;
using CocoonClientLibTest.Security;
using Xunit;
using Xunit.Abstractions;


namespace CocoonClientLibTest.DataObject
{
    using Cocoon.Data;
    using DataObject = Cocoon.Data.DataObject;
    public class DataObjectTest
    {
        private ITestOutputHelper _output ;
        public DataObjectTest(ITestOutputHelper helper)
        {
            _output = helper;
        }

        [Fact]
        public void DataObjectSerializeTest()
        {
            DataObject dataObject = new DataObject();
            dataObject.SetByte(0, 119);
            var buffer = ByteBufferAllocator.NewBuffer(Endian.BigEndian);
            DataSerializer.Write(buffer, dataObject);
            _output.WriteLine(TestUtil.ToString(buffer.ToArray()));
        }
    }
}
