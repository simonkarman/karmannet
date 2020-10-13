using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Networking {
    public static class ByteHelper {
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
            Debug.Log(string.Format("Merged {0} byte array(s) into {1} total bytes", arrays.Length, mergedBytes.Length));
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
    }
}