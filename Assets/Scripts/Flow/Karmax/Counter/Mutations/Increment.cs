using KarmanNet.Karmax;
using KarmanNet.Networking;
using System;

namespace KarmaxCounter {
    public class Increment : Mutation.Update<Counter> {
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

        protected override Counter Mutate(Counter counterFragment, Guid requester) {
            return new Counter(counterFragment.GetValue() + amount);
        }
    }
}
