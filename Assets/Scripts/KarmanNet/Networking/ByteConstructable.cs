using System;

namespace KarmanNet.Networking {
    public abstract class ByteConstructable {
        private int marker = 0;
        private readonly byte[] bytes;

        protected ByteConstructable(byte[] bytes) {
            this.bytes = bytes;
        }

        public byte[] GetBytes() {
            return bytes;
        }

        public abstract bool IsValid();

        protected bool IsDone() {
            return marker >= bytes.Length;
        }

        protected bool ReadBoolean() {
            bool value = Bytes.GetBoolean(bytes, marker);
            marker += 1;
            return value;
        }

        protected int ReadInt() {
            int value = Bytes.GetInt32(bytes, marker);
            marker += 4;
            return value;
        }

        protected int[] ReadIntArray() {
            int[] value = Bytes.GetInt32Array(bytes, marker);
            marker += (value.Length * 4) + 4;
            return value;
        }

        protected float ReadFloat() {
            float value = Bytes.GetFloat(bytes, marker);
            marker += 4;
            return value;
        }

        protected string ReadString() {
            string value = Bytes.GetString(bytes, out int count, marker);
            marker += count;
            return value;
        }

        protected string ReadRawString(int count = -1) {
            string value = Bytes.GetRawString(bytes, marker, count);
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

        protected Guid[] ReadGuidArray() {
            Guid[] value = Bytes.GetGuidArray(bytes, marker);
            marker += (value.Length * 16) + 4;
            return value;
        }

        protected byte[] ReadRestBytes() {
            int numberOfRestBytes = bytes.Length - marker;
            byte[] restBytes = new byte[numberOfRestBytes]; 
            Array.Copy(bytes, marker, restBytes, 0, numberOfRestBytes);
            return restBytes;
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
