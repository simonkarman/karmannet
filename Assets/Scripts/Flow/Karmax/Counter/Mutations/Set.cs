using KarmanNet.Karmax;
using KarmanNet.Networking;
using System;

namespace KarmaxCounter {
    public class Set : Mutation.Insert<Counter> {
        private readonly int value;

        public Set(byte[] bytes) : base(bytes) {
            value = Bytes.GetInt32(bytes);
        }

        private Set(int value) : base(Bytes.Of(value)) {
            this.value = value;
        }

        public static Set To(int value) {
            return new Set(value);
        }

        public override bool IsValid() {
            return true;
        }

        protected override Counter Instantiate(Guid requester) {
            return new Counter(value);
        }
    }
}
