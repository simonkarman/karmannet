using KarmanNet.Karmax;
using KarmanNet.Networking;
using System;
using System.Collections.Generic;

namespace KarmaxCounter {
    public class Set : InsertMutation<Counter> {
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

        protected override InsertResult<Counter> Insert(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester) {
            return InsertResult<Counter>.Success(new Counter(value));
        }
    }
}
