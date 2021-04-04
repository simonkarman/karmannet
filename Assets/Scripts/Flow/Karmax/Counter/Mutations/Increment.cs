using KarmanNet.Karmax;
using KarmanNet.Networking;
using System;
using System.Collections.Generic;

namespace KarmaxCounter {
    public class Increment : UpdateMutation<Counter> {
        private readonly int amount;

        public Increment(byte[] bytes) : base(bytes) {
            amount = Bytes.GetInt32(bytes);
        }

        private Increment(int amount) : base(Bytes.Of(amount)) {
            this.amount = amount;
        }

        public static Increment By(int amount) {
            return new Increment(amount);
        }

        public override bool IsValid() {
            return true;
        }

        protected override UpdateResult<Counter> Update(Counter counterFragment, IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey key, Guid requester) {
            return UpdateResult<Counter>.Success(new Counter(counterFragment.GetValue() + amount));
        }
    }
}
