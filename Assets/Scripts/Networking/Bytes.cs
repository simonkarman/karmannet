using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Networking {
    public static class Bytes {
        public static byte[] Merge(params byte[][] arrays) {
            int byteLength = arrays.Length * 4 + arrays.Sum(array => array.Length);
            byte[] mergedBytes = new byte[byteLength];
            int currentIndex = 0;
            for (int i = 0; i < arrays.Length; i++) {
                byte[] array = arrays[i];
                byte[] prefix = BitConverter.GetBytes(array.Length);
                Array.Copy(prefix, 0, mergedBytes, currentIndex, 4);
                Array.Copy(array, 0, mergedBytes, currentIndex + 4, array.Length);
                currentIndex += 4 + array.Length;
            }
            //Debug.Log(string.Format("Merged {0} byte array(s) into on array with a total length of {1} bytes", arrays.Length, mergedBytes.Length));
            return mergedBytes;
        }

        public static byte[][] Split(byte[] mergedBytes) {
            List<byte[]> arrays = new List<byte[]>();
            int currentIndex = 0;
            while (currentIndex < mergedBytes.Length) {
                int arrayLength = BitConverter.ToInt32(mergedBytes, currentIndex);
                byte[] array = new byte[arrayLength];
                Array.Copy(mergedBytes, currentIndex + 4, array, 0, arrayLength);
                arrays.Add(array);
                currentIndex += 4 + arrayLength;
            }
            Debug.Log(string.Format("Splitted {0} total bytes into {1} byte array(s)", mergedBytes.Length, arrays.Count));
            return arrays.ToArray();
        }

        public static byte[] Pack(params byte[][] arrays) {
            int totalByteLength = arrays.Sum(array => array.Length);
            byte[] mergedBytes = new byte[totalByteLength];
            int currentIndex = 0;
            for (int i = 0; i < arrays.Length; i++) {
                Array.Copy(arrays[i], 0, mergedBytes, currentIndex, arrays[i].Length);
                currentIndex += arrays[i].Length;
            }
            //Debug.Log(string.Format("Packed {0} byte array(s) into on array with a total length of {1} bytes", arrays.Length, mergedBytes.Length));
            return mergedBytes;
        }

        public static byte[] Empty {
            get {
                return new byte[0];
            }
        }

        public static byte[] Of(string text) {
            return Encoding.UTF8.GetBytes(text);
        }

        public static byte[] Of(Guid guid) {
            return guid.ToByteArray();
        }

        public static byte[] Of(Vector2 location) {
            byte[] bytes = new byte[8];
            Array.Copy(BitConverter.GetBytes(location.x), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(location.y), 0, bytes, 4, 4);
            return bytes;
        }

        public static byte[] Of(Vector3 location) {
            byte[] bytes = new byte[12];
            Array.Copy(BitConverter.GetBytes(location.x), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(location.y), 0, bytes, 4, 4);
            Array.Copy(BitConverter.GetBytes(location.z), 0, bytes, 8, 4);
            return bytes;
        }

        public static byte[] Of(Quaternion quaternion) {
            byte[] bytes = new byte[16];
            Array.Copy(BitConverter.GetBytes(quaternion.x), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(quaternion.y), 0, bytes, 4, 4);
            Array.Copy(BitConverter.GetBytes(quaternion.z), 0, bytes, 8, 4);
            Array.Copy(BitConverter.GetBytes(quaternion.w), 0, bytes, 12, 4);
            return bytes;
        }

        public static byte[] Of(Color color) {
            byte[] bytes = new byte[16];
            Array.Copy(BitConverter.GetBytes(color.r), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(color.g), 0, bytes, 4, 4);
            Array.Copy(BitConverter.GetBytes(color.b), 0, bytes, 8, 4);
            Array.Copy(BitConverter.GetBytes(color.a), 0, bytes, 12, 4);
            return bytes;
        }

        public static string GetString(byte[] bytes, int startIndex = 0, int count = -1) {
            if (count < 0) {
                count = bytes.Length - startIndex;
            }
            return Encoding.UTF8.GetString(bytes, startIndex, count);
        }

        public static Guid GetGuid(byte[] bytes, int startIndex = 0) {
            byte[] guidBytes = new byte[16];
            Array.Copy(bytes, startIndex, guidBytes, 0, 16);
            return new Guid(guidBytes);
        }

        public static Vector2 GetVector2(byte[] bytes, int startIndex = 0) {
            float x = BitConverter.ToSingle(bytes, startIndex);
            float y = BitConverter.ToSingle(bytes, startIndex + 4);
            return new Vector2(x, y);
        }

        public static Vector3 GetVector3(byte[] bytes, int startIndex = 0) {
            float x = BitConverter.ToSingle(bytes, startIndex);
            float y = BitConverter.ToSingle(bytes, startIndex + 4);
            float z = BitConverter.ToSingle(bytes, startIndex + 8);
            return new Vector3(x, y, z);
        }

        public static Quaternion GetQuaternion(byte[] bytes, int startIndex = 0) {
            float x = BitConverter.ToSingle(bytes, startIndex + 0);
            float y = BitConverter.ToSingle(bytes, startIndex + 4);
            float z = BitConverter.ToSingle(bytes, startIndex + 8);
            float w = BitConverter.ToSingle(bytes, startIndex + 12);
            return new Quaternion(x, y, z, w);
        }

        public static Color GetColor(byte[] bytes, int startIndex = 0) {
            float r = BitConverter.ToSingle(bytes, startIndex + 0);
            float g = BitConverter.ToSingle(bytes, startIndex + 4);
            float b = BitConverter.ToSingle(bytes, startIndex + 8);
            float a = BitConverter.ToSingle(bytes, startIndex + 12);
            return new Color(r, g, b, a);
        }
    }
}
