using KarmanNet.Karmax;
using KarmanNet.Networking;
using System;
using System.Collections.Generic;

namespace KarmaxCounter {
    public class IncrementOrSet : UpsertMutation<Counter> {
        private readonly int amount;

        public IncrementOrSet(byte[] bytes) : base(bytes) {
            amount = Bytes.GetInt32(bytes);
        }

        private IncrementOrSet(int amount) : base(Bytes.Of(amount)) {
            this.amount = amount;
        }

        public static IncrementOrSet At(int amount) {
            return new IncrementOrSet(amount);
        }

        public override bool IsValid() {
            return true;
        }

        protected override InsertResult<Counter> Insert(IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey key, Guid requester) {
            return InsertResult<Counter>.Success(new Counter(amount));
        }

        protected override UpdateResult<Counter> Update(Counter counterFragment, IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey key, Guid requester) {
            return UpdateResult<Counter>.Success(new Counter(counterFragment.GetValue() + amount));
        }
    }
}
