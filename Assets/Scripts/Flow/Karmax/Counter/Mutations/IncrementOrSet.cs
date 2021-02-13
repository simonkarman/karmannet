using KarmanNet.Karmax;
using KarmanNet.Networking;
using System;

namespace KarmaxCounter {
    public class IncrementOrSet : Mutation.Upsert<Counter> {
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

        protected override Counter Instantiate(Guid requester) {
            return new Counter(amount);
        }

        protected override Counter Mutate(Counter counterFragment, Guid requester) {
            return new Counter(counterFragment.GetValue() + amount);
        }
    }
}
