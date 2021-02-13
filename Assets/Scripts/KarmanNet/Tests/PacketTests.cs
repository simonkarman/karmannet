using KarmanNet.Networking;
using NUnit.Framework;
using System;
using UnityEngine;

namespace KarmanNet.Tests {
    public class PacketTests {
        private class ExamplePacket : Packet {
            private readonly int integer;
            private readonly int[] integerArray;
            private readonly bool boolean;

            private readonly Guid guid;
            private readonly Guid[] guidArray;
            private readonly Vector2 vector2;

            private readonly Vector3 vector3;
            private readonly Quaternion quaternion;
            private readonly Color color;

            private readonly string text;
            private readonly string textRaw;

            public ExamplePacket(byte[] bytes): base(bytes) {
                integer = ReadInt();
                integerArray = ReadIntArray();
                boolean = ReadBoolean();

                guid = ReadGuid();
                guidArray = ReadGuidArray();
                vector2 = ReadVector2();

                vector3 = ReadVector3();
                quaternion = ReadQuaternion();
                color = ReadColor();

                text = ReadString();
                textRaw = ReadRawString();
            }

            public ExamplePacket(
                int integer,
                int[] integerArray,
                bool boolean,

                Guid guid,
                Guid[] guidArray,
                Vector2 vector2,

                Vector3 vector3,
                Quaternion quaternion,
                Color color,

                string text,
                string textRaw
            ): base(Bytes.Pack(
                Bytes.Of(integer),
                Bytes.Of(integerArray),
                Bytes.Of(boolean),

                Bytes.Of(guid),
                Bytes.Of(guidArray),
                Bytes.Of(vector2),

                Bytes.Of(vector3),
                Bytes.Of(quaternion),
                Bytes.Of(color),

                Bytes.Of(text),
                Bytes.Of(textRaw, Bytes.StringMode.RAW)
            )) {
                this.integer = integer;
                this.integerArray = integerArray;
                this.boolean = boolean;

                this.guid = guid;
                this.guidArray = guidArray;
                this.vector2 = vector2;

                this.vector3 = vector3;
                this.quaternion = quaternion;
                this.color = color;

                this.text = text;
                this.textRaw = textRaw;
            }

            public override bool IsValid() {
                return true;
            }

            public int GetInt() {
                return integer;
            }

            public int[] GetIntArray() {
                return integerArray;
            }

            public bool GetBoolean() {
                return boolean;
            }

            public Guid GetGuid() {
                return guid;
            }

            public Guid[] GetGuidArray() {
                return guidArray;
            }

            public Vector2 GetVector2() {
                return vector2;
            }

            public Vector3 GetVector3() {
                return vector3;
            }

            public Quaternion GetQuaternion() {
                return quaternion;
            }

            public Color GetColor() {
                return color;
            }

            public string GetText() {
                return text;
            }

            public string GetTextRaw() {
                return textRaw;
            }
        }

        [Test]
        public void PacketShouldBeReadableCorrectly() {
            ExamplePacket examplePacket = new ExamplePacket(
                1337,
                new[] { 420, 69, 11 },
                true,
                
                Guid.NewGuid(),
                new[] { Guid.NewGuid(), Guid.NewGuid(),Guid.NewGuid() },
                new Vector2(2.3f, 11.1f),
                
                new Vector3(35f, -12f, 65f),
                new Quaternion(-0.1f, 3.2f, 129.5f, 2f),
                new Color(23.0f, 12.7f, 98.34f, 56f),
                
                "Hello, World!",
                "Hello, Raw World!"
            );
            ExamplePacket examplePacketFromBytes = new ExamplePacket(examplePacket.GetBytes());
            Assert.AreEqual(examplePacket.GetInt(), examplePacketFromBytes.GetInt());
            Assert.AreEqual(examplePacket.GetIntArray(), examplePacketFromBytes.GetIntArray());
            Assert.AreEqual(examplePacket.GetBoolean(), examplePacketFromBytes.GetBoolean());

            Assert.AreEqual(examplePacket.GetGuid(), examplePacketFromBytes.GetGuid());
            Assert.AreEqual(examplePacket.GetGuidArray(), examplePacketFromBytes.GetGuidArray());
            Assert.AreEqual(examplePacket.GetVector2(), examplePacketFromBytes.GetVector2());

            Assert.AreEqual(examplePacket.GetVector3(), examplePacketFromBytes.GetVector3());
            Assert.AreEqual(examplePacket.GetQuaternion(), examplePacketFromBytes.GetQuaternion());
            Assert.AreEqual(examplePacket.GetColor(), examplePacketFromBytes.GetColor());

            Assert.AreEqual(examplePacket.GetText(), examplePacketFromBytes.GetText());
            Assert.AreEqual(examplePacket.GetTextRaw(), examplePacketFromBytes.GetTextRaw());
        }

        [Test]
        public void SafeMultipleInvocationsExample() {
            Action<string> example = null;
            int calls = 0;
            example += (string data) => { Debug.Log("First!"); calls++; };
            example += (string data) => { Debug.Log("Second!"); calls++; throw new Exception("Exception in 2"); };
            example += (string data) => { Debug.Log("Third!"); calls++; };
            example += null;
            foreach (Delegate invocation in example.GetInvocationList()) {
                try {
                    (invocation as Action<string>)?.Invoke("abc");
                } catch (Exception e) {
                    Debug.LogWarningFormat("Exception found! {0}", e);
                }
            }
            Assert.AreEqual(3, calls);
        }
    }
}
