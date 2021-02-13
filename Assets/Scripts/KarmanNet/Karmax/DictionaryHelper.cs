using System.Collections.Generic;

namespace KarmanNet.Karmax {
    public static class DictionaryHelper {
        public static IReadOnlyDictionary<T, U> CloneWith<T, U>(this IReadOnlyDictionary<T, U> original, T key, U value) {
            Dictionary<T, U> clone = (Dictionary<T, U>) original.CloneWithout(key);
            clone.Add(key, value);
            return clone;
        }

        public static IReadOnlyDictionary<T, U> CloneWithout<T, U>(this IReadOnlyDictionary<T, U> original, T key) {
            Dictionary<T, U> clone = new Dictionary<T, U>();
            foreach (var kvp in original) {
                if (kvp.Key.Equals(key)) {
                    continue;
                }
                clone.Add(kvp.Key, kvp.Value);
            }
            return clone;
        }
    }
}