using System.Buffers;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Trackman.PerfectEngine.Tests.Editor
{
    public class JsonUtilityTest
    {
        #region Containers
        public struct TestStruct
        {
            public string TestString;
            public int TestInt;
        }
        #endregion

        #region Fields
        TestStruct testStruct = new() { TestString = "John", TestInt = 30 };
        string serializedTestStruct = "{\"TestString\":\"John\",\"TestInt\":30}";
        #endregion

        #region Methods
        [Test]
        public void ToJsonAllocByteArrayTest()
        {
            byte[] buffer = JsonUtility.ToJsonAlloc(testStruct);

            using MemoryStream memoryStream = new MemoryStream(buffer);
            using StreamReader streamReader = new StreamReader(memoryStream);
            string actualJson = streamReader.ReadToEnd();

            Assert.AreEqual(serializedTestStruct, actualJson);
        }
        [Test]
        public void ToJsonStringTest()
        {
            string actualJson = JsonUtility.ToJson(testStruct, false, false);

            Assert.AreEqual(serializedTestStruct, actualJson);
        }
        [Test]
        public void TestFromJsonByteArray()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(serializedTestStruct);
            TestStruct result = JsonUtility.FromJson<TestStruct>(bytes);
            Assert.AreEqual(testStruct.TestString, result.TestString);
            Assert.AreEqual(testStruct.TestInt, result.TestInt);
        }
        [Test]
        public void TestFromJsonByteArrayWithType()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(serializedTestStruct);
            TestStruct result = (TestStruct)JsonUtility.FromJson(bytes, typeof(TestStruct));
            Assert.AreEqual(testStruct.TestString, result.TestString);
            Assert.AreEqual(testStruct.TestInt, result.TestInt);
        }
        [Test]
        public void TestFromJsonStream()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(serializedTestStruct);
            using MemoryStream stream = new MemoryStream(bytes);
            TestStruct result = (TestStruct)JsonUtility.FromJson(stream, typeof(TestStruct));
            Assert.AreEqual(testStruct.TestString, result.TestString);
            Assert.AreEqual(testStruct.TestInt, result.TestInt);
        }
        [Test]
        public void TestFromJsonString()
        {
            TestStruct result = (TestStruct)JsonUtility.FromJson(serializedTestStruct, typeof(TestStruct));
            Assert.AreEqual(testStruct.TestString, result.TestString);
            Assert.AreEqual(testStruct.TestInt, result.TestInt);
        }
        [Test]
        public void TestFromJsonStringWithUseArrayPool()
        {
            TestStruct result = (TestStruct)JsonUtility.FromJson(serializedTestStruct, typeof(TestStruct), true);
            Assert.AreEqual(testStruct.TestString, result.TestString);
            Assert.AreEqual(testStruct.TestInt, result.TestInt);
        }
        [Test]
        public void TestFromJsonStringWithTypeT()
        {
            TestStruct result = JsonUtility.FromJson<TestStruct>(serializedTestStruct);
            Assert.AreEqual(testStruct.TestString, result.TestString);
            Assert.AreEqual(testStruct.TestInt, result.TestInt);
        }
        [Test]
        public void TestFromJsonStringWithTypeTAndUseArrayPool()
        {
            TestStruct result = JsonUtility.FromJson<TestStruct>(serializedTestStruct, true);
            Assert.AreEqual(testStruct.TestString, result.TestString);
            Assert.AreEqual(testStruct.TestInt, result.TestInt);
        } 
        #endregion
    }
}