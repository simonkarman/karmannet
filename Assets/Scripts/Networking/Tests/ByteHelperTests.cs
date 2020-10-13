using Networking;
using NUnit.Framework;
using System;
using System.Text;

namespace Tests {
    public class ByteHelperTests {
        [Test]
        public void ShouldMergeAndSplitEmptyInput() {
            byte[] merged = ByteHelper.Merge(new byte[0], new byte[0]);
            byte[][] splits = ByteHelper.Split(merged);
            Assert.AreEqual(splits.Length, 2);
        }
        
        [Test]
        public void ShouldJoinMultipleByteArrasAndSplitThemCorrectly() {
            string inputA = "abc";
            Guid inputB = Guid.NewGuid();

            byte[] merged = ByteHelper.Merge(Encoding.ASCII.GetBytes(inputA), inputB.ToByteArray());
            byte[][] splits = ByteHelper.Split(merged);

            Assert.AreEqual(splits.Length, 2);
            Assert.AreEqual(Encoding.ASCII.GetString(splits[0]), inputA);
            Assert.AreEqual(new Guid(splits[1]), inputB);
        }
    }
}
