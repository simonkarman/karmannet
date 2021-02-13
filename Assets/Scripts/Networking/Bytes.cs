using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Networking {
    public static class Bytes {
        private static readonly Logger log = new Logger(typeof(Bytes));

        public static byte[] Empty {
            get {
                return new byte[0];
            }
        }

        #region Packing
        public static byte[] SplittablePack(params byte[][] arrays) {
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
            log.Trace("Packed {0} byte array(s) into a splittable array with a total length of {1} bytes", arrays.Length, mergedBytes.Length);
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
            log.Trace("Splitted {0} total bytes into {1} byte array(s)", mergedBytes.Length, arrays.Count);
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
            log.Trace("Packed {0} byte array(s) into an array with a total length of {1} bytes", arrays.Length, mergedBytes.Length);
            return mergedBytes;
        }
        #endregion

        #region Boolean
        public static byte[] Of(bool boolean) {
            return BitConverter.GetBytes(boolean);
        }

        public static bool GetBoolean(byte[] bytes, int startIndex = 0) {
            return BitConverter.ToBoolean(bytes, startIndex);
        }
        #endregion

        #region Integer
        public static byte[] Of(int integer) {
            return BitConverter.GetBytes(integer);
        }

        public static int GetInt32(byte[] bytes, int startIndex = 0) {
            return BitConverter.ToInt32(bytes, startIndex);
        }

        public static byte[] Of(int[] intArray) {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(Of(intArray.Length));
            foreach (var integer in intArray) {
                bytes.AddRange(Of(integer));
            }
            return bytes.ToArray();
        }

        public static int[] GetInt32Array(byte[] bytes, int startIndex = 0) {
            int length = GetInt32(bytes, startIndex);
            int[] intArray = new int[length];
            for (int i = 0; i < length; i++) {
                intArray[i] = GetInt32(bytes, startIndex + (i * 4) + 4);
            }
            return intArray;
        }
        #endregion

        #region Float
        public static byte[] Of(float value) {
            return BitConverter.GetBytes(value);
        }

        public static float GetFloat(byte[] bytes, int startIndex = 0) {
            return BitConverter.ToSingle(bytes, startIndex);
        }
        #endregion

        #region String
        public enum StringMode {
            RAW = 0,
            LENGTH_PREFIXED = 1,
        }

        public static byte[] Of(string value, StringMode mode = StringMode.LENGTH_PREFIXED) {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            if (mode == StringMode.RAW) {
                return bytes;
            }
            return SplittablePack(bytes);
        }

        public static string GetString(byte[] bytes, out int count, int startIndex = 0) {
            int length = GetInt32(bytes, startIndex);
            count = 4 + length;
            return GetRawString(bytes, startIndex + 4, length);
        }

        public static string GetRawString(byte[] bytes, int startIndex = 0, int count = -1) {
            if (count < 0) {
                count = bytes.Length - startIndex;
            }
            return Encoding.UTF8.GetString(bytes, startIndex, count);
        }
        #endregion

        #region Guid
        public static byte[] Of(Guid guid) {
            return guid.ToByteArray();
        }

        public static Guid GetGuid(byte[] bytes, int startIndex = 0) {
            byte[] guidBytes = new byte[16];
            Array.Copy(bytes, startIndex, guidBytes, 0, 16);
            return new Guid(guidBytes);
        }

        public static byte[] Of(Guid[] guidArray) {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(Of(guidArray.Length));
            foreach (var guid in guidArray) {
                bytes.AddRange(Of(guid));
            }
            return bytes.ToArray();
        }

        public static Guid[] GetGuidArray(byte[] bytes, int startIndex = 0) {
            int length = GetInt32(bytes, startIndex);
            Guid[] guidArray = new Guid[length];
            for (int i = 0; i < length; i++) {
                guidArray[i] = GetGuid(bytes, startIndex + (i * 16) + 4);
            }
            return guidArray;
        }
        #endregion

        #region Vector2
        public static byte[] Of(UnityEngine.Vector2 vector2) {
            byte[] bytes = new byte[8];
            Array.Copy(BitConverter.GetBytes(vector2.x), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(vector2.y), 0, bytes, 4, 4);
            return bytes;
        }

        public static UnityEngine.Vector2 GetVector2(byte[] bytes, int startIndex = 0) {
            float x = BitConverter.ToSingle(bytes, startIndex);
            float y = BitConverter.ToSingle(bytes, startIndex + 4);
            return new UnityEngine.Vector2(x, y);
        }
        #endregion

        #region Vector3
        public static byte[] Of(UnityEngine.Vector3 vector3) {
            byte[] bytes = new byte[12];
            Array.Copy(BitConverter.GetBytes(vector3.x), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(vector3.y), 0, bytes, 4, 4);
            Array.Copy(BitConverter.GetBytes(vector3.z), 0, bytes, 8, 4);
            return bytes;
        }

        public static UnityEngine.Vector3 GetVector3(byte[] bytes, int startIndex = 0) {
            float x = BitConverter.ToSingle(bytes, startIndex);
            float y = BitConverter.ToSingle(bytes, startIndex + 4);
            float z = BitConverter.ToSingle(bytes, startIndex + 8);
            return new UnityEngine.Vector3(x, y, z);
        }
        #endregion

        #region Quaternion
        public static byte[] Of(UnityEngine.Quaternion quaternion) {
            byte[] bytes = new byte[16];
            Array.Copy(BitConverter.GetBytes(quaternion.x), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(quaternion.y), 0, bytes, 4, 4);
            Array.Copy(BitConverter.GetBytes(quaternion.z), 0, bytes, 8, 4);
            Array.Copy(BitConverter.GetBytes(quaternion.w), 0, bytes, 12, 4);
            return bytes;
        }

        public static UnityEngine.Quaternion GetQuaternion(byte[] bytes, int startIndex = 0) {
            float x = BitConverter.ToSingle(bytes, startIndex + 0);
            float y = BitConverter.ToSingle(bytes, startIndex + 4);
            float z = BitConverter.ToSingle(bytes, startIndex + 8);
            float w = BitConverter.ToSingle(bytes, startIndex + 12);
            return new UnityEngine.Quaternion(x, y, z, w);
        }
        #endregion

        #region Color
        public static byte[] Of(UnityEngine.Color color) {
            byte[] bytes = new byte[16];
            Array.Copy(BitConverter.GetBytes(color.r), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(color.g), 0, bytes, 4, 4);
            Array.Copy(BitConverter.GetBytes(color.b), 0, bytes, 8, 4);
            Array.Copy(BitConverter.GetBytes(color.a), 0, bytes, 12, 4);
            return bytes;
        }

        public static UnityEngine.Color GetColor(byte[] bytes, int startIndex = 0) {
            float r = BitConverter.ToSingle(bytes, startIndex + 0);
            float g = BitConverter.ToSingle(bytes, startIndex + 4);
            float b = BitConverter.ToSingle(bytes, startIndex + 8);
            float a = BitConverter.ToSingle(bytes, startIndex + 12);
            return new UnityEngine.Color(r, g, b, a);
        }
        #endregion
    }
}
