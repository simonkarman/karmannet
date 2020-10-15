﻿using Networking;
using NUnit.Framework;
using System;
using UnityEngine;

namespace Tests {
    public class PacketTests {
        private class ExamplePacket : Packet {
            private readonly Guid guid;
            private readonly Vector2 vector2;
            private readonly Vector3 vector3;
            private readonly Quaternion quaternion;
            private readonly Color color;
            private readonly string text;

            public ExamplePacket(byte[] bytes): base(bytes) {
                guid = ReadGuid();
                vector2 = ReadVector2();
                vector3 = ReadVector3();
                quaternion = ReadQuaternion();
                color = ReadColor();
                text = ReadString();
            }

            public ExamplePacket(Guid guid, Vector2 vector2, Vector3 vector3, Quaternion quaternion, Color color, string text): base(
                Bytes.Pack(Bytes.Of(guid), Bytes.Of(vector2), Bytes.Of(vector3), Bytes.Of(quaternion), Bytes.Of(color), Bytes.Of(text))
            ) {
                this.guid = guid;
                this.vector2 = vector2;
                this.vector3 = vector3;
                this.quaternion = quaternion;
                this.color = color;
                this.text = text;
            }

            public override void Validate() { }

            public Guid GetGuid() {
                return guid;
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
        }

        [Test]
        public void PacketShouldBeReadableCorrectly() {
            ExamplePacket examplePacket = new ExamplePacket(
                Guid.NewGuid(), new Vector2(2.3f, 11.1f), new Vector3(35f, -12f, 65f), new Quaternion(-0.1f, 3.2f, 129.5f, 2f),new Color(23.0f, 12.7f, 98.34f, 56f), "Hello, World!"
            );
            ExamplePacket examplePacketFromBytes = new ExamplePacket(examplePacket.GetBytes());
            Assert.AreEqual(examplePacket.GetGuid(), examplePacketFromBytes.GetGuid());
            Assert.AreEqual(examplePacket.GetVector2(), examplePacketFromBytes.GetVector2());
            Assert.AreEqual(examplePacket.GetVector3(), examplePacketFromBytes.GetVector3());
            Assert.AreEqual(examplePacket.GetQuaternion(), examplePacketFromBytes.GetQuaternion());
            Assert.AreEqual(examplePacket.GetColor(), examplePacketFromBytes.GetColor());
            Assert.AreEqual(examplePacket.GetText(), examplePacketFromBytes.GetText());
        }
    }
}
