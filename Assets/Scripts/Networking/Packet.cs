﻿using System;

namespace Networking {
    public abstract class Packet {

        private int marker = 0;
        private byte[] bytes;

        protected Packet(byte[] bytes) {
            this.bytes = bytes;
        }

        public byte[] GetBytes() {
            return bytes;
        }

        public abstract void Validate();

        protected void ResetMarker() {
            marker = 0;
        }

        protected void MoveMarker(int amount) {
            marker += amount;
        }

        protected int ReadInt() {
            int value = Bytes.GetInt32(bytes, marker);
            marker += 4;
            return value;
        }

        protected float ReadFloat() {
            float value = Bytes.GetFloat(bytes, marker);
            marker += 4;
            return value;
        }

        protected string ReadString(int count = -1) {
            string value = Bytes.GetString(bytes, marker, count);
            if (count < 0) {
                marker = bytes.Length;
            } else {
                marker += count;
            }
            return value;
        }

        protected Guid ReadGuid() {
            Guid value = Bytes.GetGuid(bytes, marker);
            marker += 16;
            return value;
        }

        protected UnityEngine.Vector2 ReadVector2() {
            UnityEngine.Vector2 value = Bytes.GetVector2(bytes, marker);
            marker += 8;
            return value;
        }

        protected UnityEngine.Vector3 ReadVector3() {
            UnityEngine.Vector3 value = Bytes.GetVector3(bytes, marker);
            marker += 12;
            return value;
        }

        protected UnityEngine.Quaternion ReadQuaternion() {
            UnityEngine.Quaternion value = Bytes.GetQuaternion(bytes, marker);
            marker += 16;
            return value;
        }

        protected UnityEngine.Color ReadColor() {
            UnityEngine.Color value = Bytes.GetColor(bytes, marker);
            marker += 16;
            return value;
        }
    }
}
