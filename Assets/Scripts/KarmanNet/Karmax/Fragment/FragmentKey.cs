using KarmanNet.Networking;
using System;

namespace KarmanNet.Karmax {
    public abstract class FragmentKey : ByteConstructable, IEquatable<FragmentKey> {
        protected FragmentKey(byte[] bytes) : base(bytes) { }
        public abstract string AsString();

        public override bool Equals(object _other) {
            return _other is FragmentKey other && this == other;
        }

        public bool Equals(FragmentKey other) {
            return this == other;
        }

        public static bool operator ==(FragmentKey x, FragmentKey y) {
            return x.AsString() == y.AsString();
        }

        public static bool operator !=(FragmentKey x, FragmentKey y) {
            return !(x == y);
        }

        public override int GetHashCode() {
            return AsString().GetHashCode();
        }
    }
}