using Networking;
using NUnit.Framework;
using System;
using System.Text;
using UnityEngine;

namespace Tests {
    public class ByteHelperTests {
        [Test]
        public void ShouldMergeAndSplitCorrectly() {
            string inputA = "abc";
            Guid inputB = Guid.NewGuid();

            byte[] merged = Bytes.SplittablePack(Encoding.ASCII.GetBytes(inputA), inputB.ToByteArray());
            byte[][] splits = Bytes.Split(merged);

            Assert.AreEqual(splits.Length, 2);
            Assert.AreEqual(Encoding.ASCII.GetString(splits[0]), inputA);
            Assert.AreEqual(new Guid(splits[1]), inputB);
        }

        [Test]
        public void ShouldMergeAndSplitCorrectlyForEmptyArrays() {
            byte[] merged = Bytes.SplittablePack(new byte[0], new byte[0]);
            byte[][] splits = Bytes.Split(merged);
            Assert.AreEqual(splits.Length, 2);
        }

        [Test]
        public void ShouldPackCorrectly() {
            Assert.AreEqual(Bytes.Pack(new byte[1] { 3 }, new byte[3] { 5, 7, 4 }), new byte[4] { 3, 5, 7, 4 });
        }

        [Test]
        public void ShouldBytifyAStringCorrectly() {
            Assert.AreEqual(Bytes.GetString(Bytes.Pack(new byte[2], Bytes.Of("abc"), new byte[2]), out int count1, 2), "abc");
            Assert.AreEqual(7, count1);
        }

        [Test]
        public void ShouldBytifyANonAsciiStringCorrectly() {
            Assert.AreEqual(Bytes.GetString(Bytes.Of("✂️"), out int count2), "✂️");
            Assert.AreEqual(10, count2);
        }

        [Test]
        public void ShouldBytifyARawStringCorrectly() {
            Assert.AreEqual(Bytes.GetRawString(Bytes.Of("abc", Bytes.StringMode.RAW)), "abc");
            Assert.AreEqual(Bytes.GetRawString(Bytes.Of("✂️", Bytes.StringMode.RAW)), "✂️");
        }

        [Test]
        public void ShouldBytifyAGuidCorrectly() {
            Guid guid = Guid.NewGuid();
            byte[] guidBytes = Bytes.Of(guid);
            Assert.AreEqual(guidBytes.Length, 16);
            Assert.AreEqual(Bytes.GetGuid(guidBytes), guid);
        }

        [Test]
        public void ShouldBytifyAGuidArrayCorrectly() {
            Guid[] guidArray = new[] { Guid.NewGuid(), Guid.NewGuid() };
            byte[] guidArrayBytes = Bytes.Of(guidArray);
            Assert.AreEqual(guidArrayBytes.Length, guidArray.Length * 16 + 4);
            Assert.AreEqual(Bytes.GetGuidArray(guidArrayBytes), guidArray);
        }

        [Test]
        public void ShouldBytifyAVector2Correctly() {
            Assert.AreEqual(Bytes.GetVector2(Bytes.Of(new Vector2(3.14f, 8.12f))), new Vector2(3.14f, 8.12f));
        }

        [Test]
        public void ShouldBytifyAVector3Correctly() {
            Assert.AreEqual(Bytes.GetVector3(Bytes.Of(new Vector3(3.14f, 8.12f, 123f))), new Vector3(3.14f, 8.12f, 123f));
        }

        [Test]
        public void ShouldBytifyAQuaternionCorrectly() {
            Assert.AreEqual(Bytes.GetQuaternion(Bytes.Of(new Quaternion(3.14f, 8.12f, 123f, -123f))), new Quaternion(3.14f, 8.12f, 123f, -123f));
        }
    }
}
