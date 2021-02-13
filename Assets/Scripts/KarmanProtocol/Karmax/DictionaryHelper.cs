using System;
using System.Collections.Generic;

namespace KarmanProtocol.Karmax {
    public static class DictionaryHelper {
        public static IReadOnlyDictionary<T, U> CloneWith<T, U>(this IReadOnlyDictionary<T, U> original, T key, U value) {
            Dictionary<T, U> clone = new Dictionary<T, U>();
            foreach (var kvp in original) {
                if (kvp.Key.Equals(key)) {
                    continue;
                }
                clone.Add(kvp.Key, kvp.Value);
            }
            clone.Add(key, value);
            return clone;
        }

        public static bool GetFragmentOrIdentity<T>(this IReadOnlyDictionary<string, Fragment> state, string key, Func<T> identity, out T value) where T : Fragment {
            if (!state.ContainsKey(key)) {
                value = identity();
                return true;
            } else if (state[key] is T t) {
                value = t;
                return true;
            }
            value = null;
            return false;
        }
    }
}